using Leia.Api.Extensions;
using Leia.Application.Dtos.AccountDtos;
using Leia.Application.Dtos.ProfileDtos;
using Leia.Application.Dtos.UserDtos;
using Leia.Application.Helpers;
using Leia.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leia.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController(
        IAccountService accountService,
        IProfileService profileService) : ControllerBase
    {
        [HttpPost("register/customer")]
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterDto dto)
        {
            await accountService.RegisterAsync(dto, "Customer");
            return Ok(ResponseModelHelper.SuccessResult("Registration successful. Check your email."));
        }

        [HttpPost("register/restaurant")]
        public async Task<IActionResult> RegisterRestaurant([FromBody] RegisterDto dto)
        {
            await accountService.RegisterAsync(dto, "RestaurantOwner");
            return Ok(ResponseModelHelper.SuccessResult("Registration successful. Check your email."));
        }

        [HttpPost("login/customer")]
        public async Task<IActionResult> LoginCustomer([FromBody] LoginDto dto)
        {
            var result = await accountService.LoginAsync(dto, "Customer");
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPost("login/restaurant")]
        public async Task<IActionResult> LoginRestaurant([FromBody] LoginDto dto)
        {
            var result = await accountService.LoginAsync(dto, "RestaurantOwner");
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPost("login/admin")]
        public async Task<IActionResult> LoginAdmin([FromBody] LoginDto dto)
        {
            var result = await accountService.LoginAsync(dto, "Admin");
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPost("verify-2fa")]
        public async Task<IActionResult> Verify2FA([FromBody] Verify2FADto dto)
        {
            var result = await accountService.Verify2FAAsync(dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] EmailConfirmDto dto)
        {
            await accountService.ConfirmEmailAsync(dto);
            return Ok(ResponseModelHelper.SuccessResult("Email confirmed successfully."));
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            await accountService.ForgotPasswordAsync(dto);
            return Ok(ResponseModelHelper.SuccessResult("Password reset token has been sent to your email."));
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            await accountService.ResetPasswordAsync(dto);
            return Ok(ResponseModelHelper.SuccessResult("Password changed successfully."));
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
        {
            var result = await accountService.RefreshTokenAsync(dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await accountService.LogoutAsync(User.GetUserId());
            return Ok(ResponseModelHelper.SuccessResult("Logged out."));
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var result = await profileService.GetAsync(User.GetUserId());
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var result = await profileService.UpdateAsync(User.GetUserId(), dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPut("profile/password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            await profileService.ChangePasswordAsync(User.GetUserId(), dto);
            return Ok(ResponseModelHelper.SuccessResult("Password changed."));
        }
    }
}