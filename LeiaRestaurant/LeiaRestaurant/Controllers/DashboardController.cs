using Leia.Api.Extensions;
using Leia.Application.Dtos.DashboardDtos;
using Leia.Application.Helpers;
using Leia.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leia.Api.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController(IDashboardService dashboardService) : ControllerBase
    {
        [HttpGet("restaurant")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> GetRestaurant([FromQuery] DashboardQueryDto query)
        {
            var result = await dashboardService.GetRestaurantAsync(User.GetUserId(), query);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdmin([FromQuery] DashboardQueryDto query)
        {
            var result = await dashboardService.GetAdminAsync(query);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }
    }
}