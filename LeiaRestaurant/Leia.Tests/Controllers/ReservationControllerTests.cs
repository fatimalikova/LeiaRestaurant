using Leia.Api.Controllers;
using Leia.Application.Dtos.Common;
using Leia.Application.Dtos.ReservationDtos;
using Leia.Application.Interfaces;
using Leia.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Leia.Tests.Controllers
{
    public class ReservationControllerTests
    {
        private readonly Mock<IReservationService> _service = new();
        private readonly ReservationController _controller;
        private readonly Guid _userId = Guid.NewGuid();

        public ReservationControllerTests()
        {
            _controller = new ReservationController(_service.Object).WithUser(_userId, "Customer");
        }

        [Fact]
        public async Task GetAvailability_IsAnonymous_AndReturnsOk()
        {
            var query = new AvailabilityQueryDto();
            _service.Setup(s => s.GetAvailabilityAsync(query)).ReturnsAsync(new ResponseAvailabilityDto());

            var result = await _controller.GetAvailability(query);

            Assert.IsType<OkObjectResult>(result);
            Assert.True(ControllerTestBase.IsAnonymous<ReservationController>(nameof(ReservationController.GetAvailability)));
        }

        [Fact]
        public async Task Create_ReturnsCreated_AndCallsServiceWithUserId()
        {
            var dto = new CreateReservationDto();
            _service.Setup(s => s.CreateAsync(_userId, dto)).ReturnsAsync(new ResponseReservationDto());

            var result = await _controller.Create(dto);

            Assert.Equal(201, Assert.IsType<ObjectResult>(result).StatusCode);
            _service.Verify(s => s.CreateAsync(_userId, dto), Times.Once);
        }

        [Fact]
        public async Task Cancel_CallsServiceWithUserIdAndId()
        {
            var id = Guid.NewGuid();
            var dto = new CancelReservationDto();
            _service.Setup(s => s.CancelAsync(_userId, id, dto)).ReturnsAsync(new ResponseReservationDto());

            var result = await _controller.Cancel(id, dto);

            Assert.IsType<OkObjectResult>(result);
            _service.Verify(s => s.CancelAsync(_userId, id, dto), Times.Once);
        }

        [Fact]
        public async Task Reschedule_CallsServiceWithUserIdAndId()
        {
            var id = Guid.NewGuid();
            var dto = new RescheduleReservationDto();
            _service.Setup(s => s.RescheduleAsync(_userId, id, dto)).ReturnsAsync(new ResponseReservationDto());

            var result = await _controller.Reschedule(id, dto);

            Assert.IsType<OkObjectResult>(result);
            _service.Verify(s => s.RescheduleAsync(_userId, id, dto), Times.Once);
        }

        [Fact]
        public async Task UpdateStatus_CallsServiceWithOwnerId()
        {
            var id = Guid.NewGuid();
            var dto = new UpdateReservationStatusDto();
            _service.Setup(s => s.UpdateStatusAsync(_userId, id, dto)).ReturnsAsync(new ResponseReservationDto());

            var result = await _controller.UpdateStatus(id, dto);

            Assert.IsType<OkObjectResult>(result);
            _service.Verify(s => s.UpdateStatusAsync(_userId, id, dto), Times.Once);
        }

        [Fact]
        public async Task UpdateSettings_CallsServiceWithOwnerId()
        {
            var dto = new ReservationSettingsDto();
            _service.Setup(s => s.UpdateSettingsAsync(_userId, dto)).ReturnsAsync(dto);

            var result = await _controller.UpdateSettings(dto);

            Assert.IsType<OkObjectResult>(result);
            _service.Verify(s => s.UpdateSettingsAsync(_userId, dto), Times.Once);
        }

        [Fact]
        public async Task GetMy_CallsService()
        {
            var filter = new ReservationFilterDto();
            _service.Setup(s => s.GetMyAsync(_userId, filter)).ReturnsAsync(new PagedResultDto<ResponseReservationDto>());

            var result = await _controller.GetMy(filter);

            Assert.IsType<OkObjectResult>(result);
            _service.Verify(s => s.GetMyAsync(_userId, filter), Times.Once);
        }

        [Theory]
        [InlineData(nameof(ReservationController.Create), "Customer")]
        [InlineData(nameof(ReservationController.Cancel), "Customer")]
        [InlineData(nameof(ReservationController.Reschedule), "Customer")]
        [InlineData(nameof(ReservationController.UpdateStatus), "RestaurantOwner")]
        [InlineData(nameof(ReservationController.UpdateSettings), "RestaurantOwner")]
        [InlineData(nameof(ReservationController.GetAllForAdmin), "Admin")]
        public void Methods_AreRestrictedToTheRightRole(string method, string role)
        {
            Assert.Equal(role, ControllerTestBase.RolesOf<ReservationController>(method));
        }
    }
}