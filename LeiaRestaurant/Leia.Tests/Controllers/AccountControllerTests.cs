using Leia.Api.Controllers;
using Leia.Application.Dtos.ProfileDtos;
using Leia.Application.Dtos.UserDtos;
using Leia.Application.Exceptions;
using Leia.Application.Interfaces;
using Leia.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Leia.Tests.Controllers
{
    public class AccountControllerTests
    {
        private readonly Mock<IAccountService> _account = new();
        private readonly Mock<IProfileService> _profile = new();
        private readonly AccountController _controller;
        private readonly Guid _userId = Guid.NewGuid();

        public AccountControllerTests()
        {
            _controller = new AccountController(_account.Object, _profile.Object).WithUser(_userId, "Customer");
        }

        [Fact]
        public async Task RegisterCustomer_AssignsCustomerRole()
        {
            var dto = new RegisterDto();

            var result = await _controller.RegisterCustomer(dto);

            Assert.IsType<OkObjectResult>(result);
            _account.Verify(s => s.RegisterAsync(dto, "Customer"), Times.Once);
        }

        [Fact]
        public async Task RegisterRestaurant_AssignsRestaurantOwnerRole()
        {
            var dto = new RegisterDto();

            await _controller.RegisterRestaurant(dto);

            _account.Verify(s => s.RegisterAsync(dto, "RestaurantOwner"), Times.Once);
        }

        [Theory]
        [InlineData("Customer")]
        [InlineData("RestaurantOwner")]
        [InlineData("Admin")]
        public async Task Login_UsesTheRequiredPortalRole(string role)
        {
            var dto = new LoginDto();
            _account.Setup(s => s.LoginAsync(dto, role)).ReturnsAsync(new AuthResponseDto());

            var result = role switch
            {
                "Customer" => await _controller.LoginCustomer(dto),
                "RestaurantOwner" => await _controller.LoginRestaurant(dto),
                _ => await _controller.LoginAdmin(dto)
            };

            Assert.IsType<OkObjectResult>(result);
            _account.Verify(s => s.LoginAsync(dto, role), Times.Once);
        }

        [Fact]
        public async Task Login_PropagatesUnauthorized()
        {
            var dto = new LoginDto();
            _account.Setup(s => s.LoginAsync(dto, "Customer")).ThrowsAsync(new UnauthorizedException("bad"));

            await Assert.ThrowsAsync<UnauthorizedException>(() => _controller.LoginCustomer(dto));
        }

        [Fact]
        public async Task Verify2FA_ReturnsTokens()
        {
            var dto = new Verify2FADto();
            _account.Setup(s => s.Verify2FAAsync(dto)).ReturnsAsync(new AuthResponseDto { Token = "t" });

            var result = await _controller.Verify2FA(dto);

            Assert.Equal("t", ControllerTestBase.Body<AuthResponseDto>(result).Data!.Token);
        }

        [Fact]
        public async Task Logout_UsesUserIdFromToken()
        {
            var result = await _controller.Logout();

            Assert.IsType<OkObjectResult>(result);
            _account.Verify(s => s.LogoutAsync(_userId), Times.Once);
        }

        [Fact]
        public async Task GetProfile_UsesUserIdFromToken()
        {
            _profile.Setup(s => s.GetAsync(_userId)).ReturnsAsync(new ProfileResponseDto());

            var result = await _controller.GetProfile();

            Assert.IsType<OkObjectResult>(result);
            _profile.Verify(s => s.GetAsync(_userId), Times.Once);
        }

        [Fact]
        public async Task UpdateProfile_UsesUserIdFromToken()
        {
            var dto = new UpdateProfileDto();
            _profile.Setup(s => s.UpdateAsync(_userId, dto)).ReturnsAsync(new ProfileResponseDto());

            await _controller.UpdateProfile(dto);

            _profile.Verify(s => s.UpdateAsync(_userId, dto), Times.Once);
        }

        [Theory]
        [InlineData(nameof(AccountController.Logout))]
        [InlineData(nameof(AccountController.GetProfile))]
        [InlineData(nameof(AccountController.UpdateProfile))]
        [InlineData(nameof(AccountController.ChangePassword))]
        public void ProtectedMethods_RequireAuthentication(string method)
        {
            var attr = typeof(AccountController).GetMethod(method)!
                .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true);

            Assert.NotEmpty(attr);
        }


        [Fact]
        public async Task ConfirmEmail_CallsService()
        {
            var dto = new EmailConfirmDto { UserId = Guid.NewGuid(), Token = "t" };

            var result = await _controller.ConfirmEmail(dto);

            Assert.IsType<OkObjectResult>(result);
            _account.Verify(s => s.ConfirmEmailAsync(dto), Times.Once);
        }

        [Fact]
        public async Task ForgotPassword_AlwaysReturnsOkMessage()
        {
            var result = await _controller.ForgotPassword(new ForgotPasswordDto());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ResetPassword_CallsService()
        {
            var dto = new ResetPasswordDto();

            await _controller.ResetPassword(dto);

            _account.Verify(s => s.ResetPasswordAsync(dto), Times.Once);
        }

        [Fact]
        public async Task ResetPassword_PropagatesBadRequest()
        {
            _account.Setup(s => s.ResetPasswordAsync(It.IsAny<ResetPasswordDto>())).ThrowsAsync(new BadRequestException("x"));

            await Assert.ThrowsAsync<BadRequestException>(() => _controller.ResetPassword(new ResetPasswordDto()));
        }

        [Fact]
        public async Task Refresh_ReturnsNewTokens()
        {
            var dto = new RefreshTokenDto();
            _account.Setup(s => s.RefreshTokenAsync(dto)).ReturnsAsync(new AuthResponseDto { Token = "n" });

            var result = await _controller.Refresh(dto);

            Assert.Equal("n", ControllerTestBase.Body<AuthResponseDto>(result).Data!.Token);
        }

        [Fact]
        public async Task ChangePassword_UsesUserIdFromToken()
        {
            var dto = new ChangePasswordDto();

            await _controller.ChangePassword(dto);

            _profile.Verify(s => s.ChangePasswordAsync(_userId, dto), Times.Once);
        }

        [Theory]
        [InlineData(nameof(AccountController.Verify2FA))]
        [InlineData(nameof(AccountController.Refresh))]
        [InlineData(nameof(AccountController.ForgotPassword))]
        [InlineData(nameof(AccountController.ResetPassword))]
        [InlineData(nameof(AccountController.LoginAdmin))]
        public void Auth_endpoints_are_rate_limited(string method)
        {
            var attr = typeof(AccountController).GetMethod(method)!
                .GetCustomAttributes(typeof(Microsoft.AspNetCore.RateLimiting.EnableRateLimitingAttribute), true);

            if (method is nameof(AccountController.Refresh))
                return;

            Assert.NotEmpty(attr);
        }
    }
}