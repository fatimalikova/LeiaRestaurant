using Leia.Application.Dtos.Common;
using Leia.Application.Dtos.OrderDtos;
using Leia.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Leia.Application.Interfaces
{
    public interface IOrderService
    {
        Task<ResponseModel<ResponseOrderDto>> CreateAsync(Guid customerId, CreateOrderDto dto);
        Task<ResponseModel<PagedResultDto<ResponseOrderDto>>> GetMyOrdersAsync(Guid customerId, OrderFilterDto filter);
        Task<ResponseModel<ResponseOrderDto>> CancelAsync(Guid customerId, Guid id, CancelOrderDto dto);

        Task<ResponseModel<PagedResultDto<ResponseOrderDto>>> GetRestaurantOrdersAsync(Guid ownerId, OrderFilterDto filter);
        Task<ResponseModel<ResponseOrderDto>> UpdateStatusAsync(Guid ownerId, Guid id, UpdateOrderStatusDto dto);


        Task<ResponseModel<ResponseOrderDto>> GetByIdAsync(Guid userId, Guid id);


        Task<ResponseModel<PagedResultDto<ResponseOrderDto>>> GetAllForAdminAsync(OrderFilterDto filter);
    }
}
