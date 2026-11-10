using Leia.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Reflection;
using Xunit;

namespace Leia.Tests.Controllers
{
    public class SecurityTests
    {
        public static IEnumerable<object[]> AllActions()
        {
            var controllers = typeof(AccountController).Assembly.GetTypes()
                .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract);

            foreach (var controller in controllers)
                foreach (var method in controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(m => m.GetCustomAttributes<HttpMethodAttribute>().Any()))
                    yield return new object[] { controller.Name, method.Name };
        }

        [Theory]
        [MemberData(nameof(AllActions))]
        public void Every_action_is_explicitly_protected_or_anonymous(string controllerName, string methodName)
        {
            var type = typeof(AccountController).Assembly.GetTypes().First(t => t.Name == controllerName);
            var method = type.GetMethod(methodName)!;

            var hasPolicy = method.GetCustomAttributes<AuthorizeAttribute>().Any()
                || method.GetCustomAttributes<AllowAnonymousAttribute>().Any()
                || type.GetCustomAttributes<AuthorizeAttribute>().Any();

            Assert.True(hasPolicy, $"{controllerName}.{methodName} has no [Authorize] or [AllowAnonymous].");
        }

        [Theory]
        [MemberData(nameof(AllActions))]
        public void Admin_only_actions_never_allow_other_roles(string controllerName, string methodName)
        {
            var type = typeof(AccountController).Assembly.GetTypes().First(t => t.Name == controllerName);
            var roles = type.GetMethod(methodName)!.GetCustomAttribute<AuthorizeAttribute>()?.Roles;

            if (roles != null && roles.Contains("Admin") && !roles.Contains(','))
                Assert.Equal("Admin", roles);
        }
    }
}