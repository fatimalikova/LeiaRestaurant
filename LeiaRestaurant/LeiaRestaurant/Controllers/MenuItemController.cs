using Leia.Api.Extensions;
using Leia.Application.Dtos.AllergenDtos;
using Leia.Application.Dtos.MenuItemDtos;
using Leia.Application.Dtos.ModifierDtos;
using Leia.Application.Dtos.VariantDtos;
using Leia.Application.Helpers;
using Leia.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leia.Api.Controllers
{
    [ApiController]
    [Route("api/menu-items")]
    public class MenuItemController(IMenuItemService menuItemService, IMenuOptionService menuOptionService) : ControllerBase
    {
        [HttpGet("restaurant/{restaurantId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByRestaurant(Guid restaurantId)
        {
            var result = await menuItemService.GetByRestaurantAsync(restaurantId);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] MenuSearchDto dto)
        {
            var result = await menuItemService.SearchAsync(dto);
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

        [HttpPut("{id:guid}/restore")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Restore(Guid id)
        {
            var result = await menuItemService.RestoreAsync(id);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPost("{id:guid}/variants")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> AddVariant(Guid id, CreateVariantDto dto)
        {
            var result = await menuOptionService.AddVariantAsync(User.GetUserId(), id, dto);
            return StatusCode(201, ResponseModelHelper.CreatedResult(result));
        }

        [HttpPut("variants/{variantId:guid}")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> UpdateVariant(Guid variantId, UpdateVariantDto dto)
        {
            var result = await menuOptionService.UpdateVariantAsync(User.GetUserId(), variantId, dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpDelete("variants/{variantId:guid}")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> DeleteVariant(Guid variantId)
        {
            var result = await menuOptionService.DeleteVariantAsync(User.GetUserId(), variantId);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPost("{id:guid}/modifier-groups")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> AddGroup(Guid id, CreateModifierGroupDto dto)
        {
            var result = await menuOptionService.AddGroupAsync(User.GetUserId(), id, dto);
            return StatusCode(201, ResponseModelHelper.CreatedResult(result));
        }

        [HttpPut("modifier-groups/{groupId:guid}")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> UpdateGroup(Guid groupId, UpdateModifierGroupDto dto)
        {
            var result = await menuOptionService.UpdateGroupAsync(User.GetUserId(), groupId, dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpDelete("modifier-groups/{groupId:guid}")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> DeleteGroup(Guid groupId)
        {
            var result = await menuOptionService.DeleteGroupAsync(User.GetUserId(), groupId);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPost("modifier-groups/{groupId:guid}/options")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> AddOption(Guid groupId, CreateModifierOptionDto dto)
        {
            var result = await menuOptionService.AddOptionAsync(User.GetUserId(), groupId, dto);
            return StatusCode(201, ResponseModelHelper.CreatedResult(result));
        }

        [HttpPut("modifier-options/{optionId:guid}")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> UpdateOption(Guid optionId, UpdateModifierOptionDto dto)
        {
            var result = await menuOptionService.UpdateOptionAsync(User.GetUserId(), optionId, dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpDelete("modifier-options/{optionId:guid}")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> DeleteOption(Guid optionId)
        {
            var result = await menuOptionService.DeleteOptionAsync(User.GetUserId(), optionId);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPut("{id:guid}/allergens")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> SetAllergens(Guid id, SetMenuItemAllergensDto dto)
        {
            var result = await menuOptionService.SetAllergensAsync(User.GetUserId(), id, dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }
    }
}