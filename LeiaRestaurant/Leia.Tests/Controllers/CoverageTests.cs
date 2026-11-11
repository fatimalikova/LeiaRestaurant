using Leia.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Leia.Tests.Controllers
{
    public class CoverageTests
    {
        public static IEnumerable<object[]> ControllerNames() =>
            typeof(Leia.Api.Controllers.AccountController).Assembly.GetTypes()
                .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract)
                .Select(t => new object[] { t.Name });

        [Theory]
        [MemberData(nameof(ControllerNames))]
        public void Every_controller_has_a_test_class(string controllerName)
        {
            var expected = controllerName + "Tests";
            var exists = typeof(CoverageTests).Assembly.GetTypes().Any(t => t.Name == expected);

            Assert.True(exists, $"{controllerName} has no test class named {expected}.");
        }
    }
}