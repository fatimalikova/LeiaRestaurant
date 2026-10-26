using Leia.Application.Dtos.MenuItemDtos;
using Leia.Domain.Entities;

namespace Leia.Infrastructure.Services
{
    internal static class MenuMapper
    {
        public static ResponseMenuItemDto ToDto(MenuItem i, string categoryName) => new()
        {
            Id = i.Id,
            RestaurantId = i.RestaurantId,
            CategoryId = i.CategoryId,
            CategoryName = categoryName,
            Name = i.Name,
            Description = i.Description,
            Price = i.Price,
            ImageUrl = i.ImageUrl,
            PrepMinutes = i.PrepMinutes,
            IsAvailable = i.IsAvailable,
            IsActive = i.IsActive,
            DeactivatedByAdmin = i.DeactivatedByAdmin
        };
    }
}