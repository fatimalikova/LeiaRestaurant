using Leia.Api.Extensions;
using Leia.Application.Dtos.MenuItemDtos;
using Leia.Application.Helpers;
using Leia.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leia.Api.Controllers
{
    [ApiController]
    [Route("api/menu-items")]
    public class MenuItemController(IMenuItemService menuItemService) : ControllerBase
    {
        [HttpGet("restaurant/{restaurantId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByRestaurant(Guid restaurantId)
        {
            var result = await menuItemService.GetByRestaurantAsync(restaurantId);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await menuItemService.GetByIdAsync(id);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpGet("my")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> GetMy()
        {
            var result = await menuItemService.GetMyAsync(User.GetUserId());
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPost]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> Create(CreateMenuItemDto dto)
        {
            var result = await menuItemService.CreateAsync(User.GetUserId(), dto);
            return StatusCode(201, ResponseModelHelper.CreatedResult(result));
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> Update(Guid id, UpdateMenuItemDto dto)
        {
            var result = await menuItemService.UpdateAsync(User.GetUserId(), id, dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPut("{id:guid}/availability")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> SetAvailability(Guid id, [FromQuery] bool isAvailable)
        {
            await menuItemService.SetAvailabilityAsync(User.GetUserId(), id, isAvailable);
            return Ok(ResponseModelHelper.SuccessResult("Availability updated."));
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await menuItemService.DeleteAsync(User.GetUserId(), id);
            return Ok(ResponseModelHelper.SuccessResult("Deleted."));
        }

        [HttpPut("{id:guid}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetDeactivatedByAdmin(Guid id, [FromQuery] bool deactivated)
        {
            await menuItemService.SetDeactivatedByAdminAsync(id, deactivated);
            return Ok(ResponseModelHelper.SuccessResult("Updated."));
        }
    }
}