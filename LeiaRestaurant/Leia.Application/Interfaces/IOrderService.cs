using Leia.Application.Dtos.Common;
using Leia.Application.Dtos.OrderDtos;

namespace Leia.Application.Interfaces
{
    public interface IOrderService
    {
        Task<ResponseOrderDto> CreateAsync(Guid customerId, CreateOrderDto dto);
        Task<PagedResultDto<ResponseOrderDto>> GetMyOrdersAsync(Guid customerId, OrderFilterDto filter);
        Task<ResponseOrderDto> CancelAsync(Guid customerId, Guid id, CancelOrderDto dto);
        Task<PagedResultDto<ResponseOrderDto>> GetRestaurantOrdersAsync(Guid ownerId, OrderFilterDto filter);
        Task<ResponseOrderDto> UpdateStatusAsync(Guid ownerId, Guid id, UpdateOrderStatusDto dto);
        Task<ResponseOrderDto> GetByIdAsync(Guid userId, Guid id);
        Task<PagedResultDto<ResponseOrderDto>> GetAllForAdminAsync(OrderFilterDto filter);
    }
}