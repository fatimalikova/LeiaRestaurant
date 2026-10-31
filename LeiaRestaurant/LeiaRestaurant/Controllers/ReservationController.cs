using Leia.Api.Extensions;
using Leia.Application.Dtos.ReservationDtos;
using Leia.Application.Helpers;
using Leia.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leia.Api.Controllers
{
    [ApiController]
    [Route("api/reservations")]
    public class ReservationController(IReservationService reservationService) : ControllerBase
    {
        [HttpGet("availability")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailability([FromQuery] AvailabilityQueryDto query)
        {
            var result = await reservationService.GetAvailabilityAsync(query);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Create(CreateReservationDto dto)
        {
            var result = await reservationService.CreateAsync(User.GetUserId(), dto);
            return StatusCode(201, ResponseModelHelper.CreatedResult(result));
        }

        [HttpGet("my")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMy([FromQuery] ReservationFilterDto filter)
        {
            var result = await reservationService.GetMyAsync(User.GetUserId(), filter);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPut("{id:guid}/cancel")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Cancel(Guid id, CancelReservationDto dto)
        {
            var result = await reservationService.CancelAsync(User.GetUserId(), id, dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPut("{id:guid}/reschedule")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Reschedule(Guid id, RescheduleReservationDto dto)
        {
            var result = await reservationService.RescheduleAsync(User.GetUserId(), id, dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpGet("restaurant")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> GetRestaurantReservations([FromQuery] ReservationFilterDto filter)
        {
            var result = await reservationService.GetRestaurantReservationsAsync(User.GetUserId(), filter);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPut("{id:guid}/status")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> UpdateStatus(Guid id, UpdateReservationStatusDto dto)
        {
            var result = await reservationService.UpdateStatusAsync(User.GetUserId(), id, dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpGet("settings")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> GetSettings()
        {
            var result = await reservationService.GetSettingsAsync(User.GetUserId());
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPut("settings")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> UpdateSettings(ReservationSettingsDto dto)
        {
            var result = await reservationService.UpdateSettingsAsync(User.GetUserId(), dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllForAdmin([FromQuery] ReservationFilterDto filter)
        {
            var result = await reservationService.GetAllForAdminAsync(filter);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await reservationService.GetByIdAsync(User.GetUserId(), id);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }
    }
}