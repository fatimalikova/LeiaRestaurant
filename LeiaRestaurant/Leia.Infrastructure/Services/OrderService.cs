using AutoMapper;
using Leia.Application.Dtos.Common;
using Leia.Application.Dtos.OrderDtos;
using Leia.Application.Exceptions;
using Leia.Application.Helpers;
using Leia.Application.Interfaces;
using Leia.Domain.Entities;
using Leia.Domain.Enums;
using Leia.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Leia.Infrastructure.Services
{
    public class OrderService(
        AppDbContext db,
        INotificationService notificationService,
        UserManager<ApplicationUser> userManager,
        IMapper mapper) : IOrderService
    {
        public async Task<ResponseOrderDto> CreateAsync(Guid customerId, CreateOrderDto dto)
        {
            var restaurant = await db.Restaurants
                .Include(r => r.WorkingHours)
                .Include(r => r.UnavailableDays)
                .FirstOrDefaultAsync(r => r.Id == dto.RestaurantId)
                ?? throw new NotFoundException("Restaurant not found.");

            if (restaurant.Status != RestaurantStatus.Approved || !restaurant.IsActive)
                throw new BadRequestException("This restaurant is not currently accepting orders.");

            if (dto.Type == OrderType.Delivery && !restaurant.AcceptsDelivery)
                throw new BadRequestException("This restaurant does not offer delivery.");

            if (dto.Type == OrderType.Pickup && !restaurant.AcceptsPickup)
                throw new BadRequestException("This restaurant does not offer pickup.");

            if (!IsOpenNow(restaurant))
                throw new BadRequestException("This restaurant is closed right now.");

            string? deliveryAddress = null;
            if (dto.Type == OrderType.Delivery)
            {
                if (dto.AddressId.HasValue)
                {
                    var address = await db.UserAddresses.AsNoTracking()
                        .FirstOrDefaultAsync(a => a.Id == dto.AddressId && a.UserId == customerId)
                        ?? throw new NotFoundException("Address not found.");

                    deliveryAddress = address.AddressLine;
                }
                else
                {
                    deliveryAddress = dto.DeliveryAddress;
                }
            }

            var ids = dto.Items.Select(i => i.MenuItemId).Distinct().ToList();
            var menuItems = await db.MenuItems
                .Where(m => ids.Contains(m.Id) && m.RestaurantId == restaurant.Id)
                .ToListAsync();

            if (menuItems.Count != ids.Count)
                throw new BadRequestException("One or more items do not belong to this restaurant.");

            var unavailable = menuItems.FirstOrDefault(m => !m.IsActive || !m.IsAvailable || m.DeactivatedByAdmin);
            if (unavailable != null)
                throw new BadRequestException($"\"{unavailable.Name}\" is currently unavailable.");

            var order = new Order
            {
                OrderNumber = await GenerateOrderNumberAsync(),
                CustomerId = customerId,
                RestaurantId = restaurant.Id,
                Type = dto.Type,
                PaymentMethod = dto.PaymentMethod,
                DeliveryAddress = deliveryAddress,
                CustomerPhone = dto.CustomerPhone,
                Notes = dto.Notes
            };

            foreach (var line in dto.Items)
            {
                var menuItem = menuItems.First(m => m.Id == line.MenuItemId);
                order.Items.Add(new OrderItem
                {
                    MenuItemId = menuItem.Id,
                    NameAtOrder = menuItem.Name,
                    PriceAtOrder = menuItem.Price,
                    Quantity = line.Quantity,
                    Notes = line.Notes
                });
            }

            order.Subtotal = order.Items.Sum(i => i.PriceAtOrder * i.Quantity);

            if (order.Subtotal < restaurant.MinOrderAmount)
                throw new BadRequestException($"Minimum order amount is {restaurant.MinOrderAmount:0.00} AZN.");

            order.DeliveryFee = dto.Type == OrderType.Delivery ? restaurant.DeliveryFee : 0;
            order.Total = order.Subtotal + order.DeliveryFee;

            AddHistory(order, null, OrderStatus.Pending, customerId, null);

            db.Orders.Add(order);
            await db.SaveChangesAsync();

            await notificationService.CreateAsync(
                restaurant.OwnerId, "New order",
                $"New order {order.OrderNumber} - {order.Total:0.00} AZN.",
                NotificationType.NewOrder, order.Id);

            return await GetMappedAsync(order.Id);
        }

        public async Task<PagedResultDto<ResponseOrderDto>> GetMyOrdersAsync(Guid customerId, OrderFilterDto filter)
        {
            return await PageAsync(Query().Where(o => o.CustomerId == customerId), filter);
        }

        public async Task<ResponseOrderDto> CancelAsync(Guid customerId, Guid id, CancelOrderDto dto)
        {
            var order = await db.Orders
                .Include(o => o.Restaurant)
                .FirstOrDefaultAsync(o => o.Id == id)
                ?? throw new NotFoundException("Order not found.");

            if (order.CustomerId != customerId)
                throw new ForbiddenException("You can only cancel your own orders.");

            if (order.Status == OrderStatus.Accepted)
            {
                var setting = await db.SystemSettings.FirstOrDefaultAsync();
                var windowMinutes = setting?.MinCancellationMinutes ?? 5;

                if ((DateTime.UtcNow - order.PlacedAt).TotalMinutes > windowMinutes)
                    throw new BadRequestException(
                        $"The restaurant already accepted this order. It can only be cancelled within {windowMinutes} minutes of placing it.");
            }
            else if (order.Status != OrderStatus.Pending)
            {
                throw new BadRequestException("This order can no longer be cancelled.");
            }

            var previous = order.Status;
            order.Status = OrderStatus.Cancelled;
            order.CancelReason = dto.Reason;
            order.UpdatedAt = DateTime.UtcNow;

            AddHistory(order, previous, OrderStatus.Cancelled, customerId, dto.Reason);
            await db.SaveChangesAsync();

            await notificationService.CreateAsync(
                order.Restaurant.OwnerId, "Order cancelled",
                $"Order {order.OrderNumber} was cancelled by the customer.",
                NotificationType.OrderCancelled, order.Id);

            return await GetMappedAsync(order.Id);
        }

        public async Task<PagedResultDto<ResponseOrderDto>> GetRestaurantOrdersAsync(Guid ownerId, OrderFilterDto filter)
        {
            var restaurant = await GetRestaurantAsync(ownerId);
            return await PageAsync(Query().Where(o => o.RestaurantId == restaurant.Id), filter);
        }

        public async Task<ResponseOrderDto> UpdateStatusAsync(Guid ownerId, Guid id, UpdateOrderStatusDto dto)
        {
            var restaurant = await GetRestaurantAsync(ownerId);

            var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == id && o.RestaurantId == restaurant.Id)
                ?? throw new NotFoundException("Order not found.");

            if (!IsTransitionAllowed(order, dto.Status))
                throw new BadRequestException($"Order cannot move from {order.Status} to {dto.Status}.");

            var previous = order.Status;
            order.Status = dto.Status;
            order.UpdatedAt = DateTime.UtcNow;

            if (dto.Status == OrderStatus.Rejected)
                order.CancelReason = dto.Comment;

            AddHistory(order, previous, dto.Status, ownerId, dto.Comment);
            await db.SaveChangesAsync();

            await notificationService.CreateAsync(
                order.CustomerId, "Order update",
                $"Your order {order.OrderNumber} is now: {dto.Status}.",
                NotificationType.OrderStatusChanged, order.Id);

            return await GetMappedAsync(order.Id);
        }

        public async Task<ResponseOrderDto> GetByIdAsync(Guid userId, Guid id)
        {
            var order = await Query().FirstOrDefaultAsync(o => o.Id == id)
                ?? throw new NotFoundException("Order not found.");

            if (order.CustomerId != userId && order.Restaurant.OwnerId != userId)
            {
                var user = await userManager.FindByIdAsync(userId.ToString());
                if (user == null || !await userManager.IsInRoleAsync(user, "Admin"))
                    throw new ForbiddenException("You are not authorized to view this order.");
            }

            return mapper.Map<ResponseOrderDto>(order);
        }

        public async Task<PagedResultDto<ResponseOrderDto>> GetAllForAdminAsync(OrderFilterDto filter)
        {
            return await PageAsync(Query(), filter);
        }

        private static void AddHistory(Order order, OrderStatus? from, OrderStatus to, Guid userId, string? comment)
        {
            order.History.Add(new OrderStatusHistory
            {
                FromStatus = from,
                ToStatus = to,
                ChangedByUserId = userId,
                Comment = comment
            });
        }

        private IQueryable<Order> Query() => db.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Restaurant)
            .Include(o => o.Items)
            .Include(o => o.History)
            .Include(o => o.Review);

        private async Task<ResponseOrderDto> GetMappedAsync(Guid id)
        {
            var order = await Query().FirstAsync(o => o.Id == id);
            return mapper.Map<ResponseOrderDto>(order);
        }

        private async Task<Restaurant> GetRestaurantAsync(Guid ownerId)
        {
            return await db.Restaurants.FirstOrDefaultAsync(r => r.OwnerId == ownerId)
                ?? throw new NotFoundException("Restaurant profile not found.");
        }

        private async Task<PagedResultDto<ResponseOrderDto>> PageAsync(IQueryable<Order> query, OrderFilterDto f)
        {
            if (f.Status is OrderStatus status)
                query = query.Where(o => o.Status == status);

            if (f.From is DateTime from)
                query = query.Where(o => o.PlacedAt >= from);

            if (f.To is DateTime to)
            {
                var end = to.Date.AddDays(1);
                query = query.Where(o => o.PlacedAt < end);
            }

            var total = await query.CountAsync();
            var orders = await query
                .OrderByDescending(o => o.PlacedAt)
                .Skip((f.Page - 1) * f.PageSize)
                .Take(f.PageSize)
                .ToListAsync();

            return new PagedResultDto<ResponseOrderDto>
            {
                Items = mapper.Map<List<ResponseOrderDto>>(orders),
                TotalCount = total,
                Page = f.Page,
                PageSize = f.PageSize
            };
        }

        private static bool IsTransitionAllowed(Order o, OrderStatus next) => (o.Status, next) switch
        {
            (OrderStatus.Pending, OrderStatus.Accepted) => true,
            (OrderStatus.Pending, OrderStatus.Rejected) => true,
            (OrderStatus.Accepted, OrderStatus.Preparing) => true,
            (OrderStatus.Preparing, OrderStatus.Ready) => true,
            (OrderStatus.Ready, OrderStatus.OutForDelivery) => o.Type == OrderType.Delivery,
            (OrderStatus.Ready, OrderStatus.Completed) => o.Type == OrderType.Pickup,
            (OrderStatus.OutForDelivery, OrderStatus.Completed) => true,
            _ => false
        };

        private static bool IsOpenNow(Restaurant r)
        {
            var local = TimeHelper.NowLocal();

            if (r.UnavailableDays.Any(u => u.Date.Date == local.Date))
                return false;

            return r.WorkingHours.Any(w =>
                w.Day == local.DayOfWeek &&
                w.StartTime <= local.TimeOfDay &&
                w.EndTime > local.TimeOfDay);
        }

        private async Task<string> GenerateOrderNumberAsync()
        {
            for (var attempt = 0; attempt < 5; attempt++)
            {
                var number = $"LEIA-{DateTime.UtcNow:yyMMdd}-{Random.Shared.Next(10000, 99999)}";
                if (!await db.Orders.AnyAsync(o => o.OrderNumber == number))
                    return number;
            }

            throw new InvalidOperationException("Could not generate a unique order number.");
        }
    }
}