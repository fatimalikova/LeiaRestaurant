using Leia.Application.Dtos.ProfileDtos;
using Leia.Application.Exceptions;
using Leia.Application.Interfaces;
using Leia.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Leia.Infrastructure.Services
{
    public class ProfileService(UserManager<ApplicationUser> userManager) : IProfileService
    {
        public async Task<ProfileResponseDto> GetAsync(Guid userId)
        {
            return Map(await GetUserAsync(userId));
        }

        public async Task<ProfileResponseDto> UpdateAsync(Guid userId, UpdateProfileDto dto)
        {
            var user = await GetUserAsync(userId);

            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;
            user.ImageUrl = dto.ImageUrl;

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new BadRequestException(string.Join(" ", result.Errors.Select(e => e.Description)));

            return Map(user);
        }

        public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
        {
            var user = await GetUserAsync(userId);

            var result = await userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
                throw new BadRequestException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        private async Task<ApplicationUser> GetUserAsync(Guid userId)
        {
            return await userManager.FindByIdAsync(userId.ToString())
                ?? throw new NotFoundException("User not found.");
        }

        private static ProfileResponseDto Map(ApplicationUser u) => new()
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email ?? string.Empty,
            PhoneNumber = u.PhoneNumber,
            ImageUrl = u.ImageUrl,
            CreatedAt = u.CreatedAt
        };
    }
}