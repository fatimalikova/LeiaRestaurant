using Leia.Api.Controllers;
using Leia.Application.Dtos.Common;
using Leia.Application.Dtos.OrderDtos;
using Leia.Application.Dtos.PaymentDtos;
using Leia.Application.Dtos.ReservationDtos;
using Leia.Application.Exceptions;
using Leia.Application.Interfaces;
using Leia.Tests.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Reflection;
using Xunit;

namespace Leia.Tests.Controllers
{
    public class ReservationControllerEndpointTests
    {
        private readonly Mock<IReservationService> _service = new();
        private readonly Guid _userId = Guid.NewGuid();

        private ReservationController As(params string[] roles) =>
            new ReservationController(_service.Object).WithUser(_userId, roles);

        [Fact]
        public async Task GetRestaurantReservations_UsesOwnerId()
        {
            var filter = new ReservationFilterDto();
            _service.Setup(s => s.GetRestaurantReservationsAsync(_userId, filter))
                .ReturnsAsync(new PagedResultDto<ResponseReservationDto>());

            var result = await As("RestaurantOwner").GetRestaurantReservations(filter);

            Assert.IsType<OkObjectResult>(result);
            _service.Verify(s => s.GetRestaurantReservationsAsync(_userId, filter), Times.Once);
        }

        [Fact]
        public async Task GetSettings_UsesOwnerId()
        {
            _service.Setup(s => s.GetSettingsAsync(_userId)).ReturnsAsync(new ReservationSettingsDto());

            var result = await As("RestaurantOwner").GetSettings();

            Assert.IsType<OkObjectResult>(result);
            _service.Verify(s => s.GetSettingsAsync(_userId), Times.Once);
        }

        [Fact]
        public async Task GetSettings_PropagatesNotFoundWhenNoRestaurant()
        {
            _service.Setup(s => s.GetSettingsAsync(_userId)).ThrowsAsync(new NotFoundException("x"));

            await Assert.ThrowsAsync<NotFoundException>(() => As("RestaurantOwner").GetSettings());
        }

        [Fact]
        public async Task UpdateSettings_PropagatesBadRequestWithoutTables()
        {
            _service.Setup(s => s.UpdateSettingsAsync(_userId, It.IsAny<ReservationSettingsDto>()))
                .ThrowsAsync(new BadRequestException("no tables"));

            await Assert.ThrowsAsync<BadRequestException>(() =>
                As("RestaurantOwner").UpdateSettings(new ReservationSettingsDto()));
        }

        [Fact]
        public async Task GetAllForAdmin_PassesFilter()
        {
            var filter = new ReservationFilterDto();
            _service.Setup(s => s.GetAllForAdminAsync(filter)).ReturnsAsync(new PagedResultDto<ResponseReservationDto>());

            var result = await As("Admin").GetAllForAdmin(filter);

            Assert.IsType<OkObjectResult>(result);
            _service.Verify(s => s.GetAllForAdminAsync(filter), Times.Once);
        }

        [Fact]
        public async Task GetById_UsesUserIdAndId()
        {
            var id = Guid.NewGuid();
            _service.Setup(s => s.GetByIdAsync(_userId, id)).ReturnsAsync(new ResponseReservationDto());

            var result = await As("Customer").GetById(id);

            Assert.IsType<OkObjectResult>(result);
            _service.Verify(s => s.GetByIdAsync(_userId, id), Times.Once);
        }

        [Fact]
        public async Task GetById_PropagatesForbidden()
        {
            _service.Setup(s => s.GetByIdAsync(_userId, It.IsAny<Guid>())).ThrowsAsync(new ForbiddenException("x"));

            await Assert.ThrowsAsync<ForbiddenException>(() => As("Customer").GetById(Guid.NewGuid()));
        }

        [Fact]
        public async Task Create_PropagatesBadRequestWhenNoTable()
        {
            _service.Setup(s => s.CreateAsync(_userId, It.IsAny<CreateReservationDto>()))
                .ThrowsAsync(new BadRequestException("no table"));

            await Assert.ThrowsAsync<BadRequestException>(() => As("Customer").Create(new CreateReservationDto()));
        }

        [Theory]
        [InlineData(nameof(ReservationController.GetMy), "Customer")]
        [InlineData(nameof(ReservationController.GetRestaurantReservations), "RestaurantOwner")]
        [InlineData(nameof(ReservationController.GetSettings), "RestaurantOwner")]
        public void Read_methods_are_restricted_to_the_right_role(string method, string role)
        {
            Assert.Equal(role, ControllerTestBase.RolesOf<ReservationController>(method));
        }

        [Fact]
        public void GetById_requires_authentication()
        {
            var attr = typeof(ReservationController).GetMethod(nameof(ReservationController.GetById))!
                .GetCustomAttributes<AuthorizeAttribute>();

            Assert.NotEmpty(attr);
        }
    }

    public class OrderAndPaymentReadEndpointTests
    {
        [Fact]
        public async Task Order_GetById_requires_authentication_and_uses_user_id()
        {
            var service = new Mock<IOrderService>();
            var userId = Guid.NewGuid();
            var id = Guid.NewGuid();
            service.Setup(s => s.GetByIdAsync(userId, id)).ReturnsAsync(new ResponseOrderDto());
            var controller = new OrderController(service.Object).WithUser(userId, "Customer");

            var result = await controller.GetById(id);

            Assert.IsType<OkObjectResult>(result);
            Assert.NotEmpty(typeof(OrderController).GetMethod(nameof(OrderController.GetById))!
                .GetCustomAttributes<AuthorizeAttribute>());
        }

        [Fact]
        public async Task Order_GetById_propagates_forbidden()
        {
            var service = new Mock<IOrderService>();
            var userId = Guid.NewGuid();
            service.Setup(s => s.GetByIdAsync(userId, It.IsAny<Guid>())).ThrowsAsync(new ForbiddenException("x"));
            var controller = new OrderController(service.Object).WithUser(userId, "Customer");

            await Assert.ThrowsAsync<ForbiddenException>(() => controller.GetById(Guid.NewGuid()));
        }

        [Fact]
        public void Payment_GetByOrder_requires_authentication()
        {
            Assert.NotEmpty(typeof(PaymentController).GetMethod(nameof(PaymentController.GetByOrder))!
                .GetCustomAttributes<AuthorizeAttribute>());
        }

        [Fact]
        public async Task Payment_GetByOrder_propagates_forbidden()
        {
            var service = new Mock<IPaymentService>();
            var userId = Guid.NewGuid();
            service.Setup(s => s.GetByOrderAsync(userId, It.IsAny<Guid>())).ThrowsAsync(new ForbiddenException("x"));
            var controller = new PaymentController(service.Object).WithUser(userId, "Customer");

            await Assert.ThrowsAsync<ForbiddenException>(() => controller.GetByOrder(Guid.NewGuid()));
        }

        [Fact]
        public async Task Payment_Refund_propagates_bad_request_when_not_paid()
        {
            var service = new Mock<IPaymentService>();
            service.Setup(s => s.RefundAsync(It.IsAny<Guid>(), It.IsAny<string?>()))
                .ThrowsAsync(new BadRequestException("not paid"));
            var controller = new PaymentController(service.Object).WithUser(Guid.NewGuid(), "Admin");

            await Assert.ThrowsAsync<BadRequestException>(() =>
                controller.Refund(Guid.NewGuid(), new RefundPaymentDto()));
        }
    }
}