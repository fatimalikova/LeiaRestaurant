using Leia.Application.Dtos.Common;
using Leia.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Leia.Application.Interfaces
{
    public interface IReviewService
    {
        Task<ResponseModel<ResponseReviewDto>> CreateAsync(Guid clientId, CreateReviewDto dto);
        Task<ResponseModel<PagedResultDto<ResponseReviewDto>>> GetByRestaurantAsync(Guid restaurantId, PaginationDto dto);
    }
}
