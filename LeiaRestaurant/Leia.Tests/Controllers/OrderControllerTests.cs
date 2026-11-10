using Leia.Api.Controllers;
using Leia.Application.Dtos.Common;
using Leia.Application.Dtos.OrderDtos;
using Leia.Application.Exceptions;
using Leia.Application.Interfaces;
using Leia.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Leia.Tests.Controllers
{
    public class OrderControllerTests
    {
        private readonly Mock<IOrderService> _service = new();
        private readonly OrderController _controller;
        private readonly Guid _userId = Guid.NewGuid();

        public OrderControllerTests()
        {
            _controller = new OrderController(_service.Object).WithUser(_userId, "Customer");
        }

        [Fact]
        public async Task Create_ReturnsCreated_AndCallsServiceWithUserId()
        {
            var dto = new CreateOrderDto();
            _service.Setup(s => s.CreateAsync(_userId, dto)).ReturnsAsync(new ResponseOrderDto());

            var result = await _controller.Create(dto);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, obj.StatusCode);
            Assert.True(ControllerTestBase.Body<ResponseOrderDto>(result).Success);
            _service.Verify(s => s.CreateAsync(_userId, dto), Times.Once);
        }

        [Fact]
        public async Task Create_PropagatesServiceException()
        {
            var dto = new CreateOrderDto();
            _service.Setup(s => s.CreateAsync(_userId, dto)).ThrowsAsync(new BadRequestException("closed"));

            await Assert.ThrowsAsync<BadRequestException>(() => _controller.Create(dto));
        }

        [Fact]
        public async Task GetMy_ReturnsOk_AndPassesFilter()
        {
            var filter = new OrderFilterDto { Page = 2 };
            _service.Setup(s => s.GetMyOrdersAsync(_userId, filter)).ReturnsAsync(new PagedResultDto<ResponseOrderDto>());

            var result = await _controller.GetMy(filter);

            Assert.IsType<OkObjectResult>(result);
            Assert.True(ControllerTestBase.Body<PagedResultDto<ResponseOrderDto>>(result).Success);
            _service.Verify(s => s.GetMyOrdersAsync(_userId, filter), Times.Once);
        }

        [Fact]
        public async Task Cancel_CallsServiceWithUserIdAndOrderId()
        {
            var id = Guid.NewGuid();
            var dto = new CancelOrderDto { Reason = "x" };
            _service.Setup(s => s.CancelAsync(_userId, id, dto)).ReturnsAsync(new ResponseOrderDto());

            var result = await _controller.Cancel(id, dto);

            Assert.IsType<OkObjectResult>(result);
            _service.Verify(s => s.CancelAsync(_userId, id, dto), Times.Once);
        }

        [Fact]
        public async Task UpdateStatus_CallsServiceWithOwnerIdAndOrderId()
        {
            var id = Guid.NewGuid();
            var dto = new UpdateOrderStatusDto();
            _service.Setup(s => s.UpdateStatusAsync(_userId, id, dto)).ReturnsAsync(new ResponseOrderDto());

            var result = await _controller.UpdateStatus(id, dto);

            Assert.IsType<OkObjectResult>(result);
            _service.Verify(s => s.UpdateStatusAsync(_userId, id, dto), Times.Once);
        }

        [Fact]
        public async Task GetRestaurantOrders_CallsService()
        {
            var filter = new OrderFilterDto();
            _service.Setup(s => s.GetRestaurantOrdersAsync(_userId, filter)).ReturnsAsync(new PagedResultDto<ResponseOrderDto>());

            var result = await _controller.GetRestaurantOrders(filter);

            Assert.IsType<OkObjectResult>(result);
            _service.Verify(s => s.GetRestaurantOrdersAsync(_userId, filter), Times.Once);
        }

        [Fact]
        public async Task GetAllForAdmin_CallsService()
        {
            var filter = new OrderFilterDto();
            _service.Setup(s => s.GetAllForAdminAsync(filter)).ReturnsAsync(new PagedResultDto<ResponseOrderDto>());

            var result = await _controller.GetAllForAdmin(filter);

            Assert.IsType<OkObjectResult>(result);
            _service.Verify(s => s.GetAllForAdminAsync(filter), Times.Once);
        }

        [Fact]
        public async Task GetById_CallsServiceWithUserIdAndId()
        {
            var id = Guid.NewGuid();
            _service.Setup(s => s.GetByIdAsync(_userId, id)).ReturnsAsync(new ResponseOrderDto());

            var result = await _controller.GetById(id);

            Assert.IsType<OkObjectResult>(result);
            _service.Verify(s => s.GetByIdAsync(_userId, id), Times.Once);
        }

        [Theory]
        [InlineData(nameof(OrderController.Create), "Customer")]
        [InlineData(nameof(OrderController.GetMy), "Customer")]
        [InlineData(nameof(OrderController.Cancel), "Customer")]
        [InlineData(nameof(OrderController.GetRestaurantOrders), "RestaurantOwner")]
        [InlineData(nameof(OrderController.UpdateStatus), "RestaurantOwner")]
        [InlineData(nameof(OrderController.GetAllForAdmin), "Admin")]
        public void Methods_AreRestrictedToTheRightRole(string method, string role)
        {
            Assert.Equal(role, ControllerTestBase.RolesOf<OrderController>(method));
        }
    }
}