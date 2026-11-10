using Leia.Api.Controllers;
using Leia.Application.Dtos.AddressDtos;
using Leia.Application.Dtos.Common;
using Leia.Application.Dtos.DashboardDtos;
using Leia.Application.Dtos.TableDtos;
using Leia.Application.Exceptions;
using Leia.Application.Interfaces;
using Leia.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Leia.Tests.Controllers
{
    public class AddressControllerTests
    {
        private readonly Mock<IAddressService> _service = new();
        private readonly AddressController _controller;
        private readonly Guid _userId = Guid.NewGuid();

        public AddressControllerTests()
        {
            _controller = new AddressController(_service.Object).WithUser(_userId, "Customer");
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
            var dto = new CreateAddressDto();

            var result = await _controller.Create(dto);

            Assert.Equal(201, Assert.IsType<ObjectResult>(result).StatusCode);
            _service.Verify(s => s.CreateAsync(_userId, dto), Times.Once);
        }

        [Fact]
        public async Task Create_PropagatesLimitError()
        {
            _service.Setup(s => s.CreateAsync(_userId, It.IsAny<CreateAddressDto>())).ThrowsAsync(new BadRequestException("max"));

            await Assert.ThrowsAsync<BadRequestException>(() => _controller.Create(new CreateAddressDto()));
        }

        [Fact]
        public async Task Update_SetDefault_Delete_UseUserIdAndId()
        {
            var id = Guid.NewGuid();
            var dto = new UpdateAddressDto();

            await _controller.Update(id, dto);
            await _controller.SetDefault(id);
            await _controller.Delete(id);

            _service.Verify(s => s.UpdateAsync(_userId, id, dto), Times.Once);
            _service.Verify(s => s.SetDefaultAsync(_userId, id), Times.Once);
            _service.Verify(s => s.DeleteAsync(_userId, id), Times.Once);
        }

        [Theory]
        [InlineData(nameof(AddressController.GetMy))]
        [InlineData(nameof(AddressController.Create))]
        [InlineData(nameof(AddressController.Update))]
        [InlineData(nameof(AddressController.SetDefault))]
        [InlineData(nameof(AddressController.Delete))]
        public void Is_customer_only(string method)
        {
            Assert.Equal("Customer", ControllerTestBase.RolesOf<AddressController>(method));
        }
    }

    public class FavoriteControllerTests
    {
        private readonly Mock<IFavoriteService> _service = new();
        private readonly FavoriteController _controller;
        private readonly Guid _userId = Guid.NewGuid();

        public FavoriteControllerTests()
        {
            _controller = new FavoriteController(_service.Object).WithUser(_userId, "Customer");
        }

        [Fact]
        public async Task GetMy_PassesPaging()
        {
            var paging = new PaginationDto();

            await _controller.GetMy(paging);

            _service.Verify(s => s.GetMyAsync(_userId, paging), Times.Once);
        }

        [Fact]
        public async Task GetMyIds_UsesUserId()
        {
            await _controller.GetMyIds();

            _service.Verify(s => s.GetMyIdsAsync(_userId), Times.Once);
        }

        [Fact]
        public async Task Add_And_Remove_UseUserIdAndRestaurantId()
        {
            var restaurantId = Guid.NewGuid();

            await _controller.Add(restaurantId);
            await _controller.Remove(restaurantId);

            _service.Verify(s => s.AddAsync(_userId, restaurantId), Times.Once);
            _service.Verify(s => s.RemoveAsync(_userId, restaurantId), Times.Once);
        }

        [Fact]
        public async Task Add_PropagatesNotFound()
        {
            _service.Setup(s => s.AddAsync(_userId, It.IsAny<Guid>())).ThrowsAsync(new NotFoundException("x"));

            await Assert.ThrowsAsync<NotFoundException>(() => _controller.Add(Guid.NewGuid()));
        }

        [Theory]
        [InlineData(nameof(FavoriteController.GetMy))]
        [InlineData(nameof(FavoriteController.GetMyIds))]
        [InlineData(nameof(FavoriteController.Add))]
        [InlineData(nameof(FavoriteController.Remove))]
        public void Is_customer_only(string method)
        {
            Assert.Equal("Customer", ControllerTestBase.RolesOf<FavoriteController>(method));
        }
    }

    public class TableControllerTests
    {
        private readonly Mock<ITableService> _service = new();
        private readonly TableController _controller;
        private readonly Guid _userId = Guid.NewGuid();

        public TableControllerTests()
        {
            _controller = new TableController(_service.Object).WithUser(_userId, "RestaurantOwner");
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
            var dto = new CreateTableDto();

            var result = await _controller.Create(dto);

            Assert.Equal(201, Assert.IsType<ObjectResult>(result).StatusCode);
            _service.Verify(s => s.CreateAsync(_userId, dto), Times.Once);
        }

        [Fact]
        public async Task Update_UsesUserIdAndId()
        {
            var id = Guid.NewGuid();
            var dto = new UpdateTableDto();

            await _controller.Update(id, dto);

            _service.Verify(s => s.UpdateAsync(_userId, id, dto), Times.Once);
        }

        [Fact]
        public async Task Delete_PropagatesBadRequestWhenUpcomingReservations()
        {
            var id = Guid.NewGuid();
            _service.Setup(s => s.DeleteAsync(_userId, id)).ThrowsAsync(new BadRequestException("upcoming"));

            await Assert.ThrowsAsync<BadRequestException>(() => _controller.Delete(id));
        }

        [Theory]
        [InlineData(nameof(TableController.GetMy))]
        [InlineData(nameof(TableController.Create))]
        [InlineData(nameof(TableController.Update))]
        [InlineData(nameof(TableController.Delete))]
        public void Is_owner_only(string method)
        {
            Assert.Equal("RestaurantOwner", ControllerTestBase.RolesOf<TableController>(method));
        }
    }

    public class DashboardControllerTests
    {
        private readonly Mock<IDashboardService> _service = new();
        private readonly DashboardController _controller;
        private readonly Guid _userId = Guid.NewGuid();

        public DashboardControllerTests()
        {
            _controller = new DashboardController(_service.Object).WithUser(_userId, "RestaurantOwner");
        }

        [Fact]
        public async Task GetRestaurant_UsesOwnerId()
        {
            var query = new DashboardQueryDto();

            var result = await _controller.GetRestaurant(query);

            Assert.IsType<OkObjectResult>(result);
            _service.Verify(s => s.GetRestaurantAsync(_userId, query), Times.Once);
        }

        [Fact]
        public async Task GetRestaurant_PropagatesBadRequestForLongPeriod()
        {
            _service.Setup(s => s.GetRestaurantAsync(_userId, It.IsAny<DashboardQueryDto>())).ThrowsAsync(new BadRequestException("92"));

            await Assert.ThrowsAsync<BadRequestException>(() => _controller.GetRestaurant(new DashboardQueryDto()));
        }

        [Fact]
        public async Task GetAdmin_PassesQuery()
        {
            var query = new DashboardQueryDto();

            await _controller.GetAdmin(query);

            _service.Verify(s => s.GetAdminAsync(query), Times.Once);
        }

        [Theory]
        [InlineData(nameof(DashboardController.GetRestaurant), "RestaurantOwner")]
        [InlineData(nameof(DashboardController.GetAdmin), "Admin")]
        public void Methods_AreRestrictedToTheRightRole(string method, string role)
        {
            Assert.Equal(role, ControllerTestBase.RolesOf<DashboardController>(method));
        }
    }

    public class UploadControllerTests
    {
        private readonly Mock<IFileUploadService> _service = new();
        private readonly UploadController _controller;

        public UploadControllerTests()
        {
            _controller = new UploadController(_service.Object).WithUser(Guid.NewGuid(), "Customer");
        }

        private static IFormFile File(long length, string contentType, string name = "a.png")
        {
            var mock = new Mock<IFormFile>();
            mock.Setup(f => f.Length).Returns(length);
            mock.Setup(f => f.ContentType).Returns(contentType);
            mock.Setup(f => f.FileName).Returns(name);
            mock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[Math.Max(1, (int)Math.Min(length, 16))]));
            return mock.Object;
        }

        [Fact]
        public async Task NullFile_IsRejected()
        {
            await Assert.ThrowsAsync<BadRequestException>(() => _controller.UploadImage(null!));
        }

        [Fact]
        public async Task EmptyFile_IsRejected()
        {
            await Assert.ThrowsAsync<BadRequestException>(() => _controller.UploadImage(File(0, "image/png")));
        }

        [Fact]
        public async Task FileOver5Mb_IsRejected()
        {
            await Assert.ThrowsAsync<BadRequestException>(() => _controller.UploadImage(File(5 * 1024 * 1024 + 1, "image/png")));
        }

        [Theory]
        [InlineData("application/pdf")]
        [InlineData("text/html")]
        [InlineData("image/gif")]
        public async Task NonAllowedType_IsRejected(string contentType)
        {
            await Assert.ThrowsAsync<BadRequestException>(() => _controller.UploadImage(File(100, contentType)));
            _service.Verify(s => s.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData("image/jpeg")]
        [InlineData("image/png")]
        [InlineData("image/webp")]
        public async Task AllowedImage_IsUploaded(string contentType)
        {
            _service.Setup(s => s.UploadImageAsync(It.IsAny<Stream>(), "a.png")).ReturnsAsync("https://img/x.png");

            var result = await _controller.UploadImage(File(100, contentType));

            Assert.IsType<OkObjectResult>(result);
            _service.Verify(s => s.UploadImageAsync(It.IsAny<Stream>(), "a.png"), Times.Once);
        }

        [Fact]
        public void Requires_authentication()
        {
            var attr = typeof(UploadController).GetMethod(nameof(UploadController.UploadImage))!
                .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true);

            Assert.NotEmpty(attr);
        }
    }
}