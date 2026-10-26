using Leia.Api.Extensions;
using Leia.Application.Dtos.OrderDtos;
using Leia.Application.Helpers;
using Leia.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leia.Api.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrderController(IOrderService orderService) : ControllerBase
    {
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Create(CreateOrderDto dto)
        {
            var result = await orderService.CreateAsync(User.GetUserId(), dto);
            return StatusCode(201, ResponseModelHelper.CreatedResult(result));
        }

        [HttpGet("my")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMy([FromQuery] OrderFilterDto filter)
        {
            var result = await orderService.GetMyOrdersAsync(User.GetUserId(), filter);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPut("{id:guid}/cancel")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Cancel(Guid id, CancelOrderDto dto)
        {
            var result = await orderService.CancelAsync(User.GetUserId(), id, dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpGet("restaurant")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> GetRestaurantOrders([FromQuery] OrderFilterDto filter)
        {
            var result = await orderService.GetRestaurantOrdersAsync(User.GetUserId(), filter);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPut("{id:guid}/status")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> UpdateStatus(Guid id, UpdateOrderStatusDto dto)
        {
            var result = await orderService.UpdateStatusAsync(User.GetUserId(), id, dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllForAdmin([FromQuery] OrderFilterDto filter)
        {
            var result = await orderService.GetAllForAdminAsync(filter);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await orderService.GetByIdAsync(User.GetUserId(), id);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }
    }
}