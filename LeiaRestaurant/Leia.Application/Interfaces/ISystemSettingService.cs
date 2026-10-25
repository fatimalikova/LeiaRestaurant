using Leia.Application.Dtos.SystemSettingDtos;

namespace Leia.Application.Interfaces
{
    public interface ISystemSettingService
    {
        Task<ResponseSystemSettingDto> GetAsync();
        Task<ResponseSystemSettingDto> UpdateAsync(UpdateSystemSettingDto dto);
    }
}