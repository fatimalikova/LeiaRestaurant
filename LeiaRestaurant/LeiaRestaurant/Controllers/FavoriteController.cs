using Leia.Api.Extensions;
using Leia.Application.Dtos.Common;
using Leia.Application.Helpers;
using Leia.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leia.Api.Controllers
{
    [ApiController]
    [Route("api/favorites")]
    public class FavoriteController(IFavoriteService favoriteService) : ControllerBase
    {
        [HttpGet("my")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMy([FromQuery] PaginationDto dto)
        {
            var result = await favoriteService.GetMyAsync(User.GetUserId(), dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpGet("my/ids")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyIds()
        {
            var result = await favoriteService.GetMyIdsAsync(User.GetUserId());
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPost("{restaurantId:guid}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Add(Guid restaurantId)
        {
            await favoriteService.AddAsync(User.GetUserId(), restaurantId);
            return Ok(ResponseModelHelper.SuccessResult("Added to favorites."));
        }

        [HttpDelete("{restaurantId:guid}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Remove(Guid restaurantId)
        {
            await favoriteService.RemoveAsync(User.GetUserId(), restaurantId);
            return Ok(ResponseModelHelper.SuccessResult("Removed from favorites."));
        }
    }
}