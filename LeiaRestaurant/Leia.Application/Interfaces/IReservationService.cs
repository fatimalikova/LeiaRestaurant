using Leia.Application.Dtos.Common;
using Leia.Application.Dtos.ReservationDtos;

namespace Leia.Application.Interfaces
{
    public interface IReservationService
    {
        Task<ResponseAvailabilityDto> GetAvailabilityAsync(AvailabilityQueryDto query);
        Task<ResponseReservationDto> CreateAsync(Guid customerId, CreateReservationDto dto);
        Task<PagedResultDto<ResponseReservationDto>> GetMyAsync(Guid customerId, ReservationFilterDto filter);
        Task<ResponseReservationDto> CancelAsync(Guid customerId, Guid id, CancelReservationDto dto);
        Task<ResponseReservationDto> RescheduleAsync(Guid customerId, Guid id, RescheduleReservationDto dto);
        Task<PagedResultDto<ResponseReservationDto>> GetRestaurantReservationsAsync(Guid ownerId, ReservationFilterDto filter);
        Task<ResponseReservationDto> UpdateStatusAsync(Guid ownerId, Guid id, UpdateReservationStatusDto dto);
        Task<ResponseReservationDto> GetByIdAsync(Guid userId, Guid id);
        Task<PagedResultDto<ResponseReservationDto>> GetAllForAdminAsync(ReservationFilterDto filter);
        Task<ReservationSettingsDto> GetSettingsAsync(Guid ownerId);
        Task<ReservationSettingsDto> UpdateSettingsAsync(Guid ownerId, ReservationSettingsDto dto);
    }
}