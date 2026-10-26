using Leia.Api.Extensions;
using Leia.Application.Dtos.WorkingHourDtos;
using Leia.Application.Helpers;
using Leia.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leia.Api.Controllers
{
    [ApiController]
    [Route("api/working-hours")]
    public class WorkingHourController(
        IWorkingHourService workingHourService,
        IRestaurantService restaurantService) : ControllerBase
    {
        [HttpGet("restaurant/{restaurantId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByRestaurant(Guid restaurantId)
        {
            var result = await workingHourService.GetByRestaurantAsync(restaurantId);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpGet("my")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> GetMy()
        {
            var restaurant = await restaurantService.GetMyAsync(User.GetUserId());
            var result = await workingHourService.GetByRestaurantAsync(restaurant.Id);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPost]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> Create(CreateWorkingHourDto dto)
        {
            var result = await workingHourService.CreateAsync(User.GetUserId(), dto);
            return StatusCode(201, ResponseModelHelper.CreatedResult(result));
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> Update(Guid id, UpdateWorkingHourDto dto)
        {
            var result = await workingHourService.UpdateAsync(User.GetUserId(), id, dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await workingHourService.DeleteAsync(User.GetUserId(), id);
            return Ok(ResponseModelHelper.SuccessResult("Deleted."));
        }
    }
}