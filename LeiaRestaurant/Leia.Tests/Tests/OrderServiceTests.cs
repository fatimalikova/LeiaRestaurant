using Leia.Application.Dtos.OrderDtos;
using Leia.Application.Exceptions;
using Leia.Domain.Entities;
using Leia.Domain.Enums;
using Leia.Infrastructure.Services;
using Leia.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Leia.Tests
{
    public class OrderServiceTests
    {
        private sealed class Ctx : IDisposable
        {
            public TestDb T { get; init; } = null!;
            public OrderService Service { get; init; } = null!;
            public FakeNotificationService Notes { get; init; } = null!;
            public ApplicationUser Owner { get; init; } = null!;
            public ApplicationUser Customer { get; init; } = null!;
            public Restaurant Restaurant { get; init; } = null!;
            public MenuItem Item { get; init; } = null!;

            public void Dispose() => T.Dispose();
        }

        private static Ctx Build(TimeSpan? open = null, TimeSpan? close = null, decimal price = 10m)
        {
            var t = new TestDb();
            var owner = Seeder.AddUser(t.Db, "owner@test.com");
            var customer = Seeder.AddUser(t.Db, "customer@test.com");
            var restaurant = Seeder.AddRestaurant(t.Db, owner.Id, open ?? Seeder.DayStart, close ?? Seeder.DayEnd);
            var item = Seeder.AddItem(t.Db, restaurant, price);

            var notes = new FakeNotificationService();
            var service = new OrderService(
                t.Db, notes, new CouponService(t.Db, t.Mapper), new FakePaymentService(), null!, t.Mapper);

            return new Ctx { T = t, Service = service, Notes = notes, Owner = owner, Customer = customer, Restaurant = restaurant, Item = item };
        }

        private static CreateOrderDto NewOrder(Ctx c, int quantity = 1, OrderType type = OrderType.Pickup,
            PaymentMethod payment = PaymentMethod.Cash, string? coupon = null) => new()
            {
                RestaurantId = c.Restaurant.Id,
                Type = type,
                PaymentMethod = payment,
                DeliveryAddress = type == OrderType.Delivery ? "Baku, Nizami 1" : null,
                CustomerPhone = "+994501234567",
                CouponCode = coupon,
                Items = new List<CreateOrderItemDto> { new() { MenuItemId = c.Item.Id, Quantity = quantity } }
            };

        private static async Task MoveTo(Ctx c, Guid orderId, params OrderStatus[] statuses)
        {
            foreach (var status in statuses)
                await c.Service.UpdateStatusAsync(c.Owner.Id, orderId, new UpdateOrderStatusDto { Status = status });
        }

        [Fact]
        public async Task Price_includes_variant_and_modifiers()
        {
            using var c = Build(price: 10m);

            var variant = new MenuItemVariant { MenuItemId = c.Item.Id, Name = "Big", PriceDelta = 5m };
            var group = new ModifierGroup { MenuItemId = c.Item.Id, Name = "Cheese", MinSelect = 0, MaxSelect = 2 };
            var option = new ModifierOption { Group = group, Name = "Mozzarella", PriceDelta = 1.5m };
            c.T.Db.AddRange(variant, group, option);
            await c.T.Db.SaveChangesAsync();

            var dto = NewOrder(c, quantity: 2);
            dto.Items[0].VariantId = variant.Id;
            dto.Items[0].ModifierOptionIds = new List<Guid> { option.Id };

            var result = await c.Service.CreateAsync(c.Customer.Id, dto);

            Assert.Equal(33m, result.Subtotal);
            Assert.Equal(33m, result.Total);
            Assert.Equal("Big", result.Items[0].VariantName);
            Assert.Single(result.Items[0].Modifiers);
        }

        [Fact]
        public async Task Variant_is_required_when_item_has_variants()
        {
            using var c = Build();
            c.T.Db.MenuItemVariants.Add(new MenuItemVariant { MenuItemId = c.Item.Id, Name = "Small" });
            await c.T.Db.SaveChangesAsync();

            await Assert.ThrowsAsync<BadRequestException>(() =>
                c.Service.CreateAsync(c.Customer.Id, NewOrder(c)));
        }

        [Fact]
        public async Task Coupon_discount_is_applied_and_usage_is_counted()
        {
            using var c = Build(price: 33m);
            var coupon = Seeder.AddCoupon(c.T.Db, c.Restaurant.Id, value: 10, maxDiscount: 5, perUserLimit: 1);

            var result = await c.Service.CreateAsync(c.Customer.Id, NewOrder(c, coupon: "welcome10"));

            Assert.Equal(3.3m, result.Discount);
            Assert.Equal(29.7m, result.Total);
            Assert.Equal("WELCOME10", result.CouponCode);

            var stored = await c.T.Db.Coupons.AsNoTracking().FirstAsync(x => x.Id == coupon.Id);
            Assert.Equal(1, stored.UsedCount);
        }

        [Fact]
        public async Task Coupon_cannot_be_used_twice_by_the_same_customer_when_limited()
        {
            using var c = Build(price: 33m);
            Seeder.AddCoupon(c.T.Db, c.Restaurant.Id, perUserLimit: 1);

            await c.Service.CreateAsync(c.Customer.Id, NewOrder(c, coupon: "WELCOME10"));

            await Assert.ThrowsAsync<BadRequestException>(() =>
                c.Service.CreateAsync(c.Customer.Id, NewOrder(c, coupon: "WELCOME10")));
        }

        [Fact]
        public async Task Rejecting_an_order_releases_the_coupon()
        {
            using var c = Build(price: 33m);
            var coupon = Seeder.AddCoupon(c.T.Db, c.Restaurant.Id, perUserLimit: 1);

            var order = await c.Service.CreateAsync(c.Customer.Id, NewOrder(c, coupon: "WELCOME10"));

            await c.Service.UpdateStatusAsync(c.Owner.Id, order.Id,
                new UpdateOrderStatusDto { Status = OrderStatus.Rejected, Comment = "Sold out" });

            var stored = await c.T.Db.Coupons.AsNoTracking().FirstAsync(x => x.Id == coupon.Id);
            Assert.Equal(0, stored.UsedCount);
            Assert.False(await c.T.Db.CouponUsages.AnyAsync());
        }

        [Fact]
        public async Task Minimum_order_amount_is_enforced()
        {
            using var c = Build(price: 10m);
            c.Restaurant.MinOrderAmount = 100m;
            await c.T.Db.SaveChangesAsync();

            await Assert.ThrowsAsync<BadRequestException>(() =>
                c.Service.CreateAsync(c.Customer.Id, NewOrder(c)));
        }

        [Fact]
        public async Task Closed_restaurant_does_not_accept_orders()
        {
            using var c = Build(open: TimeSpan.Zero, close: TimeSpan.Zero);

            await Assert.ThrowsAsync<BadRequestException>(() =>
                c.Service.CreateAsync(c.Customer.Id, NewOrder(c)));
        }

        [Fact]
        public async Task Unavailable_item_cannot_be_ordered()
        {
            using var c = Build();
            c.Item.IsAvailable = false;
            await c.T.Db.SaveChangesAsync();

            await Assert.ThrowsAsync<BadRequestException>(() =>
                c.Service.CreateAsync(c.Customer.Id, NewOrder(c)));
        }

        [Fact]
        public async Task Item_from_another_restaurant_is_rejected()
        {
            using var c = Build();
            var otherOwner = Seeder.AddUser(c.T.Db, "other@test.com");
            var other = Seeder.AddRestaurant(c.T.Db, otherOwner.Id, Seeder.DayStart, Seeder.DayEnd);
            var foreignItem = Seeder.AddItem(c.T.Db, other, 5m, "Foreign");

            var dto = NewOrder(c);
            dto.Items[0].MenuItemId = foreignItem.Id;

            await Assert.ThrowsAsync<BadRequestException>(() =>
                c.Service.CreateAsync(c.Customer.Id, dto));
        }

        [Fact]
        public async Task Cash_order_notifies_the_restaurant_immediately()
        {
            using var c = Build();

            await c.Service.CreateAsync(c.Customer.Id, NewOrder(c));

            Assert.Contains(c.Notes.Sent, n => n.UserId == c.Owner.Id && n.Type == NotificationType.NewOrder);
        }

        [Fact]
        public async Task Card_order_is_pending_payment_and_hidden_from_restaurant()
        {
            using var c = Build();

            var order = await c.Service.CreateAsync(c.Customer.Id, NewOrder(c, payment: PaymentMethod.Card));

            Assert.Equal(PaymentStatus.Pending, order.PaymentStatus);
            Assert.Empty(c.Notes.Sent);

            var visible = await c.Service.GetRestaurantOrdersAsync(c.Owner.Id, new OrderFilterDto());
            Assert.Empty(visible.Items);

            await Assert.ThrowsAsync<BadRequestException>(() =>
                MoveTo(c, order.Id, OrderStatus.Accepted));
        }

        [Fact]
        public async Task Pickup_order_follows_the_full_happy_path()
        {
            using var c = Build();
            var order = await c.Service.CreateAsync(c.Customer.Id, NewOrder(c));

            await MoveTo(c, order.Id, OrderStatus.Accepted, OrderStatus.Preparing, OrderStatus.Ready, OrderStatus.Completed);

            var stored = await c.T.Db.Orders.AsNoTracking().FirstAsync(o => o.Id == order.Id);
            Assert.Equal(OrderStatus.Completed, stored.Status);
            Assert.Equal(5, await c.T.Db.OrderStatusHistories.CountAsync(h => h.OrderId == order.Id));
        }

        [Fact]
        public async Task Status_cannot_skip_steps()
        {
            using var c = Build();
            var order = await c.Service.CreateAsync(c.Customer.Id, NewOrder(c));

            await Assert.ThrowsAsync<BadRequestException>(() => MoveTo(c, order.Id, OrderStatus.Completed));
            await Assert.ThrowsAsync<BadRequestException>(() => MoveTo(c, order.Id, OrderStatus.Preparing));
        }

        [Fact]
        public async Task Delivery_order_cannot_complete_straight_from_ready()
        {
            using var c = Build();
            var order = await c.Service.CreateAsync(c.Customer.Id, NewOrder(c, type: OrderType.Delivery));

            await MoveTo(c, order.Id, OrderStatus.Accepted, OrderStatus.Preparing, OrderStatus.Ready);

            await Assert.ThrowsAsync<BadRequestException>(() => MoveTo(c, order.Id, OrderStatus.Completed));

            await MoveTo(c, order.Id, OrderStatus.OutForDelivery, OrderStatus.Completed);
        }

        [Fact]
        public async Task Customer_can_cancel_a_pending_order_but_not_someone_elses()
        {
            using var c = Build();
            var order = await c.Service.CreateAsync(c.Customer.Id, NewOrder(c));

            await Assert.ThrowsAsync<ForbiddenException>(() =>
                c.Service.CancelAsync(Guid.NewGuid(), order.Id, new CancelOrderDto()));

            var cancelled = await c.Service.CancelAsync(c.Customer.Id, order.Id, new CancelOrderDto { Reason = "Changed my mind" });

            Assert.Equal(OrderStatus.Cancelled, cancelled.Status);
        }

        [Fact]
        public async Task Owner_cannot_update_an_order_of_another_restaurant()
        {
            using var c = Build();
            var order = await c.Service.CreateAsync(c.Customer.Id, NewOrder(c));

            var stranger = Seeder.AddUser(c.T.Db, "stranger@test.com");
            Seeder.AddRestaurant(c.T.Db, stranger.Id, Seeder.DayStart, Seeder.DayEnd);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                c.Service.UpdateStatusAsync(stranger.Id, order.Id, new UpdateOrderStatusDto { Status = OrderStatus.Accepted }));
        }
    }
}