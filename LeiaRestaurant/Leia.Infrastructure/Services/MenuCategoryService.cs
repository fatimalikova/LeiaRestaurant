using AutoMapper;
using Leia.Application.Dtos.MenuCategoryDtos;
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
            var categories = await db.MenuCategories.AsNoTracking()
                .Include(c => c.MenuItems.Where(i => i.IsActive && !i.DeactivatedByAdmin))
                .Where(c => c.RestaurantId == restaurantId && c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();

            return categories.Select(ToDto).ToList();
        }

        public async Task<List<ResponseMenuCategoryDto>> GetMyAsync(Guid ownerId)
        {
            var restaurant = await GetRestaurantAsync(ownerId);

            var categories = await db.MenuCategories.AsNoTracking()
                .Include(c => c.MenuItems)
                .Where(c => c.RestaurantId == restaurant.Id)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();

            return categories.Select(ToDto).ToList();
        }

        public async Task<ResponseMenuCategoryDto> CreateAsync(Guid ownerId, CreateMenuCategoryDto dto)
        {
            var restaurant = await GetRestaurantAsync(ownerId);

            var category = mapper.Map<MenuCategory>(dto);
            category.RestaurantId = restaurant.Id;

            db.MenuCategories.Add(category);
            await db.SaveChangesAsync();
            return ToDto(category);
        }

        public async Task<ResponseMenuCategoryDto> UpdateAsync(Guid ownerId, Guid id, UpdateMenuCategoryDto dto)
        {
            var restaurant = await GetRestaurantAsync(ownerId);

            var category = await db.MenuCategories
                .Include(c => c.MenuItems)
                .FirstOrDefaultAsync(c => c.Id == id && c.RestaurantId == restaurant.Id)
                ?? throw new NotFoundException("Category not found.");

            mapper.Map(dto, category);
            category.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return ToDto(category);
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

        private ResponseMenuCategoryDto ToDto(MenuCategory category)
        {
            var dto = mapper.Map<ResponseMenuCategoryDto>(category);
            foreach (var item in dto.Items)
                item.CategoryName = dto.Name;
            return dto;
        }
    }
}