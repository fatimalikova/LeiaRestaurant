using Leia.Application.Dtos.Common;
using Leia.Application.Dtos.CuisineDtos;
using Leia.Application.Dtos.RestaurantDtos;
using Leia.Application.Dtos.RestaurantImageDtos;
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
        Task<ResponseRestaurantDto> SetCuisinesAsync(Guid ownerId, SetRestaurantCuisinesDto dto);
        Task<ResponseRestaurantDto> AddImageAsync(Guid ownerId, AddRestaurantImageDto dto);
        Task<ResponseRestaurantDto> DeleteImageAsync(Guid ownerId, Guid imageId);
        Task<PagedResultDto<ResponseRestaurantDto>> GetAllForAdminAsync(PaginationDto dto, RestaurantStatus? status);
        Task<ResponseRestaurantDto> UpdateStatusAsync(Guid id, UpdateRestaurantStatusDto dto);
    }
}