using AutoMapper;
using Leia.Application.Dtos.Common;
using Leia.Application.Dtos.MenuItemDtos;
using Leia.Application.Exceptions;
using Leia.Application.Interfaces;
using Leia.Domain.Entities;
using Leia.Domain.Enums;
using Leia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Leia.Infrastructure.Services
{
    public class MenuItemService(AppDbContext db, IMapper mapper) : IMenuItemService
    {
        public async Task<List<ResponseMenuItemDto>> GetByRestaurantAsync(Guid restaurantId)
        {
            var items = await db.MenuItems.AsNoTracking()
                .WithOptions()
                .Where(i => i.RestaurantId == restaurantId && i.IsActive && !i.DeactivatedByAdmin && i.Category.IsActive)
                .OrderBy(i => i.Category.SortOrder).ThenBy(i => i.Name)
                .ToListAsync();

            return mapper.Map<List<ResponseMenuItemDto>>(items);
        }

        public async Task<List<ResponseMenuItemDto>> GetMyAsync(Guid ownerId)
        {
            var restaurant = await GetRestaurantAsync(ownerId);

            var items = await db.MenuItems.AsNoTracking()
                .WithOptions()
                .Where(i => i.RestaurantId == restaurant.Id)
                .OrderBy(i => i.Category.SortOrder).ThenBy(i => i.Name)
                .ToListAsync();

            return mapper.Map<List<ResponseMenuItemDto>>(items);
        }

        public async Task<ResponseMenuItemDto> GetByIdAsync(Guid id)
        {
            var item = await db.MenuItems.AsNoTracking()
                .WithOptions()
                .FirstOrDefaultAsync(i => i.Id == id && i.IsActive && !i.DeactivatedByAdmin)
                ?? throw new NotFoundException("Menu item not found.");

            return mapper.Map<ResponseMenuItemDto>(item);
        }
        public async Task<PagedResultDto<ResponseMenuSearchItemDto>> SearchAsync(MenuSearchDto dto)
        {
            var term = dto.Query.Trim();

            var query = db.MenuItems.AsNoTracking()
                .Where(i => i.IsActive && i.IsAvailable && !i.DeactivatedByAdmin
                    && i.Category.IsActive
                    && i.Restaurant.Status == RestaurantStatus.Approved && i.Restaurant.IsActive
                    && (i.Name.Contains(term) || (i.Description != null && i.Description.Contains(term))));

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(i => i.Name)
                .Skip((dto.Page - 1) * dto.PageSize)
                .Take(dto.PageSize)
                .Select(i => new ResponseMenuSearchItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Description = i.Description,
                    Price = i.Price,
                    ImageUrl = i.ImageUrl,
                    CategoryName = i.Category.Name,
                    RestaurantId = i.RestaurantId,
                    RestaurantName = i.Restaurant.Name,
                    RestaurantLogoUrl = i.Restaurant.LogoUrl
                })
                .ToListAsync();

            return new PagedResultDto<ResponseMenuSearchItemDto>
            {
                Items = items,
                TotalCount = total,
                Page = dto.Page,
                PageSize = dto.PageSize
            };
        }
        public async Task<ResponseMenuItemDto> CreateAsync(Guid ownerId, CreateMenuItemDto dto)
        {
            var restaurant = await GetRestaurantAsync(ownerId);
            var category = await GetOwnCategoryAsync(restaurant.Id, dto.CategoryId);

            var item = mapper.Map<MenuItem>(dto);
            item.RestaurantId = restaurant.Id;
            item.CategoryId = category.Id;

            db.MenuItems.Add(item);
            await db.SaveChangesAsync();

            return await LoadDtoAsync(item.Id);
        }

        public async Task<ResponseMenuItemDto> UpdateAsync(Guid ownerId, Guid id, UpdateMenuItemDto dto)
        {
            var restaurant = await GetRestaurantAsync(ownerId);
            var item = await GetOwnItemAsync(restaurant.Id, id);
            await GetOwnCategoryAsync(restaurant.Id, dto.CategoryId);

            mapper.Map(dto, item);
            item.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return await LoadDtoAsync(item.Id);
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

            item.IsDeleted = true;
            item.DeletedAt = DateTime.UtcNow;
            item.IsActive = false;
            item.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
        }

        public async Task<ResponseMenuItemDto> RestoreAsync(Guid id)
        {
            var item = await db.MenuItems.IgnoreQueryFilters()
                .FirstOrDefaultAsync(i => i.Id == id && i.IsDeleted)
                ?? throw new NotFoundException("Deleted menu item not found.");

            if (!await db.MenuCategories.AnyAsync(c => c.Id == item.CategoryId))
                throw new BadRequestException("The category of this item is deleted. Restore the category first.");

            item.IsDeleted = false;
            item.DeletedAt = null;
            item.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return await LoadDtoAsync(item.Id);
        }

        public async Task SetDeactivatedByAdminAsync(Guid id, bool deactivated)
        {
            var item = await db.MenuItems.FirstOrDefaultAsync(i => i.Id == id)
                ?? throw new NotFoundException("Menu item not found.");

            item.DeactivatedByAdmin = deactivated;
            item.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        private async Task<ResponseMenuItemDto> LoadDtoAsync(Guid id)
        {
            var item = await db.MenuItems.AsNoTracking().WithOptions().FirstAsync(i => i.Id == id);
            return mapper.Map<ResponseMenuItemDto>(item);
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