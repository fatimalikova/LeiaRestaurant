using Leia.Application.Dtos.Common;
using Leia.Application.Dtos.MenuItemDtos;

namespace Leia.Application.Interfaces
{
    public interface IMenuItemService
    {
        Task<List<ResponseMenuItemDto>> GetByRestaurantAsync(Guid restaurantId);
        Task<List<ResponseMenuItemDto>> GetMyAsync(Guid ownerId);
        Task<ResponseMenuItemDto> GetByIdAsync(Guid id);
        Task<PagedResultDto<ResponseMenuSearchItemDto>> SearchAsync(MenuSearchDto dto);
        Task<ResponseMenuItemDto> CreateAsync(Guid ownerId, CreateMenuItemDto dto);
        Task<ResponseMenuItemDto> UpdateAsync(Guid ownerId, Guid id, UpdateMenuItemDto dto);
        Task SetAvailabilityAsync(Guid ownerId, Guid id, bool isAvailable);
        Task DeleteAsync(Guid ownerId, Guid id);
        Task<ResponseMenuItemDto> RestoreAsync(Guid id);
        Task SetDeactivatedByAdminAsync(Guid id, bool deactivated);
    }
}