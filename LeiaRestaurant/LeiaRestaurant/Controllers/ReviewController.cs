using Leia.Api.Extensions;
using Leia.Application.Dtos.Common;
using Leia.Application.Dtos.ReviewDtos;
using Leia.Application.Helpers;
using Leia.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leia.Api.Controllers
{
    [ApiController]
    [Route("api/reviews")]
    public class ReviewController(IReviewService reviewService) : ControllerBase
    {
        [HttpGet("restaurant/{restaurantId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByRestaurant(Guid restaurantId, [FromQuery] PaginationDto dto)
        {
            var result = await reviewService.GetByRestaurantAsync(restaurantId, dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Create(CreateReviewDto dto)
        {
            var result = await reviewService.CreateAsync(User.GetUserId(), dto);
            return StatusCode(201, ResponseModelHelper.CreatedResult(result));
        }

        [HttpPost("{id:guid}/reply")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> Reply(Guid id, CreateReviewReplyDto dto)
        {
            var result = await reviewService.ReplyAsync(User.GetUserId(), id, dto);
            return StatusCode(201, ResponseModelHelper.CreatedResult(result));
        }

        [HttpDelete("{id:guid}/reply")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> DeleteReply(Guid id)
        {
            await reviewService.DeleteReplyAsync(User.GetUserId(), id);
            return Ok(ResponseModelHelper.SuccessResult("Deleted."));
        }
    }
}