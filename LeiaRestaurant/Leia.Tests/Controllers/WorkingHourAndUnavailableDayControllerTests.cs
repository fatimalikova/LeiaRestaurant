using Leia.Api.Controllers;
using Leia.Application.Dtos.RestaurantDtos;
using Leia.Application.Dtos.UnavailableDayDtos;
using Leia.Application.Dtos.WorkingHourDtos;
using Leia.Application.Exceptions;
using Leia.Application.Interfaces;
using Leia.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Leia.Tests.Controllers
{
    public class WorkingHourControllerTests
    {
        private readonly Mock<IWorkingHourService> _service = new();
        private readonly Mock<IRestaurantService> _restaurants = new();
        private readonly WorkingHourController _controller;
        private readonly Guid _userId = Guid.NewGuid();

        public WorkingHourControllerTests()
        {
            _controller = new WorkingHourController(_service.Object, _restaurants.Object).WithUser(_userId, "RestaurantOwner");
        }

        [Fact]
        public async Task GetByRestaurant_CallsService()
        {
            var id = Guid.NewGuid();

            var result = await _controller.GetByRestaurant(id);

            Assert.IsType<OkObjectResult>(result);
            _service.Verify(s => s.GetByRestaurantAsync(id), Times.Once);
        }

        [Fact]
        public async Task GetMy_ResolvesOwnRestaurantFirst()
        {
            var restaurantId = Guid.NewGuid();
            _restaurants.Setup(r => r.GetMyAsync(_userId)).ReturnsAsync(new ResponseRestaurantDto { Id = restaurantId });

            await _controller.GetMy();

            _service.Verify(s => s.GetByRestaurantAsync(restaurantId), Times.Once);
        }

        [Fact]
        public async Task GetMy_PropagatesNotFoundWhenNoRestaurant()
        {
            _restaurants.Setup(r => r.GetMyAsync(_userId)).ThrowsAsync(new NotFoundException("x"));

            await Assert.ThrowsAsync<NotFoundException>(() => _controller.GetMy());
        }

        [Fact]
        public async Task Create_ReturnsCreated()
        {
            var dto = new CreateWorkingHourDto();

            var result = await _controller.Create(dto);

            Assert.Equal(201, Assert.IsType<ObjectResult>(result).StatusCode);
            _service.Verify(s => s.CreateAsync(_userId, dto), Times.Once);
        }

        [Fact]
        public async Task Update_UsesUserIdAndId()
        {
            var id = Guid.NewGuid();
            var dto = new UpdateWorkingHourDto();

            await _controller.Update(id, dto);

            _service.Verify(s => s.UpdateAsync(_userId, id, dto), Times.Once);
        }

        [Fact]
        public async Task Delete_UsesUserIdAndId()
        {
            var id = Guid.NewGuid();

            var result = await _controller.Delete(id);

            Assert.IsType<OkObjectResult>(result);
            _service.Verify(s => s.DeleteAsync(_userId, id), Times.Once);
        }

        [Theory]
        [InlineData(nameof(WorkingHourController.GetMy))]
        [InlineData(nameof(WorkingHourController.Create))]
        [InlineData(nameof(WorkingHourController.Update))]
        [InlineData(nameof(WorkingHourController.Delete))]
        public void Owner_methods_are_restricted(string method)
        {
            Assert.Equal("RestaurantOwner", ControllerTestBase.RolesOf<WorkingHourController>(method));
        }
    }

    public class UnavailableDayControllerTests
    {
        private readonly Mock<IUnavailableDayService> _service = new();
        private readonly Mock<IRestaurantService> _restaurants = new();
        private readonly UnavailableDayController _controller;
        private readonly Guid _userId = Guid.NewGuid();

        public UnavailableDayControllerTests()
        {
            _controller = new UnavailableDayController(_service.Object, _restaurants.Object).WithUser(_userId, "RestaurantOwner");
        }

        [Fact]
        public async Task GetByRestaurant_CallsService()
        {
            var id = Guid.NewGuid();

            await _controller.GetByRestaurant(id);

            _service.Verify(s => s.GetByRestaurantAsync(id), Times.Once);
        }

        [Fact]
        public async Task GetMy_ResolvesOwnRestaurantFirst()
        {
            var restaurantId = Guid.NewGuid();
            _restaurants.Setup(r => r.GetMyAsync(_userId)).ReturnsAsync(new ResponseRestaurantDto { Id = restaurantId });

            await _controller.GetMy();

            _service.Verify(s => s.GetByRestaurantAsync(restaurantId), Times.Once);
        }

        [Fact]
        public async Task Create_ReturnsCreated()
        {
            var dto = new CreateUnavailableDayDto();

            var result = await _controller.Create(dto);

            Assert.Equal(201, Assert.IsType<ObjectResult>(result).StatusCode);
            _service.Verify(s => s.CreateAsync(_userId, dto), Times.Once);
        }

        [Fact]
        public async Task Delete_UsesUserIdAndId()
        {
            var id = Guid.NewGuid();

            await _controller.Delete(id);

            _service.Verify(s => s.DeleteAsync(_userId, id), Times.Once);
        }

        [Theory]
        [InlineData(nameof(UnavailableDayController.GetMy))]
        [InlineData(nameof(UnavailableDayController.Create))]
        [InlineData(nameof(UnavailableDayController.Delete))]
        public void Owner_methods_are_restricted(string method)
        {
            Assert.Equal("RestaurantOwner", ControllerTestBase.RolesOf<UnavailableDayController>(method));
        }
    }
}