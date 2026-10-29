using AutoMapper;
using Leia.Application.Dtos.Common;
using Leia.Application.Dtos.CuisineDtos;
using Leia.Application.Dtos.RestaurantDtos;
using Leia.Application.Dtos.RestaurantImageDtos;
using Leia.Application.Exceptions;
using Leia.Application.Helpers;
using Leia.Application.Interfaces;
using Leia.Domain.Entities;
using Leia.Domain.Enums;
using Leia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Leia.Infrastructure.Services
{
    public class RestaurantService(AppDbContext db, INotificationService notificationService, IMapper mapper) : IRestaurantService
    {
        private const int MaxImages = 10;

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

            if (filter.CuisineId is Guid cuisineId)
                query = query.Where(r => r.Cuisines.Any(c => c.CuisineId == cuisineId));

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
                .ProjectToList()
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
            var restaurant = await WithDetails(db.Restaurants)
                .FirstOrDefaultAsync(r => r.Id == id && r.Status == RestaurantStatus.Approved && r.IsActive)
                ?? throw new NotFoundException("Restaurant not found.");

            return await MapAsync(restaurant);
        }

        public async Task<ResponseRestaurantDto> GetMyAsync(Guid ownerId)
        {
            var restaurant = await GetOwnAsync(ownerId);
            return await MapAsync(restaurant);
        }

        public async Task<ResponseRestaurantDto> CreateAsync(Guid ownerId, CreateRestaurantDto dto)
        {
            if (await db.Restaurants.AnyAsync(r => r.OwnerId == ownerId))
                throw new ConflictException("You already have a restaurant profile.");

            var setting = await db.SystemSettings.FirstOrDefaultAsync();
            var requireApproval = setting?.RequireRestaurantApproval ?? true;

            var restaurant = mapper.Map<Restaurant>(dto);
            restaurant.OwnerId = ownerId;
            restaurant.Status = requireApproval ? RestaurantStatus.Pending : RestaurantStatus.Approved;

            db.Restaurants.Add(restaurant);
            await db.SaveChangesAsync();

            return await MapAsync(await GetOwnAsync(ownerId));
        }

        public async Task<ResponseRestaurantDto> UpdateAsync(Guid ownerId, UpdateRestaurantDto dto)
        {
            var restaurant = await GetOwnAsync(ownerId);

            if (restaurant.Status == RestaurantStatus.Blocked)
                throw new ForbiddenException("Your restaurant is blocked. Please contact support.");

            mapper.Map(dto, restaurant);
            restaurant.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return await MapAsync(restaurant);
        }

        public async Task<ResponseRestaurantDto> SetCuisinesAsync(Guid ownerId, SetRestaurantCuisinesDto dto)
        {
            var restaurant = await GetOwnAsync(ownerId);

            var ids = dto.CuisineIds.Distinct().ToList();

            var validCount = await db.Cuisines.CountAsync(c => ids.Contains(c.Id) && c.IsActive);
            if (validCount != ids.Count)
                throw new BadRequestException("One or more cuisines are invalid.");

            var toRemove = restaurant.Cuisines.Where(c => !ids.Contains(c.CuisineId)).ToList();
            var existingIds = restaurant.Cuisines.Select(c => c.CuisineId).ToHashSet();

            db.RestaurantCuisines.RemoveRange(toRemove);
            db.RestaurantCuisines.AddRange(ids
                .Where(id => !existingIds.Contains(id))
                .Select(id => new RestaurantCuisine { RestaurantId = restaurant.Id, CuisineId = id }));

            await db.SaveChangesAsync();

            return await MapAsync(await GetOwnAsync(ownerId, asNoTracking: true));
        }

        public async Task<ResponseRestaurantDto> AddImageAsync(Guid ownerId, AddRestaurantImageDto dto)
        {
            var restaurant = await GetOwnAsync(ownerId);

            if (restaurant.Images.Count >= MaxImages)
                throw new BadRequestException($"A restaurant can have at most {MaxImages} images.");

            var image = mapper.Map<RestaurantImage>(dto);
            image.RestaurantId = restaurant.Id;

            db.RestaurantImages.Add(image);
            await db.SaveChangesAsync();

            return await MapAsync(await GetOwnAsync(ownerId, asNoTracking: true));
        }

        public async Task<ResponseRestaurantDto> DeleteImageAsync(Guid ownerId, Guid imageId)
        {
            var restaurant = await GetOwnAsync(ownerId);

            var image = restaurant.Images.FirstOrDefault(i => i.Id == imageId)
                ?? throw new NotFoundException("Image not found.");

            db.RestaurantImages.Remove(image);
            await db.SaveChangesAsync();

            return await MapAsync(await GetOwnAsync(ownerId, asNoTracking: true));
        }

        public async Task<PagedResultDto<ResponseRestaurantDto>> GetAllForAdminAsync(PaginationDto dto, RestaurantStatus? status)
        {
            var query = WithDetails(db.Restaurants.AsNoTracking());

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
            var restaurant = await WithDetails(db.Restaurants)
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

        private static IQueryable<Restaurant> WithDetails(IQueryable<Restaurant> query) => query
            .Include(r => r.WorkingHours)
            .Include(r => r.Images)
            .Include(r => r.Cuisines).ThenInclude(c => c.Cuisine);

        private async Task<Restaurant> GetOwnAsync(Guid ownerId, bool asNoTracking = false)
        {
            var query = asNoTracking ? db.Restaurants.AsNoTracking() : db.Restaurants.AsQueryable();

            return await WithDetails(query).FirstOrDefaultAsync(r => r.OwnerId == ownerId)
                ?? throw new NotFoundException("Restaurant profile not found.");
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

            var dtos = mapper.Map<List<ResponseRestaurantDto>>(restaurants);

            foreach (var d in dtos)
            {
                if (stats.TryGetValue(d.Id, out var st))
                {
                    d.AverageRating = Math.Round(st.Avg, 1);
                    d.ReviewCount = st.Count;
                }

                d.WorkingHours = d.WorkingHours.OrderBy(w => ((int)w.Day + 6) % 7).ToList();
            }

            return dtos;
        }
    }
}