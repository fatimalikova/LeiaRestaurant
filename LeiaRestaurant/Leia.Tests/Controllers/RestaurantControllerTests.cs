using Leia.Api.Controllers;
using Leia.Application.Dtos.Common;
using Leia.Application.Dtos.CuisineDtos;
using Leia.Application.Dtos.RestaurantDtos;
using Leia.Application.Dtos.RestaurantImageDtos;
using Leia.Application.Exceptions;
using Leia.Application.Interfaces;
using Leia.Domain.Enums;
using Leia.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Leia.Tests.Controllers
{
    public class RestaurantControllerTests
    {
        private readonly Mock<IRestaurantService> _service = new();
        private readonly RestaurantController _controller;
        private readonly Guid _userId = Guid.NewGuid();

        public RestaurantControllerTests()
        {
            _controller = new RestaurantController(_service.Object).WithUser(_userId, "RestaurantOwner");
        }

        [Fact]
        public async Task GetAll_PassesFilter()
        {
            var filter = new RestaurantFilterDto { Search = "burger" };

            var result = await _controller.GetAll(filter);

            Assert.IsType<OkObjectResult>(result);
            _service.Verify(s => s.GetAllAsync(filter), Times.Once);
        }

        [Fact]
        public async Task GetById_CallsService()
        {
            var id = Guid.NewGuid();

            var result = await _controller.GetById(id);

            Assert.IsType<OkObjectResult>(result);
            _service.Verify(s => s.GetByIdAsync(id), Times.Once);
        }

        [Fact]
        public async Task GetById_PropagatesNotFound()
        {
            _service.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ThrowsAsync(new NotFoundException("x"));

            await Assert.ThrowsAsync<NotFoundException>(() => _controller.GetById(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetMy_UsesUserId()
        {
            await _controller.GetMy();

            _service.Verify(s => s.GetMyAsync(_userId), Times.Once);
        }

        [Fact]
        public async Task Create_ReturnsCreated()
        {
            var dto = new CreateRestaurantDto();

            var result = await _controller.Create(dto);

            Assert.Equal(201, Assert.IsType<ObjectResult>(result).StatusCode);
            _service.Verify(s => s.CreateAsync(_userId, dto), Times.Once);
        }

        [Fact]
        public async Task Create_PropagatesConflict()
        {
            _service.Setup(s => s.CreateAsync(_userId, It.IsAny<CreateRestaurantDto>())).ThrowsAsync(new ConflictException("x"));

            await Assert.ThrowsAsync<ConflictException>(() => _controller.Create(new CreateRestaurantDto()));
        }

        [Fact]
        public async Task Update_UsesUserId()
        {
            var dto = new UpdateRestaurantDto();

            await _controller.Update(dto);

            _service.Verify(s => s.UpdateAsync(_userId, dto), Times.Once);
        }

        [Fact]
        public async Task SetCuisines_UsesUserId()
        {
            var dto = new SetRestaurantCuisinesDto();

            await _controller.SetCuisines(dto);

            _service.Verify(s => s.SetCuisinesAsync(_userId, dto), Times.Once);
        }

        [Fact]
        public async Task AddImage_ReturnsCreated()
        {
            var dto = new AddRestaurantImageDto();

            var result = await _controller.AddImage(dto);

            Assert.Equal(201, Assert.IsType<ObjectResult>(result).StatusCode);
            _service.Verify(s => s.AddImageAsync(_userId, dto), Times.Once);
        }

        [Fact]
        public async Task DeleteImage_UsesUserIdAndImageId()
        {
            var imageId = Guid.NewGuid();

            await _controller.DeleteImage(imageId);

            _service.Verify(s => s.DeleteImageAsync(_userId, imageId), Times.Once);
        }

        [Fact]
        public async Task GetAllForAdmin_PassesPagingAndStatus()
        {
            var paging = new PaginationDto();

            await _controller.GetAllForAdmin(paging, RestaurantStatus.Pending);

            _service.Verify(s => s.GetAllForAdminAsync(paging, RestaurantStatus.Pending), Times.Once);
        }

        [Fact]
        public async Task UpdateStatus_PassesIdAndDto()
        {
            var id = Guid.NewGuid();
            var dto = new UpdateRestaurantStatusDto { Status = RestaurantStatus.Approved };

            await _controller.UpdateStatus(id, dto);

            _service.Verify(s => s.UpdateStatusAsync(id, dto), Times.Once);
        }

        [Theory]
        [InlineData(nameof(RestaurantController.GetMy), "RestaurantOwner")]
        [InlineData(nameof(RestaurantController.Create), "RestaurantOwner")]
        [InlineData(nameof(RestaurantController.Update), "RestaurantOwner")]
        [InlineData(nameof(RestaurantController.SetCuisines), "RestaurantOwner")]
        [InlineData(nameof(RestaurantController.AddImage), "RestaurantOwner")]
        [InlineData(nameof(RestaurantController.DeleteImage), "RestaurantOwner")]
        [InlineData(nameof(RestaurantController.GetAllForAdmin), "Admin")]
        [InlineData(nameof(RestaurantController.UpdateStatus), "Admin")]
        public void Methods_AreRestrictedToTheRightRole(string method, string role)
        {
            Assert.Equal(role, ControllerTestBase.RolesOf<RestaurantController>(method));
        }

        [Theory]
        [InlineData(nameof(RestaurantController.GetAll))]
        [InlineData(nameof(RestaurantController.GetById))]
        public void Public_methods_are_anonymous(string method)
        {
            Assert.True(ControllerTestBase.IsAnonymous<RestaurantController>(method));
        }
    }
}