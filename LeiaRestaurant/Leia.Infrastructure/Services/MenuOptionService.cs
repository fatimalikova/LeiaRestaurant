using AutoMapper;
using Leia.Application.Dtos.AllergenDtos;
using Leia.Application.Dtos.MenuItemDtos;
using Leia.Application.Dtos.ModifierDtos;
using Leia.Application.Dtos.VariantDtos;
using Leia.Application.Exceptions;
using Leia.Application.Interfaces;
using Leia.Domain.Entities;
using Leia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Leia.Infrastructure.Services
{
    public class MenuOptionService(AppDbContext db, IMapper mapper) : IMenuOptionService
    {
        private const int MaxVariants = 10;
        private const int MaxGroups = 10;
        private const int MaxOptions = 30;

        public async Task<ResponseMenuItemDto> AddVariantAsync(Guid ownerId, Guid menuItemId, CreateVariantDto dto)
        {
            var restaurant = await GetRestaurantAsync(ownerId);
            var item = await GetOwnItemAsync(restaurant.Id, menuItemId);
            var name = dto.Name.Trim();

            if (await db.MenuItemVariants.CountAsync(v => v.MenuItemId == item.Id) >= MaxVariants)
                throw new BadRequestException($"An item can have at most {MaxVariants} variants.");

            if (await db.MenuItemVariants.AnyAsync(v => v.MenuItemId == item.Id && v.Name == name))
                throw new ConflictException("A variant with this name already exists.");

            var variant = mapper.Map<MenuItemVariant>(dto);
            variant.MenuItemId = item.Id;
            variant.Name = name;

            db.MenuItemVariants.Add(variant);
            await db.SaveChangesAsync();

            return await LoadDtoAsync(item.Id);
        }

        public async Task<ResponseMenuItemDto> UpdateVariantAsync(Guid ownerId, Guid variantId, UpdateVariantDto dto)
        {
            var restaurant = await GetRestaurantAsync(ownerId);

            var variant = await db.MenuItemVariants
                .FirstOrDefaultAsync(v => v.Id == variantId && v.MenuItem.RestaurantId == restaurant.Id)
                ?? throw new NotFoundException("Variant not found.");

            var name = dto.Name.Trim();

            if (await db.MenuItemVariants.AnyAsync(v => v.MenuItemId == variant.MenuItemId && v.Name == name && v.Id != variantId))
                throw new ConflictException("A variant with this name already exists.");

            mapper.Map(dto, variant);
            variant.Name = name;
            variant.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return await LoadDtoAsync(variant.MenuItemId);
        }

        public async Task<ResponseMenuItemDto> DeleteVariantAsync(Guid ownerId, Guid variantId)
        {
            var restaurant = await GetRestaurantAsync(ownerId);

            var variant = await db.MenuItemVariants
                .FirstOrDefaultAsync(v => v.Id == variantId && v.MenuItem.RestaurantId == restaurant.Id)
                ?? throw new NotFoundException("Variant not found.");

            var itemId = variant.MenuItemId;
            db.MenuItemVariants.Remove(variant);
            await db.SaveChangesAsync();

            return await LoadDtoAsync(itemId);
        }

        public async Task<ResponseMenuItemDto> AddGroupAsync(Guid ownerId, Guid menuItemId, CreateModifierGroupDto dto)
        {
            var restaurant = await GetRestaurantAsync(ownerId);
            var item = await GetOwnItemAsync(restaurant.Id, menuItemId);
            var name = dto.Name.Trim();

            if (await db.ModifierGroups.CountAsync(g => g.MenuItemId == item.Id) >= MaxGroups)
                throw new BadRequestException($"An item can have at most {MaxGroups} option groups.");

            if (await db.ModifierGroups.AnyAsync(g => g.MenuItemId == item.Id && g.Name == name))
                throw new ConflictException("A group with this name already exists.");

            var group = mapper.Map<ModifierGroup>(dto);
            group.MenuItemId = item.Id;
            group.Name = name;

            db.ModifierGroups.Add(group);
            await db.SaveChangesAsync();

            return await LoadDtoAsync(item.Id);
        }

        public async Task<ResponseMenuItemDto> UpdateGroupAsync(Guid ownerId, Guid groupId, UpdateModifierGroupDto dto)
        {
            var restaurant = await GetRestaurantAsync(ownerId);

            var group = await db.ModifierGroups
                .FirstOrDefaultAsync(g => g.Id == groupId && g.MenuItem.RestaurantId == restaurant.Id)
                ?? throw new NotFoundException("Group not found.");

            var name = dto.Name.Trim();

            if (await db.ModifierGroups.AnyAsync(g => g.MenuItemId == group.MenuItemId && g.Name == name && g.Id != groupId))
                throw new ConflictException("A group with this name already exists.");

            mapper.Map(dto, group);
            group.Name = name;
            group.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return await LoadDtoAsync(group.MenuItemId);
        }

        public async Task<ResponseMenuItemDto> DeleteGroupAsync(Guid ownerId, Guid groupId)
        {
            var restaurant = await GetRestaurantAsync(ownerId);

            var group = await db.ModifierGroups
                .FirstOrDefaultAsync(g => g.Id == groupId && g.MenuItem.RestaurantId == restaurant.Id)
                ?? throw new NotFoundException("Group not found.");

            var itemId = group.MenuItemId;
            db.ModifierGroups.Remove(group);
            await db.SaveChangesAsync();

            return await LoadDtoAsync(itemId);
        }

        public async Task<ResponseMenuItemDto> AddOptionAsync(Guid ownerId, Guid groupId, CreateModifierOptionDto dto)
        {
            var restaurant = await GetRestaurantAsync(ownerId);

            var group = await db.ModifierGroups
                .FirstOrDefaultAsync(g => g.Id == groupId && g.MenuItem.RestaurantId == restaurant.Id)
                ?? throw new NotFoundException("Group not found.");

            var name = dto.Name.Trim();

            if (await db.ModifierOptions.CountAsync(o => o.GroupId == groupId) >= MaxOptions)
                throw new BadRequestException($"A group can have at most {MaxOptions} options.");

            if (await db.ModifierOptions.AnyAsync(o => o.GroupId == groupId && o.Name == name))
                throw new ConflictException("An option with this name already exists.");

            var option = mapper.Map<ModifierOption>(dto);
            option.GroupId = groupId;
            option.Name = name;

            db.ModifierOptions.Add(option);
            await db.SaveChangesAsync();

            return await LoadDtoAsync(group.MenuItemId);
        }

        public async Task<ResponseMenuItemDto> UpdateOptionAsync(Guid ownerId, Guid optionId, UpdateModifierOptionDto dto)
        {
            var restaurant = await GetRestaurantAsync(ownerId);

            var option = await db.ModifierOptions
                .Include(o => o.Group)
                .FirstOrDefaultAsync(o => o.Id == optionId && o.Group.MenuItem.RestaurantId == restaurant.Id)
                ?? throw new NotFoundException("Option not found.");

            var name = dto.Name.Trim();

            if (await db.ModifierOptions.AnyAsync(o => o.GroupId == option.GroupId && o.Name == name && o.Id != optionId))
                throw new ConflictException("An option with this name already exists.");

            mapper.Map(dto, option);
            option.Name = name;
            option.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return await LoadDtoAsync(option.Group.MenuItemId);
        }

        public async Task<ResponseMenuItemDto> DeleteOptionAsync(Guid ownerId, Guid optionId)
        {
            var restaurant = await GetRestaurantAsync(ownerId);

            var option = await db.ModifierOptions
                .Include(o => o.Group)
                .FirstOrDefaultAsync(o => o.Id == optionId && o.Group.MenuItem.RestaurantId == restaurant.Id)
                ?? throw new NotFoundException("Option not found.");

            var itemId = option.Group.MenuItemId;
            db.ModifierOptions.Remove(option);
            await db.SaveChangesAsync();

            return await LoadDtoAsync(itemId);
        }

        public async Task<ResponseMenuItemDto> SetAllergensAsync(Guid ownerId, Guid menuItemId, SetMenuItemAllergensDto dto)
        {
            var restaurant = await GetRestaurantAsync(ownerId);
            var item = await GetOwnItemAsync(restaurant.Id, menuItemId);

            var ids = dto.AllergenIds.Distinct().ToList();

            var validCount = await db.Allergens.CountAsync(a => ids.Contains(a.Id) && a.IsActive);
            if (validCount != ids.Count)
                throw new BadRequestException("One or more allergens are invalid.");

            var existing = await db.MenuItemAllergens.Where(a => a.MenuItemId == item.Id).ToListAsync();
            var existingIds = existing.Select(e => e.AllergenId).ToHashSet();

            db.MenuItemAllergens.RemoveRange(existing.Where(e => !ids.Contains(e.AllergenId)));
            db.MenuItemAllergens.AddRange(ids
                .Where(id => !existingIds.Contains(id))
                .Select(id => new MenuItemAllergen { MenuItemId = item.Id, AllergenId = id }));

            await db.SaveChangesAsync();
            return await LoadDtoAsync(item.Id);
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

        private async Task<MenuItem> GetOwnItemAsync(Guid restaurantId, Guid id)
        {
            return await db.MenuItems.FirstOrDefaultAsync(i => i.Id == id && i.RestaurantId == restaurantId)
                ?? throw new NotFoundException("Menu item not found.");
        }
    }
}