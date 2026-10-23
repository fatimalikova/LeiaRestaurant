using Leia.Application.Dtos.ProfileDtos;
using Leia.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Leia.Application.Interfaces
{
    public interface IProfileService
    {
        Task<ResponseModel<ProfileResponseDto>> GetAsync(Guid userId);
        Task<ResponseModel<ProfileResponseDto>> UpdateAsync(Guid userId, UpdateProfileDto dto);
        Task<ResponseModel<string>> ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
    }
}
