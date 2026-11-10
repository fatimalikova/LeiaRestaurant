using Leia.Api.Controllers;
using Leia.Application.Dtos.PaymentDtos;
using Leia.Application.Exceptions;
using Leia.Application.Interfaces;
using Leia.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Leia.Tests.Controllers
{
    public class PaymentControllerTests
    {
        private readonly Mock<IPaymentService> _service = new();
        private readonly PaymentController _controller;
        private readonly Guid _userId = Guid.NewGuid();

        public PaymentControllerTests()
        {
            _controller = new PaymentController(_service.Object).WithUser(_userId, "Customer");
        }

        [Fact]
        public async Task Pay_ReturnsOk_AndCallsServiceWithUserId()
        {
            var dto = new PayOrderDto { OrderId = Guid.NewGuid() };
            _service.Setup(s => s.PayAsync(_userId, dto)).ReturnsAsync(new ResponsePaymentDto());

            var result = await _controller.Pay(dto);

            Assert.IsType<OkObjectResult>(result);
            Assert.True(ControllerTestBase.Body<ResponsePaymentDto>(result).Success);
            _service.Verify(s => s.PayAsync(_userId, dto), Times.Once);
        }

        [Fact]
        public async Task Pay_PropagatesFailure()
        {
            var dto = new PayOrderDto();
            _service.Setup(s => s.PayAsync(_userId, dto)).ThrowsAsync(new BadRequestException("Payment failed: Card was declined."));

            await Assert.ThrowsAsync<BadRequestException>(() => _controller.Pay(dto));
        }

        [Fact]
        public async Task GetByOrder_CallsService()
        {
            var orderId = Guid.NewGuid();
            _service.Setup(s => s.GetByOrderAsync(_userId, orderId)).ReturnsAsync(new ResponsePaymentDto());

            var result = await _controller.GetByOrder(orderId);

            Assert.IsType<OkObjectResult>(result);
            _service.Verify(s => s.GetByOrderAsync(_userId, orderId), Times.Once);
        }

        [Fact]
        public async Task Refund_PassesReasonToService()
        {
            var orderId = Guid.NewGuid();
            _service.Setup(s => s.RefundAsync(orderId, "duplicate")).ReturnsAsync(new ResponsePaymentDto());

            var result = await _controller.Refund(orderId, new RefundPaymentDto { Reason = "duplicate" });

            Assert.IsType<OkObjectResult>(result);
            _service.Verify(s => s.RefundAsync(orderId, "duplicate"), Times.Once);
        }

        [Theory]
        [InlineData(nameof(PaymentController.Pay), "Customer")]
        [InlineData(nameof(PaymentController.Refund), "Admin")]
        public void Methods_AreRestrictedToTheRightRole(string method, string role)
        {
            Assert.Equal(role, ControllerTestBase.RolesOf<PaymentController>(method));
        }
    }
}