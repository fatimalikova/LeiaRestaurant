using Leia.Application.Dtos.WorkingHourDtos;
using Leia.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Leia.Application.Interfaces
{
    public interface IWorkingHourService
    {
        Task<ResponseModel<List<ResponseWorkingHourDto>>> GetByRestaurantAsync(Guid restaurantId);
        Task<ResponseModel<ResponseWorkingHourDto>> CreateAsync(Guid ownerId, CreateWorkingHourDto dto);
        Task<ResponseModel<ResponseWorkingHourDto>> UpdateAsync(Guid ownerId, Guid id, UpdateWorkingHourDto dto);
        Task<ResponseModel<bool>> DeleteAsync(Guid ownerId, Guid id);
    }
}
