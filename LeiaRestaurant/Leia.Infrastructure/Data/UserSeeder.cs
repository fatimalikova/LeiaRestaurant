using Leia.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Leia.Infrastructure.Data
{
    public class UserSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var config = serviceProvider.GetRequiredService<IConfiguration>();

            var adminEmail = config["Admin:Email"];
            var adminPassword = config["Admin:Password"];
            var adminFullName = config["Admin:FullName"] ?? "System Admin";
            var adminUserName = config["Admin:UserName"] ?? adminEmail;

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword) || string.IsNullOrWhiteSpace(adminUserName))
                return;

            var existing = await userManager.FindByEmailAsync(adminEmail);
            if (existing is not null)
            {
                if (existing.UserName != adminUserName)
                {
                    existing.UserName = adminUserName;
                    var update = await userManager.UpdateAsync(existing);
                    if (!update.Succeeded)
                        throw new Exception($"Admin update failed: {string.Join(", ", update.Errors.Select(e => e.Description))}");
                }

                if (!await userManager.IsInRoleAsync(existing, "Admin"))
                    await userManager.AddToRoleAsync(existing, "Admin");

                return;
            }

            var admin = new ApplicationUser
            {
                FullName = adminFullName,
                Email = adminEmail,
                UserName = adminUserName,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (!result.Succeeded)
                throw new Exception($"Admin seed failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}