using Leia.Application.Dtos.AccountDtos;
using Leia.Application.Dtos.UserDtos;
using Leia.Application.Helpers;
using Leia.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leia.Api.Controllers
{
    [ApiController]
    [Route("api/account")]
    public class AccountController(IAccountService accountService) : ControllerBase
    {
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var result = await accountService.RegisterAsync(dto);
            return StatusCode(201, ResponseModelHelper.CreatedResult(result));
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await accountService.LoginAsync(dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPost("verify-2fa")]
        [AllowAnonymous]
        public async Task<IActionResult> Verify2FA(Verify2FADto dto)
        {
            var result = await accountService.Verify2FAAsync(dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh(RefreshTokenDto dto)
        {
            var result = await accountService.RefreshTokenAsync(dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPost("confirm-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(EmailConfirmDto dto)
        {
            await accountService.ConfirmEmailAsync(dto);
            return Ok(ResponseModelHelper.SuccessResult("Email confirmed."));
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            await accountService.ForgotPasswordAsync(dto);
            return Ok(ResponseModelHelper.SuccessResult("If the email exists, a reset link has been sent."));
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            await accountService.ResetPasswordAsync(dto);
            return Ok(ResponseModelHelper.SuccessResult("Password has been reset."));
        }
    }
}