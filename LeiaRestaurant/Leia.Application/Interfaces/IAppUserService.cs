using Leia.Application.Dtos.AppUserDtos;
using Leia.Application.Dtos.Common;
using Leia.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Leia.Application.Interfaces
{
    public interface IAppUserService
    {
        Task<ResponseModel<PagedResultDto<ResponseUserDto>>> GetAllAsync(PaginationDto dto);
        Task<ResponseModel<ResponseUserDto>> GetByIdAsync(Guid id);
        Task<ResponseModel<ResponseUserDto>> CreateAsync(CreateUserDto dto);
        Task<ResponseModel<ResponseUserDto>> UpdateAsync(Guid id, UpdateUserDto dto);
        Task<ResponseModel<bool>> SetActiveAsync(Guid id, bool isActive);
    }
}
