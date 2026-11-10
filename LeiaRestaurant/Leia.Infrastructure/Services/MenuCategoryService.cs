using AutoMapper;
using Leia.Application.Dtos.MenuCategoryDtos;
using Leia.Application.Dtos.MenuItemDtos;
using Leia.Application.Exceptions;
using Leia.Application.Interfaces;
using Leia.Domain.Entities;
using Leia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Leia.Infrastructure.Services
{
    public class MenuCategoryService(AppDbContext db, IMapper mapper) : IMenuCategoryService
    {
        public async Task<List<ResponseMenuCategoryDto>> GetByRestaurantAsync(Guid restaurantId)
        {
            return await BuildAsync(restaurantId, true, null);
        }

        public async Task<List<ResponseMenuCategoryDto>> GetMyAsync(Guid ownerId)
        {
            var restaurant = await GetRestaurantAsync(ownerId);
            return await BuildAsync(restaurant.Id, false, null);
        }

        public async Task<ResponseMenuCategoryDto> CreateAsync(Guid ownerId, CreateMenuCategoryDto dto)
        {
            var restaurant = await GetRestaurantAsync(ownerId);

            var category = mapper.Map<MenuCategory>(dto);
            category.RestaurantId = restaurant.Id;

            db.MenuCategories.Add(category);
            await db.SaveChangesAsync();

            return (await BuildAsync(restaurant.Id, false, category.Id))[0];
        }

        public async Task<ResponseMenuCategoryDto> UpdateAsync(Guid ownerId, Guid id, UpdateMenuCategoryDto dto)
        {
            var restaurant = await GetRestaurantAsync(ownerId);

            var category = await db.MenuCategories
                .FirstOrDefaultAsync(c => c.Id == id && c.RestaurantId == restaurant.Id)
                ?? throw new NotFoundException("Category not found.");

            mapper.Map(dto, category);
            category.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return (await BuildAsync(restaurant.Id, false, id))[0];
        }

        public async Task DeleteAsync(Guid ownerId, Guid id)
        {
            var restaurant = await GetRestaurantAsync(ownerId);

            var category = await db.MenuCategories
                .FirstOrDefaultAsync(c => c.Id == id && c.RestaurantId == restaurant.Id)
                ?? throw new NotFoundException("Category not found.");

            if (await db.MenuItems.AnyAsync(i => i.CategoryId == id))
                throw new BadRequestException("This category still has items. Move or delete them first.");

            category.IsDeleted = true;
            category.DeletedAt = DateTime.UtcNow;
            category.IsActive = false;
            category.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
        }

        public async Task<ResponseMenuCategoryDto> RestoreAsync(Guid id)
        {
            var category = await db.MenuCategories.IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted)
                ?? throw new NotFoundException("Deleted category not found.");

            category.IsDeleted = false;
            category.DeletedAt = null;
            category.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return (await BuildAsync(category.RestaurantId, false, id))[0];
        }

        private async Task<List<ResponseMenuCategoryDto>> BuildAsync(Guid restaurantId, bool publicView, Guid? categoryId)
        {
            var categoryQuery = db.MenuCategories.AsNoTracking()
                .Where(c => c.RestaurantId == restaurantId);

            if (publicView)
                categoryQuery = categoryQuery.Where(c => c.IsActive);

            if (categoryId.HasValue)
                categoryQuery = categoryQuery.Where(c => c.Id == categoryId.Value);

            var categories = await categoryQuery.OrderBy(c => c.SortOrder).ToListAsync();

            var itemQuery = db.MenuItems.AsNoTracking().WithOptions()
                .Where(i => i.RestaurantId == restaurantId);

            if (publicView)
                itemQuery = itemQuery.Where(i => i.IsActive && !i.DeactivatedByAdmin);

            if (categoryId.HasValue)
                itemQuery = itemQuery.Where(i => i.CategoryId == categoryId.Value);

            var items = await itemQuery.OrderBy(i => i.Name).ToListAsync();

            var itemsByCategory = items
                .GroupBy(i => i.CategoryId)
                .ToDictionary(g => g.Key, g => mapper.Map<List<ResponseMenuItemDto>>(g.ToList()));

            var result = mapper.Map<List<ResponseMenuCategoryDto>>(categories);

            foreach (var dto in result)
                dto.Items = itemsByCategory.TryGetValue(dto.Id, out var list) ? list : new List<ResponseMenuItemDto>();

            return result;
        }

        private async Task<Restaurant> GetRestaurantAsync(Guid ownerId)
        {
            return await db.Restaurants.FirstOrDefaultAsync(r => r.OwnerId == ownerId)
                ?? throw new NotFoundException("Restaurant profile not found.");
        }
    }
}