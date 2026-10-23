using Leia.Application.Dtos.UnavailableDayDtos;
using Leia.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Leia.Application.Interfaces
{
    public interface IUnavailableDayService
    {
        Task<ResponseModel<List<ResponseUnavailableDayDto>>> GetByRestaurantAsync(Guid restaurantId);
        Task<ResponseModel<ResponseUnavailableDayDto>> CreateAsync(Guid ownerId, CreateUnavailableDayDto dto);
        Task<ResponseModel<bool>> DeleteAsync(Guid ownerId, Guid id);
    }
}
