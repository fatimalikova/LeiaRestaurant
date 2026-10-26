using Leia.Api.Extensions;
using Leia.Application.Dtos.UnavailableDayDtos;
using Leia.Application.Helpers;
using Leia.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leia.Api.Controllers
{
    [ApiController]
    [Route("api/unavailable-days")]
    public class UnavailableDayController(
        IUnavailableDayService unavailableDayService,
        IRestaurantService restaurantService) : ControllerBase
    {
        [HttpGet("restaurant/{restaurantId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByRestaurant(Guid restaurantId)
        {
            var result = await unavailableDayService.GetByRestaurantAsync(restaurantId);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpGet("my")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> GetMy()
        {
            var restaurant = await restaurantService.GetMyAsync(User.GetUserId());
            var result = await unavailableDayService.GetByRestaurantAsync(restaurant.Id);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPost]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> Create(CreateUnavailableDayDto dto)
        {
            var result = await unavailableDayService.CreateAsync(User.GetUserId(), dto);
            return StatusCode(201, ResponseModelHelper.CreatedResult(result));
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await unavailableDayService.DeleteAsync(User.GetUserId(), id);
            return Ok(ResponseModelHelper.SuccessResult("Deleted."));
        }
    }
}