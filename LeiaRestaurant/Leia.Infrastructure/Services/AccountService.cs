using Leia.Application.Dtos.AccountDtos;
using Leia.Application.Dtos.UserDtos;
using Leia.Application.Exceptions;
using Leia.Application.Interfaces;
using Leia.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace Leia.Infrastructure.Services
{
    public class AccountService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IJwtService jwtService,
        IEmailService emailService,
        IConfiguration config) : IAccountService
    {
        private const string TwoFactorPurpose = "TwoFactor";

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            if (await userManager.FindByEmailAsync(dto.Email) != null)
                throw new ConflictException("This email is already registered.");

            var user = new ApplicationUser
            {
                FullName = dto.FullName,
                Email = dto.Email,
                UserName = dto.Email,
                PhoneNumber = dto.PhoneNumber
            };

            var result = await userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new BadRequestException(string.Join(" ", result.Errors.Select(e => e.Description)));

            await EnsureRoleAsync(dto.Role);
            await userManager.AddToRoleAsync(user, dto.Role);

            await SendConfirmationEmailAsync(user);

            return await BuildAuthResponseAsync(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await userManager.CheckPasswordAsync(user, dto.Password))
                throw new UnauthorizedException("Email or password is incorrect.");

            if (!user.IsActive)
                throw new ForbiddenException("Your account is deactivated.");

            var requireTwoFactor = bool.TryParse(config["Auth:RequireTwoFactor"], out var flag) && flag;
            if (requireTwoFactor)
            {
                var code = await userManager.GenerateUserTokenAsync(user, TokenOptions.DefaultEmailProvider, TwoFactorPurpose);
                await emailService.SendEmailAsync(user.Email!, "Your Leia verification code",
                    $"<h3>Verification code</h3><p>Your code is <b>{code}</b>. It expires in a few minutes.</p>");

                return new AuthResponseDto { RequiresTwoFactor = true, Email = user.Email! };
            }

            return await BuildAuthResponseAsync(user);
        }

        public async Task<AuthResponseDto> Verify2FAAsync(Verify2FADto dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email)
                ?? throw new UnauthorizedException("Invalid or expired code.");

            var valid = await userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultEmailProvider, TwoFactorPurpose, dto.Code);
            if (!valid)
                throw new UnauthorizedException("Invalid or expired code.");

            if (!user.IsActive)
                throw new ForbiddenException("Your account is deactivated.");

            return await BuildAuthResponseAsync(user);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
        {
            var principal = jwtService.GetPrincipalFromExpiredToken(dto.Token, config)
                ?? throw new UnauthorizedException("Invalid token.");

            var idValue = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(idValue, out var userId))
                throw new UnauthorizedException("Invalid token.");

            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null || user.RefreshToken != dto.RefreshToken || user.RefreshTokenExpiry <= DateTime.UtcNow)
                throw new UnauthorizedException("Refresh token is invalid or expired.");

            if (!user.IsActive)
                throw new ForbiddenException("Your account is deactivated.");

            return await BuildAuthResponseAsync(user);
        }

        public async Task ConfirmEmailAsync(EmailConfirmDto dto)
        {
            var user = await userManager.FindByIdAsync(dto.UserId.ToString())
                ?? throw new NotFoundException("User not found.");

            var result = await userManager.ConfirmEmailAsync(user, dto.Token);
            if (!result.Succeeded)
                throw new BadRequestException("Confirmation link is invalid or expired.");
        }

        public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user == null) return;

            try
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var baseUrl = config["FrontendUrl"] ?? config["BaseUrl"];
                var link = $"{baseUrl}/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";

                await emailService.SendEmailAsync(user.Email!, "Reset your Leia password",
                    $"<h3>Password reset</h3><p><a href='{link}'>Click here to reset your password</a></p>");
            }
            catch
            {
                // Email failure must not reveal anything to the caller.
            }
        }

        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email)
                ?? throw new BadRequestException("Invalid reset request.");

            var result = await userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
            if (!result.Succeeded)
                throw new BadRequestException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }


        private async Task EnsureRoleAsync(string role)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }

        private async Task SendConfirmationEmailAsync(ApplicationUser user)
        {
            try
            {
                var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                var baseUrl = config["FrontendUrl"] ?? config["BaseUrl"];
                var link = $"{baseUrl}/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

                await emailService.SendEmailAsync(user.Email!, "Confirm your Leia account",
                    $"<h3>Welcome to Leia, {user.FullName}!</h3><p><a href='{link}'>Confirm your email</a></p>");
            }
            catch
            {
            }
        }

        private async Task<AuthResponseDto> BuildAuthResponseAsync(ApplicationUser user)
        {
            var roles = await userManager.GetRolesAsync(user);
            var token = jwtService.GenerateToken(user, roles, config);
            var refreshToken = jwtService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await userManager.UpdateAsync(user);

            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(JwtService.AccessTokenMinutes),
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Roles = roles
            };
        }
    }
}