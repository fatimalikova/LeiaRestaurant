using Leia.Application.Dtos.UnavailableDayDtos;

namespace Leia.Application.Interfaces
{
    public interface IUnavailableDayService
    {
        Task<List<ResponseUnavailableDayDto>> GetByRestaurantAsync(Guid restaurantId);
        Task<ResponseUnavailableDayDto> CreateAsync(Guid ownerId, CreateUnavailableDayDto dto);
        Task DeleteAsync(Guid ownerId, Guid id);
    }
}