using Leia.Application.Dtos.DashboardDtos;

namespace Leia.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<RestaurantDashboardDto> GetRestaurantAsync(Guid ownerId, DashboardQueryDto query);
        Task<AdminDashboardDto> GetAdminAsync(DashboardQueryDto query);
    }
}