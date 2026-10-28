using AutoMapper;
using Leia.Application.Dtos.UserDtos;
using Leia.Application.Exceptions;
using Leia.Application.Interfaces;
using Leia.Application.Settings;
using Leia.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Leia.Infrastructure.Services
{
    public class AccountService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IJwtService jwtService,
        IEmailService emailService,
        IOptions<JwtSettings> jwtOptions,
        IConfiguration config,
        IMapper mapper) : IAccountService
    {
        private const string TwoFactorPurpose = "TwoFactor";
        private static readonly string[] RegisterRoles = { "Customer", "RestaurantOwner" };

        public async Task RegisterAsync(RegisterDto dto, string role)
        {
            if (!RegisterRoles.Contains(role))
                throw new BadRequestException("Invalid role.");

            if (await userManager.FindByEmailAsync(dto.Email) != null)
                throw new ConflictException("This email is already registered.");

            var user = mapper.Map<ApplicationUser>(dto);

            var result = await userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new BadRequestException(string.Join(" ", result.Errors.Select(e => e.Description)));

            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));

            var roleResult = await userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                await userManager.DeleteAsync(user);
                throw new BadRequestException(string.Join(" ", roleResult.Errors.Select(e => e.Description)));
            }

            await SendConfirmationEmailAsync(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto, string requiredRole)
        {
            var user = await userManager.FindByEmailAsync(dto.Login) ?? await userManager.FindByNameAsync(dto.Login);

            if (user == null || !await userManager.CheckPasswordAsync(user, dto.Password))
                throw new UnauthorizedException("Invalid login or password.");

            var requireConfirmation = bool.TryParse(config["Auth:RequireEmailConfirmation"], out var confirm) && confirm;
            if (requireConfirmation && !user.EmailConfirmed)
                throw new BadRequestException("Email is not confirmed.");

            if (!user.IsActive)
                throw new ForbiddenException("Your account is deactivated.");

            var roles = await userManager.GetRolesAsync(user);
            if (!roles.Contains(requiredRole))
                throw new BadRequestException($"This account cannot log in from the {requiredRole} portal.");

            var requireTwoFactor = bool.TryParse(config["Auth:RequireTwoFactor"], out var flag) && flag;
            if (requireTwoFactor)
            {
                var code = await userManager.GenerateUserTokenAsync(user, TokenOptions.DefaultEmailProvider, TwoFactorPurpose);
                await emailService.SendEmailAsync(user.Email!, "2FA Code",
                    $"<h3>Login verification code</h3><p>Your code: <b>{code}</b></p><p>This code is valid for a few minutes.</p>");

                return new AuthResponseDto { RequiresTwoFactor = true, Email = user.Email! };
            }

            return await BuildAuthResponseAsync(user);
        }

        public async Task<AuthResponseDto> Verify2FAAsync(Verify2FADto dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email)
                ?? throw new UnauthorizedException("Code is invalid or expired.");

            var valid = await userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultEmailProvider, TwoFactorPurpose, dto.Code);
            if (!valid)
                throw new UnauthorizedException("Code is invalid or expired.");

            if (!user.IsActive)
                throw new ForbiddenException("Your account is deactivated.");

            return await BuildAuthResponseAsync(user);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
        {
            var principal = jwtService.GetPrincipalFromExpiredToken(dto.Token)
                ?? throw new BadRequestException("Invalid access token.");

            var idValue = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(idValue, out var userId))
                throw new BadRequestException("Invalid access token.");

            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null ||
                user.RefreshToken != dto.RefreshToken ||
                user.RefreshTokenExpiry is null ||
                user.RefreshTokenExpiry < DateTime.UtcNow)
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
                throw new BadRequestException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email)
                ?? throw new NotFoundException("User not found.");

            var token = await userManager.GeneratePasswordResetTokenAsync(user);

            await emailService.SendEmailAsync(dto.Email, "Reset Password",
                $"<h3>Password reset</h3><p>Email: <b>{dto.Email}</b></p>" +
                $"<p>Token:</p><p style='word-break:break-all'><b>{token}</b></p>" +
                $"<p>Paste this token into the reset-password endpoint.</p>");
        }

        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email)
                ?? throw new NotFoundException("User not found.");

            var result = await userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
            if (!result.Succeeded)
                throw new BadRequestException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        public async Task LogoutAsync(Guid userId)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null) return;

            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await userManager.UpdateAsync(user);
        }

        private async Task SendConfirmationEmailAsync(ApplicationUser user)
        {
            try
            {
                var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                var baseUrl = config["BaseUrl"];
                var link = $"{baseUrl}/api/account/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

                await emailService.SendEmailAsync(user.Email!, "Email Confirmation",
                    $"<h3>Confirm your email</h3><p>Click the link below:</p><a href='{link}'>Confirm</a>");
            }
            catch
            {
            }
        }

        private async Task<AuthResponseDto> BuildAuthResponseAsync(ApplicationUser user)
        {
            var roles = await userManager.GetRolesAsync(user);
            var token = jwtService.GenerateToken(user, roles);
            var refreshToken = jwtService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await userManager.UpdateAsync(user);

            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpiryMinutes),
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Roles = roles
            };
        }
    }
}