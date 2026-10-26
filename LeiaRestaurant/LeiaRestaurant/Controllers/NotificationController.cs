using Leia.Api.Extensions;
using Leia.Application.Dtos.Common;
using Leia.Application.Helpers;
using Leia.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leia.Api.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationController(INotificationService notificationService) : ControllerBase
    {
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMy([FromQuery] PaginationDto dto)
        {
            var result = await notificationService.GetMyAsync(User.GetUserId(), dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpGet("unread-count")]
        [Authorize]
        public async Task<IActionResult> GetUnreadCount()
        {
            var result = await notificationService.GetUnreadCountAsync(User.GetUserId());
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPut("{id:guid}/read")]
        [Authorize]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            await notificationService.MarkAsReadAsync(User.GetUserId(), id);
            return Ok(ResponseModelHelper.SuccessResult("Marked as read."));
        }

        [HttpPut("read-all")]
        [Authorize]
        public async Task<IActionResult> MarkAllAsRead()
        {
            await notificationService.MarkAllAsReadAsync(User.GetUserId());
            return Ok(ResponseModelHelper.SuccessResult("All marked as read."));
        }
    }
}