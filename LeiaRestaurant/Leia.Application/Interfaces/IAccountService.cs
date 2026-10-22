using Leia.Application.Dtos.AccountDtos;
using Leia.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Leia.Application.Interfaces
{
    public interface IAccountService
    {
        Task<ResponseModel<AuthResponseDto>> RegisterAsync(RegisterDto dto);
        Task<ResponseModel<AuthResponseDto>> LoginAsync(LoginDto dto);
    }
}
