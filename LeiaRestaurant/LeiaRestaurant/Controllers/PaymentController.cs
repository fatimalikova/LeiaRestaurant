using Leia.Api.Extensions;
using Leia.Application.Dtos.PaymentDtos;
using Leia.Application.Helpers;
using Leia.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leia.Api.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentController(IPaymentService paymentService) : ControllerBase
    {
        [HttpPost("pay")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Pay(PayOrderDto dto)
        {
            var result = await paymentService.PayAsync(User.GetUserId(), dto);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpGet("order/{orderId:guid}")]
        [Authorize]
        public async Task<IActionResult> GetByOrder(Guid orderId)
        {
            var result = await paymentService.GetByOrderAsync(User.GetUserId(), orderId);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }

        [HttpPost("order/{orderId:guid}/refund")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Refund(Guid orderId, RefundPaymentDto dto)
        {
            var result = await paymentService.RefundAsync(orderId, dto.Reason);
            return Ok(ResponseModelHelper.SuccessResult(result));
        }
    }
}