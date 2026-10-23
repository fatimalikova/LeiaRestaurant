using Leia.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Leia.Application.Interfaces
{
    public interface ISliderService
    {
        Task<ResponseModel<List<ResponseSliderDto>>> GetActiveAsync();
        Task<ResponseModel<List<ResponseSliderDto>>> GetAllAsync();
        Task<ResponseModel<ResponseSliderDto>> CreateAsync(CreateSliderDto dto);
        Task<ResponseModel<ResponseSliderDto>> UpdateAsync(Guid id, UpdateSliderDto dto);
        Task<ResponseModel<bool>> DeleteAsync(Guid id);
    }
}
