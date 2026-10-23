using Leia.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Leia.Application.Interfaces
{
    public interface IMenuItemService
    {
        Task<ResponseModel<List<ResponseMenuItemDto>>> GetByRestaurantAsync(Guid restaurantId);
        Task<ResponseModel<ResponseMenuItemDto>> GetByIdAsync(Guid id);
        Task<ResponseModel<ResponseMenuItemDto>> CreateAsync(Guid ownerId, CreateMenuItemDto dto);
        Task<ResponseModel<ResponseMenuItemDto>> UpdateAsync(Guid ownerId, Guid id, UpdateMenuItemDto dto);
        Task<ResponseModel<bool>> SetAvailabilityAsync(Guid ownerId, Guid id, bool isAvailable);
        Task<ResponseModel<bool>> DeleteAsync(Guid ownerId, Guid id);

        Task<ResponseModel<bool>> SetDeactivatedByAdminAsync(Guid id, bool deactivated);
    }
}
