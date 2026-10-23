using Leia.Application.Dtos.Common;
using Leia.Application.Dtos.RestaurantDtos;
using Leia.Application.Models;
using Leia.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Leia.Application.Interfaces
{
    public interface IRestaurantService
    {
        Task<ResponseModel<PagedResultDto<RestaurantListDto>>> GetAllAsync(RestaurantFilterDto filter);
        Task<ResponseModel<ResponseRestaurantDto>> GetByIdAsync(Guid id);

     
        Task<ResponseModel<ResponseRestaurantDto>> GetMyAsync(Guid ownerId);
        Task<ResponseModel<ResponseRestaurantDto>> CreateAsync(Guid ownerId, CreateRestaurantDto dto);
        Task<ResponseModel<ResponseRestaurantDto>> UpdateAsync(Guid ownerId, UpdateRestaurantDto dto);

        Task<ResponseModel<PagedResultDto<ResponseRestaurantDto>>> GetAllForAdminAsync(PaginationDto dto, RestaurantStatus? status);
        Task<ResponseModel<ResponseRestaurantDto>> UpdateStatusAsync(Guid id, UpdateRestaurantStatusDto dto);
    }
}
