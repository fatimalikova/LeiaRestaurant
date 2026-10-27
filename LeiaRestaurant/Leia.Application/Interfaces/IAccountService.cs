using Leia.Application.Dtos.AccountDtos;
using Leia.Application.Dtos.UserDtos;

namespace Leia.Application.Interfaces
{
    public interface IAccountService
    {
        Task RegisterAsync(RegisterDto dto, string role);
        Task<AuthResponseDto> LoginAsync(LoginDto dto, string requiredRole);
        Task<AuthResponseDto> Verify2FAAsync(Verify2FADto dto);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto);
        Task ConfirmEmailAsync(EmailConfirmDto dto);
        Task ForgotPasswordAsync(ForgotPasswordDto dto);
        Task ResetPasswordAsync(ResetPasswordDto dto);
        Task LogoutAsync(Guid userId);
    }
}