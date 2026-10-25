using Leia.Application.Dtos.MenuCategoryDtos;
using Leia.Application.Exceptions;
using Leia.Application.Interfaces;
using Leia.Domain.Entities;
using Leia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Leia.Infrastructure.Services
{
    public class MenuCategoryService(AppDbContext db) : IMenuCategoryService
    {
        public async Task<List<ResponseMenuCategoryDto>> GetByRestaurantAsync(Guid restaurantId)
        {
            var categories = await db.MenuCategories.AsNoTracking()
                .Include(c => c.MenuItems.Where(i => i.IsActive && !i.DeactivatedByAdmin))
                .Where(c => c.RestaurantId == restaurantId && c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();

            return categories.Select(Map).ToList();
        }

        public async Task<List<ResponseMenuCategoryDto>> GetMyAsync(Guid ownerId)
        {
            var restaurant = await GetRestaurantAsync(ownerId);

            var categories = await db.MenuCategories.AsNoTracking()
                .Include(c => c.MenuItems)
                .Where(c => c.RestaurantId == restaurant.Id)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();

            return categories.Select(Map).ToList();
        }

        public async Task<ResponseMenuCategoryDto> CreateAsync(Guid ownerId, CreateMenuCategoryDto dto)
        {
            var restaurant = await GetRestaurantAsync(ownerId);

            var category = new MenuCategory
            {
                RestaurantId = restaurant.Id,
                Name = dto.Name,
                SortOrder = dto.SortOrder
            };

            db.MenuCategories.Add(category);
            await db.SaveChangesAsync();
            return Map(category);
        }

        public async Task<ResponseMenuCategoryDto> UpdateAsync(Guid ownerId, Guid id, UpdateMenuCategoryDto dto)
        {
            var restaurant = await GetRestaurantAsync(ownerId);

            var category = await db.MenuCategories
                .Include(c => c.MenuItems)
                .FirstOrDefaultAsync(c => c.Id == id && c.RestaurantId == restaurant.Id)
                ?? throw new NotFoundException("Category not found.");

            category.Name = dto.Name;
            category.SortOrder = dto.SortOrder;
            category.IsActive = dto.IsActive;
            category.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return Map(category);
        }

        public async Task DeleteAsync(Guid ownerId, Guid id)
        {
            var restaurant = await GetRestaurantAsync(ownerId);

            var category = await db.MenuCategories
                .Include(c => c.MenuItems)
                .FirstOrDefaultAsync(c => c.Id == id && c.RestaurantId == restaurant.Id)
                ?? throw new NotFoundException("Category not found.");

            if (category.MenuItems.Count > 0)
                throw new BadRequestException("This category still has items. Move or delete them first.");

            db.MenuCategories.Remove(category);
            await db.SaveChangesAsync();
        }

        private async Task<Restaurant> GetRestaurantAsync(Guid ownerId)
        {
            return await db.Restaurants.FirstOrDefaultAsync(r => r.OwnerId == ownerId)
                ?? throw new NotFoundException("Restaurant profile not found.");
        }

        private static ResponseMenuCategoryDto Map(MenuCategory c) => new()
        {
            Id = c.Id,
            RestaurantId = c.RestaurantId,
            Name = c.Name,
            SortOrder = c.SortOrder,
            IsActive = c.IsActive,
            Items = c.MenuItems.Select(i => MenuMapper.ToDto(i, c.Name)).ToList()
        };
    }
}