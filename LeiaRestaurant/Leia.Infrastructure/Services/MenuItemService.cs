using AutoMapper;
using Leia.Application.Dtos.MenuItemDtos;
using Leia.Application.Exceptions;
using Leia.Application.Interfaces;
using Leia.Domain.Entities;
using Leia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Leia.Infrastructure.Services
{
    public class MenuItemService(AppDbContext db, IMapper mapper) : IMenuItemService
    {
        public async Task<List<ResponseMenuItemDto>> GetByRestaurantAsync(Guid restaurantId)
        {
            var items = await db.MenuItems.AsNoTracking()
                .Include(i => i.Category)
                .Where(i => i.RestaurantId == restaurantId && i.IsActive && !i.DeactivatedByAdmin && i.Category.IsActive)
                .OrderBy(i => i.Category.SortOrder).ThenBy(i => i.Name)
                .ToListAsync();

            return mapper.Map<List<ResponseMenuItemDto>>(items);
        }

        public async Task<List<ResponseMenuItemDto>> GetMyAsync(Guid ownerId)
        {
            var restaurant = await GetRestaurantAsync(ownerId);

            var items = await db.MenuItems.AsNoTracking()
                .Include(i => i.Category)
                .Where(i => i.RestaurantId == restaurant.Id)
                .OrderBy(i => i.Category.SortOrder).ThenBy(i => i.Name)
                .ToListAsync();

            return mapper.Map<List<ResponseMenuItemDto>>(items);
        }

        public async Task<ResponseMenuItemDto> GetByIdAsync(Guid id)
        {
            var item = await db.MenuItems.AsNoTracking()
                .Include(i => i.Category)
                .FirstOrDefaultAsync(i => i.Id == id && i.IsActive && !i.DeactivatedByAdmin)
                ?? throw new NotFoundException("Menu item not found.");

            return mapper.Map<ResponseMenuItemDto>(item);
        }

        public async Task<ResponseMenuItemDto> CreateAsync(Guid ownerId, CreateMenuItemDto dto)
        {
            var restaurant = await GetRestaurantAsync(ownerId);
            var category = await GetOwnCategoryAsync(restaurant.Id, dto.CategoryId);

            var item = mapper.Map<MenuItem>(dto);
            item.RestaurantId = restaurant.Id;

            db.MenuItems.Add(item);
            await db.SaveChangesAsync();

            var result = mapper.Map<ResponseMenuItemDto>(item);
            result.CategoryName = category.Name;
            return result;
        }

        public async Task<ResponseMenuItemDto> UpdateAsync(Guid ownerId, Guid id, UpdateMenuItemDto dto)
        {
            var restaurant = await GetRestaurantAsync(ownerId);
            var item = await GetOwnItemAsync(restaurant.Id, id);
            var category = await GetOwnCategoryAsync(restaurant.Id, dto.CategoryId);

            mapper.Map(dto, item);
            item.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            var result = mapper.Map<ResponseMenuItemDto>(item);
            result.CategoryName = category.Name;
            return result;
        }

        public async Task SetAvailabilityAsync(Guid ownerId, Guid id, bool isAvailable)
        {
            var restaurant = await GetRestaurantAsync(ownerId);
            var item = await GetOwnItemAsync(restaurant.Id, id);

            item.IsAvailable = isAvailable;
            item.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid ownerId, Guid id)
        {
            var restaurant = await GetRestaurantAsync(ownerId);
            var item = await GetOwnItemAsync(restaurant.Id, id);

            db.MenuItems.Remove(item);
            await db.SaveChangesAsync();
        }

        public async Task SetDeactivatedByAdminAsync(Guid id, bool deactivated)
        {
            var item = await db.MenuItems.FirstOrDefaultAsync(i => i.Id == id)
                ?? throw new NotFoundException("Menu item not found.");

            item.DeactivatedByAdmin = deactivated;
            item.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        private async Task<Restaurant> GetRestaurantAsync(Guid ownerId)
        {
            return await db.Restaurants.FirstOrDefaultAsync(r => r.OwnerId == ownerId)
                ?? throw new NotFoundException("Restaurant profile not found.");
        }

        private async Task<MenuCategory> GetOwnCategoryAsync(Guid restaurantId, Guid categoryId)
        {
            return await db.MenuCategories.FirstOrDefaultAsync(c => c.Id == categoryId && c.RestaurantId == restaurantId)
                ?? throw new NotFoundException("Category not found in your restaurant.");
        }

        private async Task<MenuItem> GetOwnItemAsync(Guid restaurantId, Guid id)
        {
            return await db.MenuItems.FirstOrDefaultAsync(i => i.Id == id && i.RestaurantId == restaurantId)
                ?? throw new NotFoundException("Menu item not found.");
        }
    }
}