using Leia.Api.Extensions;
using Leia.Application.Dtos.CouponDtos;
using Leia.Application.Helpers;
using Leia.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leia.Api.Controllers
{
    [ApiController]
    [Route("api/coupons")]
    public class CouponController(ICouponService couponService) : ControllerBase
    {
        [HttpPost("validate")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Validate(ValidateCouponDto dto)
        {
            var result = await couponService.EvaluateAsync(User.GetUserId(), dto.RestaurantId, dto.Code, dto.Subtotal);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpGet("my")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> GetMy()
        {
            var result = await couponService.GetMyAsync(User.GetUserId());
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllForAdmin()
        {
            var result = await couponService.GetAllForAdminAsync();
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPost]
        [Authorize(Roles = "RestaurantOwner,Admin")]
        public async Task<IActionResult> Create(CreateCouponDto dto)
        {
            var result = await couponService.CreateAsync(User.GetUserId(), User.IsInRole("Admin"), dto);
            return StatusCode(201, ResponseModelHelper.CreatedResult(result));
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "RestaurantOwner,Admin")]
        public async Task<IActionResult> Update(Guid id, UpdateCouponDto dto)
        {
            var result = await couponService.UpdateAsync(User.GetUserId(), User.IsInRole("Admin"), id, dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "RestaurantOwner,Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await couponService.DeleteAsync(User.GetUserId(), User.IsInRole("Admin"), id);
            return Ok(ResponseModelHelper.SuccessResult("Deleted."));
        }
    }
}