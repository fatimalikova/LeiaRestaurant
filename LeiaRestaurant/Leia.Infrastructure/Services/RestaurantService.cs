using Leia.Application.Dtos.Common;
using Leia.Application.Dtos.RestaurantDtos;
using Leia.Application.Dtos.WorkingHourDtos;
using Leia.Application.Exceptions;
using Leia.Application.Helpers;
using Leia.Application.Interfaces;
using Leia.Domain.Entities;
using Leia.Domain.Enums;
using Leia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Leia.Infrastructure.Services
{
    public class RestaurantService(AppDbContext db, INotificationService notificationService) : IRestaurantService
    {
        public async Task<PagedResultDto<RestaurantListDto>> GetAllAsync(RestaurantFilterDto filter)
        {
            var local = TimeHelper.NowLocal();
            var today = local.Date;
            var day = local.DayOfWeek;
            var time = local.TimeOfDay;

            var query = db.Restaurants.AsNoTracking()
                .Where(r => r.Status == RestaurantStatus.Approved && r.IsActive);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim();
                query = query.Where(r => r.Name.Contains(search) || r.Address.Contains(search));
            }

            if (filter.AcceptsDelivery is bool delivery)
                query = query.Where(r => r.AcceptsDelivery == delivery);

            if (filter.AcceptsPickup is bool pickup)
                query = query.Where(r => r.AcceptsPickup == pickup);

            if (filter.MinRating is double minRating)
                query = query.Where(r => (r.Reviews.Average(x => (double?)x.Rating) ?? 0) >= minRating);

            if (filter.OpenNow == true)
                query = query.Where(r =>
                    r.WorkingHours.Any(w => w.Day == day && w.StartTime <= time && w.EndTime > time) &&
                    !r.UnavailableDays.Any(u => u.Date == today));

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.Reviews.Average(x => (double?)x.Rating) ?? 0)
                .ThenBy(r => r.Name)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(r => new RestaurantListDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    LogoUrl = r.LogoUrl,
                    CoverUrl = r.CoverUrl,
                    Address = r.Address,
                    AcceptsDelivery = r.AcceptsDelivery,
                    AcceptsPickup = r.AcceptsPickup,
                    MinOrderAmount = r.MinOrderAmount,
                    DeliveryFee = r.DeliveryFee,
                    AverageRating = r.Reviews.Average(x => (double?)x.Rating) ?? 0,
                    ReviewCount = r.Reviews.Count(),
                    IsOpenNow =
                        r.WorkingHours.Any(w => w.Day == day && w.StartTime <= time && w.EndTime > time) &&
                        !r.UnavailableDays.Any(u => u.Date == today)
                })
                .ToListAsync();

            foreach (var i in items)
                i.AverageRating = Math.Round(i.AverageRating, 1);

            return new PagedResultDto<RestaurantListDto>
            {
                Items = items,
                TotalCount = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<ResponseRestaurantDto> GetByIdAsync(Guid id)
        {
            var restaurant = await db.Restaurants
                .Include(r => r.WorkingHours)
                .FirstOrDefaultAsync(r => r.Id == id && r.Status == RestaurantStatus.Approved && r.IsActive)
                ?? throw new NotFoundException("Restaurant not found.");

            return await MapAsync(restaurant);
        }

        public async Task<ResponseRestaurantDto> GetMyAsync(Guid ownerId)
        {
            var restaurant = await db.Restaurants
                .Include(r => r.WorkingHours)
                .FirstOrDefaultAsync(r => r.OwnerId == ownerId)
                ?? throw new NotFoundException("Restaurant profile not found.");

            return await MapAsync(restaurant);
        }

        public async Task<ResponseRestaurantDto> CreateAsync(Guid ownerId, CreateRestaurantDto dto)
        {
            if (await db.Restaurants.AnyAsync(r => r.OwnerId == ownerId))
                throw new ConflictException("You already have a restaurant profile.");

            var setting = await db.SystemSettings.FirstOrDefaultAsync();
            var requireApproval = setting?.RequireRestaurantApproval ?? true;

            var restaurant = new Restaurant
            {
                OwnerId = ownerId,
                Name = dto.Name,
                Description = dto.Description,
                LogoUrl = dto.LogoUrl,
                CoverUrl = dto.CoverUrl,
                Address = dto.Address,
                PhoneNumber = dto.PhoneNumber,
                ContactEmail = dto.ContactEmail,
                InstagramUrl = dto.InstagramUrl,
                FacebookUrl = dto.FacebookUrl,
                AcceptsDelivery = dto.AcceptsDelivery,
                AcceptsPickup = dto.AcceptsPickup,
                MinOrderAmount = dto.MinOrderAmount,
                DeliveryFee = dto.DeliveryFee,
                Status = requireApproval ? RestaurantStatus.Pending : RestaurantStatus.Approved
            };

            db.Restaurants.Add(restaurant);
            await db.SaveChangesAsync();

            return await MapAsync(restaurant);
        }

        public async Task<ResponseRestaurantDto> UpdateAsync(Guid ownerId, UpdateRestaurantDto dto)
        {
            var restaurant = await db.Restaurants
                .Include(r => r.WorkingHours)
                .FirstOrDefaultAsync(r => r.OwnerId == ownerId)
                ?? throw new NotFoundException("Restaurant profile not found.");

            if (restaurant.Status == RestaurantStatus.Blocked)
                throw new ForbiddenException("Your restaurant is blocked. Please contact support.");

            restaurant.Name = dto.Name;
            restaurant.Description = dto.Description;
            restaurant.LogoUrl = dto.LogoUrl;
            restaurant.CoverUrl = dto.CoverUrl;
            restaurant.Address = dto.Address;
            restaurant.PhoneNumber = dto.PhoneNumber;
            restaurant.ContactEmail = dto.ContactEmail;
            restaurant.InstagramUrl = dto.InstagramUrl;
            restaurant.FacebookUrl = dto.FacebookUrl;
            restaurant.IsActive = dto.IsActive;
            restaurant.AcceptsDelivery = dto.AcceptsDelivery;
            restaurant.AcceptsPickup = dto.AcceptsPickup;
            restaurant.MinOrderAmount = dto.MinOrderAmount;
            restaurant.DeliveryFee = dto.DeliveryFee;
            restaurant.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return await MapAsync(restaurant);
        }

        public async Task<PagedResultDto<ResponseRestaurantDto>> GetAllForAdminAsync(PaginationDto dto, RestaurantStatus? status)
        {
            var query = db.Restaurants.AsNoTracking().Include(r => r.WorkingHours).AsQueryable();

            if (status is RestaurantStatus s)
                query = query.Where(r => r.Status == s);

            var total = await query.CountAsync();
            var restaurants = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((dto.Page - 1) * dto.PageSize)
                .Take(dto.PageSize)
                .ToListAsync();

            return new PagedResultDto<ResponseRestaurantDto>
            {
                Items = await MapManyAsync(restaurants),
                TotalCount = total,
                Page = dto.Page,
                PageSize = dto.PageSize
            };
        }

        public async Task<ResponseRestaurantDto> UpdateStatusAsync(Guid id, UpdateRestaurantStatusDto dto)
        {
            var restaurant = await db.Restaurants
                .Include(r => r.WorkingHours)
                .FirstOrDefaultAsync(r => r.Id == id)
                ?? throw new NotFoundException("Restaurant not found.");

            restaurant.Status = dto.Status;
            restaurant.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            var message = dto.Status switch
            {
                RestaurantStatus.Approved => $"Your restaurant \"{restaurant.Name}\" has been approved. You can now receive orders.",
                RestaurantStatus.Blocked => $"Your restaurant \"{restaurant.Name}\" has been blocked.",
                _ => $"Your restaurant \"{restaurant.Name}\" is under review."
            };

            await notificationService.CreateAsync(
                restaurant.OwnerId, "Restaurant status updated", message,
                NotificationType.System, restaurant.Id);

            return await MapAsync(restaurant);
        }

        

        private async Task<ResponseRestaurantDto> MapAsync(Restaurant restaurant)
        {
            return (await MapManyAsync(new List<Restaurant> { restaurant }))[0];
        }

        private async Task<List<ResponseRestaurantDto>> MapManyAsync(List<Restaurant> restaurants)
        {
            var ids = restaurants.Select(r => r.Id).ToList();

            var stats = await db.Reviews
                .Where(x => ids.Contains(x.RestaurantId))
                .GroupBy(x => x.RestaurantId)
                .Select(g => new { Id = g.Key, Count = g.Count(), Avg = g.Average(x => (double)x.Rating) })
                .ToDictionaryAsync(x => x.Id);

            return restaurants.Select(r =>
            {
                stats.TryGetValue(r.Id, out var st);

                return new ResponseRestaurantDto
                {
                    Id = r.Id,
                    OwnerId = r.OwnerId,
                    Name = r.Name,
                    Description = r.Description,
                    LogoUrl = r.LogoUrl,
                    CoverUrl = r.CoverUrl,
                    Address = r.Address,
                    PhoneNumber = r.PhoneNumber,
                    ContactEmail = r.ContactEmail,
                    InstagramUrl = r.InstagramUrl,
                    FacebookUrl = r.FacebookUrl,
                    Status = r.Status,
                    IsActive = r.IsActive,
                    AcceptsDelivery = r.AcceptsDelivery,
                    AcceptsPickup = r.AcceptsPickup,
                    MinOrderAmount = r.MinOrderAmount,
                    DeliveryFee = r.DeliveryFee,
                    AverageRating = st == null ? 0 : Math.Round(st.Avg, 1),
                    ReviewCount = st?.Count ?? 0,
                    WorkingHours = r.WorkingHours
                        .OrderBy(w => ((int)w.Day + 6) % 7)
                        .Select(w => new ResponseWorkingHourDto
                        {
                            Id = w.Id,
                            Day = w.Day,
                            StartTime = w.StartTime,
                            EndTime = w.EndTime
                        }).ToList()
                };
            }).ToList();
        }
    }
}   