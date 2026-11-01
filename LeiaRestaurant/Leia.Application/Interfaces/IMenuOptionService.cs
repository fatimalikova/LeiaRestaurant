using Leia.Application.Dtos.AllergenDtos;
using Leia.Application.Dtos.MenuItemDtos;
using Leia.Application.Dtos.ModifierDtos;
using Leia.Application.Dtos.VariantDtos;

namespace Leia.Application.Interfaces
{
    public interface IMenuOptionService
    {
        Task<ResponseMenuItemDto> AddVariantAsync(Guid ownerId, Guid menuItemId, CreateVariantDto dto);
        Task<ResponseMenuItemDto> UpdateVariantAsync(Guid ownerId, Guid variantId, UpdateVariantDto dto);
        Task<ResponseMenuItemDto> DeleteVariantAsync(Guid ownerId, Guid variantId);
        Task<ResponseMenuItemDto> AddGroupAsync(Guid ownerId, Guid menuItemId, CreateModifierGroupDto dto);
        Task<ResponseMenuItemDto> UpdateGroupAsync(Guid ownerId, Guid groupId, UpdateModifierGroupDto dto);
        Task<ResponseMenuItemDto> DeleteGroupAsync(Guid ownerId, Guid groupId);
        Task<ResponseMenuItemDto> AddOptionAsync(Guid ownerId, Guid groupId, CreateModifierOptionDto dto);
        Task<ResponseMenuItemDto> UpdateOptionAsync(Guid ownerId, Guid optionId, UpdateModifierOptionDto dto);
        Task<ResponseMenuItemDto> DeleteOptionAsync(Guid ownerId, Guid optionId);
        Task<ResponseMenuItemDto> SetAllergensAsync(Guid ownerId, Guid menuItemId, SetMenuItemAllergensDto dto);
    }
}