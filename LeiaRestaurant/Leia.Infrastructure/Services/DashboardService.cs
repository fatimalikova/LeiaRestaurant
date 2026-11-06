using Leia.Application.Dtos.DashboardDtos;
using Leia.Application.Exceptions;
using Leia.Application.Helpers;
using Leia.Application.Interfaces;
using Leia.Domain.Enums;
using Leia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Leia.Infrastructure.Services
{
    public class DashboardService(AppDbContext db) : IDashboardService
    {
        private const int OffsetHours = 4;

        private static readonly OrderStatus[] ActiveStatuses =
        {
            OrderStatus.Accepted, OrderStatus.Preparing, OrderStatus.Ready, OrderStatus.OutForDelivery
        };

        private record OrderRow(DateTime PlacedAt, OrderStatus Status, decimal Total);

        public async Task<RestaurantDashboardDto> GetRestaurantAsync(Guid ownerId, DashboardQueryDto query)
        {
            var restaurant = await db.Restaurants.AsNoTracking().FirstOrDefaultAsync(r => r.OwnerId == ownerId)
                ?? throw new NotFoundException("Restaurant profile not found.");

            var (from, to, fromUtc, toUtc) = Period(query);

            var visible = db.Orders.AsNoTracking().Where(o => o.RestaurantId == restaurant.Id
                && (o.PaymentMethod == PaymentMethod.Cash
                    || o.PaymentStatus == PaymentStatus.Paid
                    || o.PaymentStatus == PaymentStatus.Refunded));

            var rows = await visible
                .Where(o => o.PlacedAt >= fromUtc && o.PlacedAt < toUtc)
                .Select(o => new OrderRow(o.PlacedAt, o.Status, o.Total))
                .ToListAsync();

            var topItems = await db.OrderItems.AsNoTracking()
                .Where(i => i.Order.RestaurantId == restaurant.Id
                    && i.Order.Status == OrderStatus.Completed
                    && i.Order.PlacedAt >= fromUtc && i.Order.PlacedAt < toUtc)
                .GroupBy(i => i.NameAtOrder)
                .Select(g => new TopItemDto
                {
                    Name = g.Key,
                    Quantity = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.PriceAtOrder * x.Quantity)
                })
                .OrderByDescending(x => x.Quantity)
                .Take(5)
                .ToListAsync();

            var pending = await visible.CountAsync(o => o.Status == OrderStatus.Pending);
            var active = await visible.CountAsync(o => ActiveStatuses.Contains(o.Status));

            var todayLocal = TimeHelper.NowLocal().Date;
            var tomorrowLocal = todayLocal.AddDays(1);

            var pendingReservations = await db.Reservations.CountAsync(r =>
                r.RestaurantId == restaurant.Id && r.Status == ReservationStatus.Pending && r.EndDateTime > TimeHelper.NowLocal());

            var todayReservations = await db.Reservations.CountAsync(r =>
                r.RestaurantId == restaurant.Id
                && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed)
                && r.StartDateTime >= todayLocal && r.StartDateTime < tomorrowLocal);

            var rating = await db.Reviews.AsNoTracking()
                .Where(r => r.RestaurantId == restaurant.Id)
                .GroupBy(r => r.RestaurantId)
                .Select(g => new { Avg = g.Average(x => (double)x.Rating), Count = g.Count() })
                .FirstOrDefaultAsync();

            var counted = rows.Where(r => r.Status != OrderStatus.Cancelled && r.Status != OrderStatus.Rejected).ToList();
            var completed = rows.Where(r => r.Status == OrderStatus.Completed).ToList();
            var revenue = completed.Sum(r => r.Total);

            return new RestaurantDashboardDto
            {
                From = from,
                To = to,
                TotalOrders = counted.Count,
                CompletedOrders = completed.Count,
                CancelledOrders = rows.Count(r => r.Status == OrderStatus.Cancelled || r.Status == OrderStatus.Rejected),
                Revenue = revenue,
                AverageOrderValue = completed.Count == 0 ? 0 : Math.Round(revenue / completed.Count, 2),
                PendingOrders = pending,
                ActiveOrders = active,
                PendingReservations = pendingReservations,
                TodayReservations = todayReservations,
                AverageRating = rating == null ? 0 : Math.Round(rating.Avg, 1),
                ReviewCount = rating?.Count ?? 0,
                OrdersByStatus = rows.GroupBy(r => r.Status).ToDictionary(g => g.Key, g => g.Count()),
                Daily = BuildDaily(from, to, rows),
                TopItems = topItems
            };
        }

        public async Task<AdminDashboardDto> GetAdminAsync(DashboardQueryDto query)
        {
            var (from, to, fromUtc, toUtc) = Period(query);

            var visible = db.Orders.AsNoTracking().Where(o =>
                o.PaymentMethod == PaymentMethod.Cash
                || o.PaymentStatus == PaymentStatus.Paid
                || o.PaymentStatus == PaymentStatus.Refunded);

            var rows = await visible
                .Where(o => o.PlacedAt >= fromUtc && o.PlacedAt < toUtc)
                .Select(o => new OrderRow(o.PlacedAt, o.Status, o.Total))
                .ToListAsync();

            var topRestaurants = await visible
                .Where(o => o.Status == OrderStatus.Completed && o.PlacedAt >= fromUtc && o.PlacedAt < toUtc)
                .GroupBy(o => new { o.RestaurantId, o.Restaurant.Name })
                .Select(g => new TopRestaurantDto
                {
                    RestaurantId = g.Key.RestaurantId,
                    Name = g.Key.Name,
                    Orders = g.Count(),
                    Revenue = g.Sum(x => x.Total)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .ToListAsync();

            var restaurantsByStatus = await db.Restaurants.AsNoTracking()
                .GroupBy(r => r.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var customers = await CountInRoleAsync("Customer");
            var owners = await CountInRoleAsync("RestaurantOwner");

            var startLocal = from;
            var endLocal = to.AddDays(1);
            var reservations = await db.Reservations.CountAsync(r =>
                r.StartDateTime >= startLocal && r.StartDateTime < endLocal);

            var counted = rows.Where(r => r.Status != OrderStatus.Cancelled && r.Status != OrderStatus.Rejected).ToList();
            var completed = rows.Where(r => r.Status == OrderStatus.Completed).ToList();
            var revenue = completed.Sum(r => r.Total);

            return new AdminDashboardDto
            {
                From = from,
                To = to,
                TotalCustomers = customers,
                TotalRestaurantOwners = owners,
                RestaurantsByStatus = restaurantsByStatus.ToDictionary(x => x.Status, x => x.Count),
                TotalOrders = counted.Count,
                CompletedOrders = completed.Count,
                CancelledOrders = rows.Count(r => r.Status == OrderStatus.Cancelled || r.Status == OrderStatus.Rejected),
                Revenue = revenue,
                AverageOrderValue = completed.Count == 0 ? 0 : Math.Round(revenue / completed.Count, 2),
                Reservations = reservations,
                Daily = BuildDaily(from, to, rows),
                TopRestaurants = topRestaurants
            };
        }

        private async Task<int> CountInRoleAsync(string role)
        {
            return await (from ur in db.UserRoles
                          join r in db.Roles on ur.RoleId equals r.Id
                          where r.Name == role
                          select ur.UserId).CountAsync();
        }

        private static (DateTime From, DateTime To, DateTime FromUtc, DateTime ToUtc) Period(DashboardQueryDto query)
        {
            var today = TimeHelper.NowLocal().Date;
            var to = query.To?.Date ?? today;
            var from = query.From?.Date ?? to.AddDays(-6);

            if (from > to)
                throw new BadRequestException("'From' date cannot be after 'To' date.");

            if ((to - from).TotalDays > 92)
                throw new BadRequestException("The period cannot exceed 92 days.");

            return (from, to, from.AddHours(-OffsetHours), to.AddDays(1).AddHours(-OffsetHours));
        }

        private static List<DailyStatDto> BuildDaily(DateTime from, DateTime to, List<OrderRow> rows)
        {
            var byDay = rows
                .GroupBy(r => r.PlacedAt.AddHours(OffsetHours).Date)
                .ToDictionary(g => g.Key, g => g.ToList());

            var result = new List<DailyStatDto>();

            for (var day = from; day <= to; day = day.AddDays(1))
            {
                byDay.TryGetValue(day, out var list);
                list ??= new List<OrderRow>();

                result.Add(new DailyStatDto
                {
                    Date = day,
                    Orders = list.Count(r => r.Status != OrderStatus.Cancelled && r.Status != OrderStatus.Rejected),
                    Revenue = list.Where(r => r.Status == OrderStatus.Completed).Sum(r => r.Total)
                });
            }

            return result;
        }
    }
}