using Leia.Application.Dtos.Common;
using Leia.Application.Dtos.RestaurantDtos;
using Leia.Application.Exceptions;
using Leia.Application.Interfaces;
using Leia.Domain.Entities;
using Leia.Domain.Enums;
using Leia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Leia.Infrastructure.Services
{
    public class FavoriteService(AppDbContext db) : IFavoriteService
    {
        public async Task<PagedResultDto<RestaurantListDto>> GetMyAsync(Guid userId, PaginationDto dto)
        {
            var query = db.Favorites.AsNoTracking()
                .Where(f => f.UserId == userId
                    && f.Restaurant.Status == RestaurantStatus.Approved
                    && f.Restaurant.IsActive);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(f => f.CreatedAt)
                .Skip((dto.Page - 1) * dto.PageSize)
                .Take(dto.PageSize)
                .Select(f => f.Restaurant)
                .ProjectToList()
                .ToListAsync();

            foreach (var i in items)
                i.AverageRating = Math.Round(i.AverageRating, 1);

            return new PagedResultDto<RestaurantListDto>
            {
                Items = items,
                TotalCount = total,
                Page = dto.Page,
                PageSize = dto.PageSize
            };
        }

        public async Task<List<Guid>> GetMyIdsAsync(Guid userId)
        {
            return await db.Favorites.AsNoTracking()
                .Where(f => f.UserId == userId)
                .Select(f => f.RestaurantId)
                .ToListAsync();
        }

        public async Task AddAsync(Guid userId, Guid restaurantId)
        {
            var exists = await db.Restaurants.AnyAsync(r =>
                r.Id == restaurantId && r.Status == RestaurantStatus.Approved && r.IsActive);

            if (!exists)
                throw new NotFoundException("Restaurant not found.");

            if (await db.Favorites.AnyAsync(f => f.UserId == userId && f.RestaurantId == restaurantId))
                return;

            db.Favorites.Add(new Favorite { UserId = userId, RestaurantId = restaurantId });
            await db.SaveChangesAsync();
        }

        public async Task RemoveAsync(Guid userId, Guid restaurantId)
        {
            var favorite = await db.Favorites.FirstOrDefaultAsync(f => f.UserId == userId && f.RestaurantId == restaurantId);
            if (favorite == null)
                return;

            db.Favorites.Remove(favorite);
            await db.SaveChangesAsync();
        }
    }
}