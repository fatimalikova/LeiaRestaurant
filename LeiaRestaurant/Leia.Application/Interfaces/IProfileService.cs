using Leia.Application.Dtos.ProfileDtos;

namespace Leia.Application.Interfaces
{
    public interface IProfileService
    {
        Task<ProfileResponseDto> GetAsync(Guid userId);
        Task<ProfileResponseDto> UpdateAsync(Guid userId, UpdateProfileDto dto);
        Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
    }
}