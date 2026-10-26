using Leia.Application.Dtos.Common;
using Leia.Application.Dtos.RestaurantDtos;
using Leia.Domain.Enums;

namespace Leia.Application.Interfaces
{
    public interface IRestaurantService
    {
        Task<PagedResultDto<RestaurantListDto>> GetAllAsync(RestaurantFilterDto filter);
        Task<ResponseRestaurantDto> GetByIdAsync(Guid id);
        Task<ResponseRestaurantDto> GetMyAsync(Guid ownerId);
        Task<ResponseRestaurantDto> CreateAsync(Guid ownerId, CreateRestaurantDto dto);
        Task<ResponseRestaurantDto> UpdateAsync(Guid ownerId, UpdateRestaurantDto dto);
        Task<PagedResultDto<ResponseRestaurantDto>> GetAllForAdminAsync(PaginationDto dto, RestaurantStatus? status);
        Task<ResponseRestaurantDto> UpdateStatusAsync(Guid id, UpdateRestaurantStatusDto dto);
    }
}