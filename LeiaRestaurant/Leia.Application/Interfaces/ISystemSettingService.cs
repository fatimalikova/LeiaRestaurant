using Leia.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Leia.Application.Interfaces
{
    public interface ISystemSettingService
    {
        Task<ResponseModel<ResponseSystemSettingDto>> GetAsync();
        Task<ResponseModel<ResponseSystemSettingDto>> UpdateAsync(UpdateSystemSettingDto dto);
    }
}
