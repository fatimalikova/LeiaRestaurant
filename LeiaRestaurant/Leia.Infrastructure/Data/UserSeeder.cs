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
            var adminUserName = config["Admin:UserName"] ?? adminEmail;
            var adminFullName = config["Admin:FullName"] ?? "System Admin";

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
                return;

            if (await userManager.FindByEmailAsync(adminEmail) is not null)
                return;

            var admin = new ApplicationUser
            {
                FullName = adminFullName,
                Email = adminEmail,
                UserName = adminUserName,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, adminPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Admin seed failed: {errors}");
            }

            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}