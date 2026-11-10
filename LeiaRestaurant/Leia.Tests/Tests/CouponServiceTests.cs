using Leia.Application.Exceptions;
using Leia.Domain.Enums;
using Leia.Infrastructure.Services;
using Leia.Tests.Helpers;
using Xunit;

namespace Leia.Tests
{
    public class CouponServiceTests
    {
        private static (TestDb T, CouponService Service, Guid RestaurantId, Guid CustomerId) Build()
        {
            var t = new TestDb();
            var owner = Seeder.AddUser(t.Db, "owner@test.com");
            var customer = Seeder.AddUser(t.Db, "customer@test.com");
            var restaurant = Seeder.AddRestaurant(t.Db, owner.Id, Seeder.DayStart, Seeder.DayEnd);
            return (t, new CouponService(t.Db, t.Mapper), restaurant.Id, customer.Id);
        }

        [Fact]
        public async Task Percent_coupon_is_capped_by_max_discount()
        {
            var (t, service, restaurantId, customerId) = Build();
            using var _ = t;
            Seeder.AddCoupon(t.Db, restaurantId, value: 10, maxDiscount: 5);

            var result = await service.EvaluateAsync(customerId, restaurantId, "welcome10", 100m);

            Assert.Equal(5m, result.DiscountAmount);
        }

        [Fact]
        public async Task Percent_coupon_calculates_percentage()
        {
            var (t, service, restaurantId, customerId) = Build();
            using var _ = t;
            Seeder.AddCoupon(t.Db, restaurantId, value: 10, maxDiscount: 5);

            var result = await service.EvaluateAsync(customerId, restaurantId, "WELCOME10", 33m);

            Assert.Equal(3.3m, result.DiscountAmount);
        }

        [Fact]
        public async Task Fixed_coupon_never_exceeds_subtotal()
        {
            var (t, service, restaurantId, customerId) = Build();
            using var _ = t;
            Seeder.AddCoupon(t.Db, restaurantId, type: CouponType.Fixed, value: 50);

            var result = await service.EvaluateAsync(customerId, restaurantId, "WELCOME10", 20m);

            Assert.Equal(20m, result.DiscountAmount);
        }

        [Fact]
        public async Task Expired_coupon_is_rejected()
        {
            var (t, service, restaurantId, customerId) = Build();
            using var _ = t;
            Seeder.AddCoupon(t.Db, restaurantId, startsAt: DateTime.UtcNow.AddDays(-10), endsAt: DateTime.UtcNow.AddDays(-1));

            await Assert.ThrowsAsync<BadRequestException>(() =>
                service.EvaluateAsync(customerId, restaurantId, "WELCOME10", 50m));
        }

        [Fact]
        public async Task Coupon_of_another_restaurant_is_rejected()
        {
            var (t, service, restaurantId, customerId) = Build();
            using var _ = t;
            Seeder.AddCoupon(t.Db, Guid.NewGuid() == restaurantId ? null : restaurantId);

            await Assert.ThrowsAsync<BadRequestException>(() =>
                service.EvaluateAsync(customerId, Guid.NewGuid(), "WELCOME10", 50m));
        }

        [Fact]
        public async Task Coupon_below_minimum_order_is_rejected()
        {
            var (t, service, restaurantId, customerId) = Build();
            using var _ = t;
            Seeder.AddCoupon(t.Db, restaurantId, minOrder: 30);

            await Assert.ThrowsAsync<BadRequestException>(() =>
                service.EvaluateAsync(customerId, restaurantId, "WELCOME10", 20m));
        }

        [Fact]
        public async Task Unknown_code_is_rejected()
        {
            var (t, service, restaurantId, customerId) = Build();
            using var _ = t;

            await Assert.ThrowsAsync<BadRequestException>(() =>
                service.EvaluateAsync(customerId, restaurantId, "NOPE", 20m));
        }
    }
}