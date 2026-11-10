using Leia.Api.Controllers;
using Leia.Application.Dtos.AllergenDtos;
using Leia.Application.Dtos.MenuCategoryDtos;
using Leia.Application.Dtos.MenuItemDtos;
using Leia.Application.Dtos.ModifierDtos;
using Leia.Application.Dtos.VariantDtos;
using Leia.Application.Exceptions;
using Leia.Application.Interfaces;
using Leia.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Leia.Tests.Controllers
{
    public class MenuCategoryControllerTests
    {
        private readonly Mock<IMenuCategoryService> _service = new();
        private readonly MenuCategoryController _controller;
        private readonly Guid _userId = Guid.NewGuid();

        public MenuCategoryControllerTests()
        {
            _controller = new MenuCategoryController(_service.Object).WithUser(_userId, "RestaurantOwner");
        }

        [Fact]
        public async Task GetByRestaurant_CallsService()
        {
            var id = Guid.NewGuid();

            await _controller.GetByRestaurant(id);

            _service.Verify(s => s.GetByRestaurantAsync(id), Times.Once);
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
            var dto = new CreateMenuCategoryDto();

            var result = await _controller.Create(dto);

            Assert.Equal(201, Assert.IsType<ObjectResult>(result).StatusCode);
            _service.Verify(s => s.CreateAsync(_userId, dto), Times.Once);
        }

        [Fact]
        public async Task Update_UsesUserIdAndId()
        {
            var id = Guid.NewGuid();
            var dto = new UpdateMenuCategoryDto();

            await _controller.Update(id, dto);

            _service.Verify(s => s.UpdateAsync(_userId, id, dto), Times.Once);
        }

        [Fact]
        public async Task Delete_PropagatesBadRequestWhenCategoryHasItems()
        {
            var id = Guid.NewGuid();
            _service.Setup(s => s.DeleteAsync(_userId, id)).ThrowsAsync(new BadRequestException("has items"));

            await Assert.ThrowsAsync<BadRequestException>(() => _controller.Delete(id));
        }

        [Fact]
        public async Task Delete_CallsService()
        {
            var id = Guid.NewGuid();

            var result = await _controller.Delete(id);

            Assert.IsType<OkObjectResult>(result);
            _service.Verify(s => s.DeleteAsync(_userId, id), Times.Once);
        }

        [Fact]
        public async Task Restore_CallsService()
        {
            var id = Guid.NewGuid();

            await _controller.Restore(id);

            _service.Verify(s => s.RestoreAsync(id), Times.Once);
        }

        [Theory]
        [InlineData(nameof(MenuCategoryController.GetMy), "RestaurantOwner")]
        [InlineData(nameof(MenuCategoryController.Create), "RestaurantOwner")]
        [InlineData(nameof(MenuCategoryController.Update), "RestaurantOwner")]
        [InlineData(nameof(MenuCategoryController.Delete), "RestaurantOwner")]
        [InlineData(nameof(MenuCategoryController.Restore), "Admin")]
        public void Methods_AreRestrictedToTheRightRole(string method, string role)
        {
            Assert.Equal(role, ControllerTestBase.RolesOf<MenuCategoryController>(method));
        }
    }

    public class MenuItemControllerTests
    {
        private readonly Mock<IMenuItemService> _items = new();
        private readonly Mock<IMenuOptionService> _options = new();
        private readonly MenuItemController _controller;
        private readonly Guid _userId = Guid.NewGuid();

        public MenuItemControllerTests()
        {
            _controller = new MenuItemController(_items.Object, _options.Object).WithUser(_userId, "RestaurantOwner");
        }

        [Fact]
        public async Task GetByRestaurant_CallsService()
        {
            var id = Guid.NewGuid();

            await _controller.GetByRestaurant(id);

            _items.Verify(s => s.GetByRestaurantAsync(id), Times.Once);
        }

        [Fact]
        public async Task Search_PassesQuery()
        {
            var dto = new MenuSearchDto { Query = "burger" };

            var result = await _controller.Search(dto);

            Assert.IsType<OkObjectResult>(result);
            _items.Verify(s => s.SearchAsync(dto), Times.Once);
        }

        [Fact]
        public async Task GetById_PropagatesNotFound()
        {
            _items.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ThrowsAsync(new NotFoundException("x"));

            await Assert.ThrowsAsync<NotFoundException>(() => _controller.GetById(Guid.NewGuid()));
        }

        [Fact]
        public async Task Create_ReturnsCreated()
        {
            var dto = new CreateMenuItemDto();

            var result = await _controller.Create(dto);

            Assert.Equal(201, Assert.IsType<ObjectResult>(result).StatusCode);
            _items.Verify(s => s.CreateAsync(_userId, dto), Times.Once);
        }

        [Fact]
        public async Task Update_UsesUserIdAndId()
        {
            var id = Guid.NewGuid();
            var dto = new UpdateMenuItemDto();

            await _controller.Update(id, dto);

            _items.Verify(s => s.UpdateAsync(_userId, id, dto), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task SetAvailability_PassesFlag(bool flag)
        {
            var id = Guid.NewGuid();

            await _controller.SetAvailability(id, flag);

            _items.Verify(s => s.SetAvailabilityAsync(_userId, id, flag), Times.Once);
        }

        [Fact]
        public async Task Delete_CallsService()
        {
            var id = Guid.NewGuid();

            await _controller.Delete(id);

            _items.Verify(s => s.DeleteAsync(_userId, id), Times.Once);
        }

        [Fact]
        public async Task SetDeactivatedByAdmin_CallsService()
        {
            var id = Guid.NewGuid();

            await _controller.SetDeactivatedByAdmin(id, true);

            _items.Verify(s => s.SetDeactivatedByAdminAsync(id, true), Times.Once);
        }

        [Fact]
        public async Task Restore_CallsService()
        {
            var id = Guid.NewGuid();

            await _controller.Restore(id);

            _items.Verify(s => s.RestoreAsync(id), Times.Once);
        }

        [Fact]
        public async Task Variants_AreRoutedToOptionService()
        {
            var id = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var create = new CreateVariantDto();
            var update = new UpdateVariantDto();

            var created = await _controller.AddVariant(id, create);
            await _controller.UpdateVariant(variantId, update);
            await _controller.DeleteVariant(variantId);

            Assert.Equal(201, Assert.IsType<ObjectResult>(created).StatusCode);
            _options.Verify(s => s.AddVariantAsync(_userId, id, create), Times.Once);
            _options.Verify(s => s.UpdateVariantAsync(_userId, variantId, update), Times.Once);
            _options.Verify(s => s.DeleteVariantAsync(_userId, variantId), Times.Once);
        }

        [Fact]
        public async Task ModifierGroups_AreRoutedToOptionService()
        {
            var id = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            var create = new CreateModifierGroupDto();
            var update = new UpdateModifierGroupDto();

            var created = await _controller.AddGroup(id, create);
            await _controller.UpdateGroup(groupId, update);
            await _controller.DeleteGroup(groupId);

            Assert.Equal(201, Assert.IsType<ObjectResult>(created).StatusCode);
            _options.Verify(s => s.AddGroupAsync(_userId, id, create), Times.Once);
            _options.Verify(s => s.UpdateGroupAsync(_userId, groupId, update), Times.Once);
            _options.Verify(s => s.DeleteGroupAsync(_userId, groupId), Times.Once);
        }

        [Fact]
        public async Task ModifierOptions_AreRoutedToOptionService()
        {
            var groupId = Guid.NewGuid();
            var optionId = Guid.NewGuid();
            var create = new CreateModifierOptionDto();
            var update = new UpdateModifierOptionDto();

            var created = await _controller.AddOption(groupId, create);
            await _controller.UpdateOption(optionId, update);
            await _controller.DeleteOption(optionId);

            Assert.Equal(201, Assert.IsType<ObjectResult>(created).StatusCode);
            _options.Verify(s => s.AddOptionAsync(_userId, groupId, create), Times.Once);
            _options.Verify(s => s.UpdateOptionAsync(_userId, optionId, update), Times.Once);
            _options.Verify(s => s.DeleteOptionAsync(_userId, optionId), Times.Once);
        }

        [Fact]
        public async Task SetAllergens_RoutedToOptionService()
        {
            var id = Guid.NewGuid();
            var dto = new SetMenuItemAllergensDto();

            await _controller.SetAllergens(id, dto);

            _options.Verify(s => s.SetAllergensAsync(_userId, id, dto), Times.Once);
        }

        [Theory]
        [InlineData(nameof(MenuItemController.GetMy), "RestaurantOwner")]
        [InlineData(nameof(MenuItemController.Create), "RestaurantOwner")]
        [InlineData(nameof(MenuItemController.Update), "RestaurantOwner")]
        [InlineData(nameof(MenuItemController.SetAvailability), "RestaurantOwner")]
        [InlineData(nameof(MenuItemController.Delete), "RestaurantOwner")]
        [InlineData(nameof(MenuItemController.AddVariant), "RestaurantOwner")]
        [InlineData(nameof(MenuItemController.UpdateVariant), "RestaurantOwner")]
        [InlineData(nameof(MenuItemController.DeleteVariant), "RestaurantOwner")]
        [InlineData(nameof(MenuItemController.AddGroup), "RestaurantOwner")]
        [InlineData(nameof(MenuItemController.UpdateGroup), "RestaurantOwner")]
        [InlineData(nameof(MenuItemController.DeleteGroup), "RestaurantOwner")]
        [InlineData(nameof(MenuItemController.AddOption), "RestaurantOwner")]
        [InlineData(nameof(MenuItemController.UpdateOption), "RestaurantOwner")]
        [InlineData(nameof(MenuItemController.DeleteOption), "RestaurantOwner")]
        [InlineData(nameof(MenuItemController.SetAllergens), "RestaurantOwner")]
        [InlineData(nameof(MenuItemController.SetDeactivatedByAdmin), "Admin")]
        [InlineData(nameof(MenuItemController.Restore), "Admin")]
        public void Methods_AreRestrictedToTheRightRole(string method, string role)
        {
            Assert.Equal(role, ControllerTestBase.RolesOf<MenuItemController>(method));
        }

        [Theory]
        [InlineData(nameof(MenuItemController.GetByRestaurant))]
        [InlineData(nameof(MenuItemController.GetById))]
        [InlineData(nameof(MenuItemController.Search))]
        public void Public_methods_are_anonymous(string method)
        {
            Assert.True(ControllerTestBase.IsAnonymous<MenuItemController>(method));
        }
    }
}