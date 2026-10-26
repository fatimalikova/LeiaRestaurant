using Leia.Application.Dtos.WorkingHourDtos;

namespace Leia.Application.Interfaces
{
    public interface IWorkingHourService
    {
        Task<List<ResponseWorkingHourDto>> GetByRestaurantAsync(Guid restaurantId);
        Task<ResponseWorkingHourDto> CreateAsync(Guid ownerId, CreateWorkingHourDto dto);
        Task<ResponseWorkingHourDto> UpdateAsync(Guid ownerId, Guid id, UpdateWorkingHourDto dto);
        Task DeleteAsync(Guid ownerId, Guid id);
    }
}