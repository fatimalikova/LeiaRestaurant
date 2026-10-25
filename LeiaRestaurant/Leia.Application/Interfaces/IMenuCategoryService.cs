using Leia.Application.Dtos.MenuCategoryDtos;

namespace Leia.Application.Interfaces
{
    public interface IMenuCategoryService
    {
        Task<List<ResponseMenuCategoryDto>> GetByRestaurantAsync(Guid restaurantId);
        Task<List<ResponseMenuCategoryDto>> GetMyAsync(Guid ownerId);
        Task<ResponseMenuCategoryDto> CreateAsync(Guid ownerId, CreateMenuCategoryDto dto);
        Task<ResponseMenuCategoryDto> UpdateAsync(Guid ownerId, Guid id, UpdateMenuCategoryDto dto);
        Task DeleteAsync(Guid ownerId, Guid id);
    }
}