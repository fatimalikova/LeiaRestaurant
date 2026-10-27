using AutoMapper;
using Leia.Application.Dtos.ProfileDtos;
using Leia.Application.Exceptions;
using Leia.Application.Interfaces;
using Leia.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Leia.Infrastructure.Services
{
    public class ProfileService(UserManager<ApplicationUser> userManager, IMapper mapper) : IProfileService
    {
        public async Task<ProfileResponseDto> GetAsync(Guid userId)
        {
            var user = await GetUserAsync(userId);
            return mapper.Map<ProfileResponseDto>(user);
        }

        public async Task<ProfileResponseDto> UpdateAsync(Guid userId, UpdateProfileDto dto)
        {
            var user = await GetUserAsync(userId);
            mapper.Map(dto, user);

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new BadRequestException(string.Join(" ", result.Errors.Select(e => e.Description)));

            return mapper.Map<ProfileResponseDto>(user);
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
    }
}