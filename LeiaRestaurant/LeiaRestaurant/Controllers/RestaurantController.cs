using Leia.Api.Extensions;
using Leia.Application.Dtos.Common;
using Leia.Application.Dtos.CuisineDtos;
using Leia.Application.Dtos.RestaurantDtos;
using Leia.Application.Dtos.RestaurantImageDtos;
using Leia.Application.Helpers;
using Leia.Application.Interfaces;
using Leia.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leia.Api.Controllers
{
    [ApiController]
    [Route("api/restaurants")]
    public class RestaurantController(IRestaurantService restaurantService) : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] RestaurantFilterDto filter)
        {
            var result = await restaurantService.GetAllAsync(filter);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await restaurantService.GetByIdAsync(id);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpGet("my")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> GetMy()
        {
            var result = await restaurantService.GetMyAsync(User.GetUserId());
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPost("my")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> Create(CreateRestaurantDto dto)
        {
            var result = await restaurantService.CreateAsync(User.GetUserId(), dto);
            return StatusCode(201, ResponseModelHelper.CreatedResult(result));
        }

        [HttpPut("my")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> Update(UpdateRestaurantDto dto)
        {
            var result = await restaurantService.UpdateAsync(User.GetUserId(), dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPut("my/cuisines")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> SetCuisines(SetRestaurantCuisinesDto dto)
        {
            var result = await restaurantService.SetCuisinesAsync(User.GetUserId(), dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPost("my/images")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> AddImage(AddRestaurantImageDto dto)
        {
            var result = await restaurantService.AddImageAsync(User.GetUserId(), dto);
            return StatusCode(201, ResponseModelHelper.CreatedResult(result));
        }

        [HttpDelete("my/images/{imageId:guid}")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> DeleteImage(Guid imageId)
        {
            var result = await restaurantService.DeleteImageAsync(User.GetUserId(), imageId);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllForAdmin([FromQuery] PaginationDto dto, [FromQuery] RestaurantStatus? status)
        {
            var result = await restaurantService.GetAllForAdminAsync(dto, status);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPut("{id:guid}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(Guid id, UpdateRestaurantStatusDto dto)
        {
            var result = await restaurantService.UpdateStatusAsync(id, dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }
    }
}