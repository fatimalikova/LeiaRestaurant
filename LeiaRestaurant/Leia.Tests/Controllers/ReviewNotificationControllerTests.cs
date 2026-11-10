using Leia.Api.Controllers;
using Leia.Application.Dtos.Common;
using Leia.Application.Dtos.ReviewDtos;
using Leia.Application.Exceptions;
using Leia.Application.Interfaces;
using Leia.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Leia.Tests.Controllers
{
    public class ReviewControllerTests
    {
        private readonly Mock<IReviewService> _service = new();
        private readonly ReviewController _controller;
        private readonly Guid _userId = Guid.NewGuid();

        public ReviewControllerTests()
        {
            _controller = new ReviewController(_service.Object).WithUser(_userId, "Customer");
        }

        [Fact]
        public async Task GetByRestaurant_PassesIdAndPaging()
        {
            var id = Guid.NewGuid();
            var paging = new PaginationDto();

            await _controller.GetByRestaurant(id, paging);

            _service.Verify(s => s.GetByRestaurantAsync(id, paging), Times.Once);
        }

        [Fact]
        public async Task Create_ReturnsCreated()
        {
            var dto = new CreateReviewDto();

            var result = await _controller.Create(dto);

            Assert.Equal(201, Assert.IsType<ObjectResult>(result).StatusCode);
            _service.Verify(s => s.CreateAsync(_userId, dto), Times.Once);
        }

        [Fact]
        public async Task Create_PropagatesConflictWhenAlreadyReviewed()
        {
            _service.Setup(s => s.CreateAsync(_userId, It.IsAny<CreateReviewDto>())).ThrowsAsync(new ConflictException("x"));

            await Assert.ThrowsAsync<ConflictException>(() => _controller.Create(new CreateReviewDto()));
        }

        [Fact]
        public async Task Reply_ReturnsCreated()
        {
            var id = Guid.NewGuid();
            var dto = new CreateReviewReplyDto();

            var result = await _controller.Reply(id, dto);

            Assert.Equal(201, Assert.IsType<ObjectResult>(result).StatusCode);
            _service.Verify(s => s.ReplyAsync(_userId, id, dto), Times.Once);
        }

        [Fact]
        public async Task DeleteReply_CallsService()
        {
            var id = Guid.NewGuid();

            await _controller.DeleteReply(id);

            _service.Verify(s => s.DeleteReplyAsync(_userId, id), Times.Once);
        }

        [Theory]
        [InlineData(nameof(ReviewController.Create), "Customer")]
        [InlineData(nameof(ReviewController.Reply), "RestaurantOwner")]
        [InlineData(nameof(ReviewController.DeleteReply), "RestaurantOwner")]
        public void Methods_AreRestrictedToTheRightRole(string method, string role)
        {
            Assert.Equal(role, ControllerTestBase.RolesOf<ReviewController>(method));
        }
    }

    public class NotificationControllerTests
    {
        private readonly Mock<INotificationService> _service = new();
        private readonly NotificationController _controller;
        private readonly Guid _userId = Guid.NewGuid();

        public NotificationControllerTests()
        {
            _controller = new NotificationController(_service.Object).WithUser(_userId, "Customer");
        }

        [Fact]
        public async Task GetMy_UsesUserId()
        {
            var paging = new PaginationDto();

            await _controller.GetMy(paging);

            _service.Verify(s => s.GetMyAsync(_userId, paging), Times.Once);
        }

        [Fact]
        public async Task GetUnreadCount_ReturnsCount()
        {
            _service.Setup(s => s.GetUnreadCountAsync(_userId)).ReturnsAsync(7);

            var result = await _controller.GetUnreadCount();

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(7, ControllerTestBase.Body<int>(result).Data);
        }

        [Fact]
        public async Task MarkAsRead_UsesUserIdAndId()
        {
            var id = Guid.NewGuid();

            await _controller.MarkAsRead(id);

            _service.Verify(s => s.MarkAsReadAsync(_userId, id), Times.Once);
        }

        [Fact]
        public async Task MarkAsRead_PropagatesNotFoundForForeignNotification()
        {
            _service.Setup(s => s.MarkAsReadAsync(_userId, It.IsAny<Guid>())).ThrowsAsync(new NotFoundException("x"));

            await Assert.ThrowsAsync<NotFoundException>(() => _controller.MarkAsRead(Guid.NewGuid()));
        }

        [Fact]
        public async Task MarkAllAsRead_UsesUserId()
        {
            await _controller.MarkAllAsRead();

            _service.Verify(s => s.MarkAllAsReadAsync(_userId), Times.Once);
        }

        [Theory]
        [InlineData(nameof(NotificationController.GetMy))]
        [InlineData(nameof(NotificationController.GetUnreadCount))]
        [InlineData(nameof(NotificationController.MarkAsRead))]
        [InlineData(nameof(NotificationController.MarkAllAsRead))]
        public void Methods_RequireAuthentication(string method)
        {
            var attr = typeof(NotificationController).GetMethod(method)!
                .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true);

            Assert.NotEmpty(attr);
        }
    }
}