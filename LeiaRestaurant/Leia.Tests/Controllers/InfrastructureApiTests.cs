using Leia.Api.Extensions;
using Leia.Api.Hubs;
using Leia.Api.Middlewares;
using Leia.Application.Dtos.NotificationDtos;
using Leia.Application.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Xunit;

namespace Leia.Tests.Api
{
    public class ClaimsPrincipalExtensionsTests
    {
        [Fact]
        public void GetUserId_returns_the_id_from_the_token()
        {
            var id = Guid.NewGuid();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, id.ToString()) }, "t"));

            Assert.Equal(id, user.GetUserId());
        }

        [Fact]
        public void GetUserId_throws_when_claim_is_missing()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity("t"));

            Assert.Throws<UnauthorizedAccessException>(() => user.GetUserId());
        }

        [Fact]
        public void GetUserId_throws_when_claim_is_not_a_guid()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "abc") }, "t"));

            Assert.Throws<UnauthorizedAccessException>(() => user.GetUserId());
        }
    }

    public class GlobalExceptionMiddlewareTests
    {
        private static async Task<(int Status, JsonElement Body)> Run(Exception ex, string environment = "Production")
        {
            var env = new Mock<IHostEnvironment>();
            env.Setup(e => e.EnvironmentName).Returns(environment);

            var middleware = new GlobalExceptionMiddleware(
                _ => throw ex,
                new Mock<ILogger<GlobalExceptionMiddleware>>().Object,
                env.Object);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(context);

            context.Response.Body.Position = 0;
            var body = JsonDocument.Parse(await new StreamReader(context.Response.Body).ReadToEndAsync()).RootElement;
            return (context.Response.StatusCode, body);
        }

        [Theory]
        [InlineData("badrequest", 400)]
        [InlineData("unauthorized", 401)]
        [InlineData("forbidden", 403)]
        [InlineData("notfound", 404)]
        [InlineData("conflict", 409)]
        public async Task Api_exceptions_map_to_their_status_code(string kind, int expected)
        {
            Exception ex = kind switch
            {
                "badrequest" => new BadRequestException("msg"),
                "unauthorized" => new UnauthorizedException("msg"),
                "forbidden" => new ForbiddenException("msg"),
                "notfound" => new NotFoundException("msg"),
                _ => new ConflictException("msg")
            };

            var (status, body) = await Run(ex);

            Assert.Equal(expected, status);
            Assert.Equal(expected, body.GetProperty("statusCode").GetInt32());
            Assert.False(body.GetProperty("success").GetBoolean());
            Assert.Equal("msg", body.GetProperty("errors")[0].GetString());
        }

        [Fact]
        public async Task Unexpected_error_hides_details_in_production()
        {
            var (status, body) = await Run(new InvalidOperationException("secret sql details"), "Production");

            Assert.Equal(500, status);
            Assert.Equal("An unexpected error occurred.", body.GetProperty("errors")[0].GetString());
        }

        [Fact]
        public async Task Unexpected_error_shows_details_in_development()
        {
            var (status, body) = await Run(new InvalidOperationException("details"), "Development");

            Assert.Equal(500, status);
            Assert.Contains("details", body.GetProperty("errors")[0].GetString());
        }

        [Fact]
        public async Task Successful_request_is_untouched()
        {
            var env = new Mock<IHostEnvironment>();
            var middleware = new GlobalExceptionMiddleware(
                ctx => { ctx.Response.StatusCode = 204; return Task.CompletedTask; },
                new Mock<ILogger<GlobalExceptionMiddleware>>().Object,
                env.Object);

            var context = new DefaultHttpContext();
            await middleware.InvokeAsync(context);

            Assert.Equal(204, context.Response.StatusCode);
        }
    }

    public class SignalRTests
    {
        [Fact]
        public async Task Notifier_sends_to_the_right_user_with_the_right_event()
        {
            var userId = Guid.NewGuid();
            var dto = new ResponseNotificationDto { Title = "hi" };

            var proxy = new Mock<IClientProxy>();
            var clients = new Mock<IHubClients>();
            clients.Setup(c => c.User(userId.ToString())).Returns(proxy.Object);
            var hub = new Mock<IHubContext<NotificationHub>>();
            hub.Setup(h => h.Clients).Returns(clients.Object);

            await new SignalRNotifier(hub.Object).SendNotificationAsync(userId, dto);

            clients.Verify(c => c.User(userId.ToString()), Times.Once);
            proxy.Verify(p => p.SendCoreAsync(
                "ReceiveNotification",
                It.Is<object?[]>(a => a.Length == 1 && ReferenceEquals(a[0], dto)),
                default), Times.Once);
        }

        [Fact]
        public void Hub_requires_authentication()
        {
            Assert.NotEmpty(typeof(NotificationHub).GetCustomAttributes<AuthorizeAttribute>());
        }
    }
}