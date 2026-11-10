using Leia.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Security.Claims;

namespace Leia.Tests.Helpers
{
    public static class ControllerTestBase
    {
        public static T WithUser<T>(this T controller, Guid userId, params string[] roles) where T : ControllerBase
        {
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId.ToString()) };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
                }
            };

            return controller;
        }

        public static ResponseModel<T> Body<T>(IActionResult result)
        {
            var obj = Assert.IsAssignableFrom<ObjectResult>(result);
            return Assert.IsType<ResponseModel<T>>(obj.Value);
        }

        public static string? RolesOf<TController>(string method)
        {
            var info = typeof(TController).GetMethod(method)
                ?? throw new InvalidOperationException($"Method {method} not found.");

            return info.GetCustomAttribute<AuthorizeAttribute>()?.Roles;
        }

        public static bool IsAnonymous<TController>(string method)
        {
            var info = typeof(TController).GetMethod(method)
                ?? throw new InvalidOperationException($"Method {method} not found.");

            return info.GetCustomAttribute<AllowAnonymousAttribute>() != null;
        }
    }
}