using Leia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Leia.Infrastructure.Data
{
    public class SettingSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var db = serviceProvider.GetRequiredService<AppDbContext>();

            if (await db.SystemSettings.AnyAsync())
                return;

            db.SystemSettings.Add(new SystemSetting());
            await db.SaveChangesAsync();
        }
    }
}