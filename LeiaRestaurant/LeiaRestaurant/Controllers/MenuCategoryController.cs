using Leia.Api.Extensions;
using Leia.Application.Dtos.MenuCategoryDtos;
using Leia.Application.Helpers;
using Leia.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leia.Api.Controllers
{
    [ApiController]
    [Route("api/menu-categories")]
    public class MenuCategoryController(IMenuCategoryService menuCategoryService) : ControllerBase
    {
        [HttpGet("restaurant/{restaurantId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByRestaurant(Guid restaurantId)
        {
            var result = await menuCategoryService.GetByRestaurantAsync(restaurantId);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpGet("my")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> GetMy()
        {
            var result = await menuCategoryService.GetMyAsync(User.GetUserId());
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPost]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> Create(CreateMenuCategoryDto dto)
        {
            var result = await menuCategoryService.CreateAsync(User.GetUserId(), dto);
            return StatusCode(201, ResponseModelHelper.CreatedResult(result));
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> Update(Guid id, UpdateMenuCategoryDto dto)
        {
            var result = await menuCategoryService.UpdateAsync(User.GetUserId(), id, dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await menuCategoryService.DeleteAsync(User.GetUserId(), id);
            return Ok(ResponseModelHelper.SuccessResult("Deleted."));
        }
    }
}