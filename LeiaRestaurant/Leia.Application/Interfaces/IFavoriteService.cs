using Leia.Application.Dtos.Common;
using Leia.Application.Dtos.RestaurantDtos;

namespace Leia.Application.Interfaces
{
    public interface IFavoriteService
    {
        Task<PagedResultDto<RestaurantListDto>> GetMyAsync(Guid userId, PaginationDto dto);
        Task<List<Guid>> GetMyIdsAsync(Guid userId);
        Task AddAsync(Guid userId, Guid restaurantId);
        Task RemoveAsync(Guid userId, Guid restaurantId);
    }
}