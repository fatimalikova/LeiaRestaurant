using Leia.Application.Dtos.SystemSettingDtos;
using Leia.Application.Interfaces;
using Leia.Domain.Entities;
using Leia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Leia.Infrastructure.Services
{
    public class SystemSettingService(AppDbContext db) : ISystemSettingService
    {
        public async Task<ResponseSystemSettingDto> GetAsync()
        {
            return Map(await GetOrCreateAsync());
        }

        public async Task<ResponseSystemSettingDto> UpdateAsync(UpdateSystemSettingDto dto)
        {
            var setting = await GetOrCreateAsync();

            setting.MinCancellationMinutes = dto.MinCancellationMinutes;
            setting.RestaurantResponseMinutes = dto.RestaurantResponseMinutes;
            setting.RequireRestaurantApproval = dto.RequireRestaurantApproval;
            setting.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return Map(setting);
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

        private static ResponseSystemSettingDto Map(SystemSetting s) => new()
        {
            MinCancellationMinutes = s.MinCancellationMinutes,
            RestaurantResponseMinutes = s.RestaurantResponseMinutes,
            RequireRestaurantApproval = s.RequireRestaurantApproval
        };
    }
}