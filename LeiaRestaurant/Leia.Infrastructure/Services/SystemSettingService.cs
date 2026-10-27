using AutoMapper;
using Leia.Application.Dtos.SystemSettingDtos;
using Leia.Application.Interfaces;
using Leia.Domain.Entities;
using Leia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Leia.Infrastructure.Services
{
    public class SystemSettingService(AppDbContext db, IMapper mapper) : ISystemSettingService
    {
        public async Task<ResponseSystemSettingDto> GetAsync()
        {
            var setting = await GetOrCreateAsync();
            return mapper.Map<ResponseSystemSettingDto>(setting);
        }

        public async Task<ResponseSystemSettingDto> UpdateAsync(UpdateSystemSettingDto dto)
        {
            var setting = await GetOrCreateAsync();

            mapper.Map(dto, setting);
            setting.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return mapper.Map<ResponseSystemSettingDto>(setting);
        }

        private async Task<SystemSetting> GetOrCreateAsync()
        {
            var setting = await db.SystemSettings.FirstOrDefaultAsync();
            if (setting != null) return setting;

            setting = new SystemSetting();
            db.SystemSettings.Add(setting);
            await db.SaveChangesAsync();
            return setting;
        }
    }
}