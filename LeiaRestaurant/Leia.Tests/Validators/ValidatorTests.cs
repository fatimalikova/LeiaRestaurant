using Leia.Application.Dtos.CouponDtos;
using Leia.Application.Dtos.DashboardDtos;
using Leia.Application.Dtos.MenuItemDtos;
using Leia.Application.Dtos.OrderDtos;
using Leia.Application.Dtos.PaymentDtos;
using Leia.Application.Dtos.UserDtos;
using Leia.Domain.Enums;
using Xunit;

namespace Leia.Tests.Validators
{
    public class ValidatorTests
    {
        private static CreateOrderDto ValidOrder() => new()
        {
            RestaurantId = Guid.NewGuid(),
            Type = OrderType.Pickup,
            PaymentMethod = PaymentMethod.Cash,
            CustomerPhone = "+994501234567",
            Items = new List<CreateOrderItemDto>
            {
                new() { MenuItemId = Guid.NewGuid(), Quantity = 1 }
            }
        };

        [Fact]
        public void Order_valid_passes()
        {
            Assert.True(new CreateOrderValidator().Validate(ValidOrder()).IsValid);
        }

        [Fact]
        public void Order_without_items_fails()
        {
            var dto = ValidOrder();
            dto.Items.Clear();

            Assert.False(new CreateOrderValidator().Validate(dto).IsValid);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(51)]
        [InlineData(-1)]
        public void Order_item_quantity_out_of_range_fails(int quantity)
        {
            var dto = ValidOrder();
            dto.Items[0].Quantity = quantity;

            Assert.False(new CreateOrderValidator().Validate(dto).IsValid);
        }

        [Fact]
        public void Delivery_order_requires_an_address_or_saved_address()
        {
            var dto = ValidOrder();
            dto.Type = OrderType.Delivery;

            Assert.False(new CreateOrderValidator().Validate(dto).IsValid);

            dto.AddressId = Guid.NewGuid();
            Assert.True(new CreateOrderValidator().Validate(dto).IsValid);
        }

        [Fact]
        public void Order_scheduled_in_the_past_fails()
        {
            var dto = ValidOrder();
            dto.ScheduledFor = DateTime.UtcNow.AddHours(-1);

            Assert.False(new CreateOrderValidator().Validate(dto).IsValid);
        }

        private static CreateCouponDto ValidCoupon() => new()
        {
            Code = "SAVE10",
            Type = CouponType.Percent,
            Value = 10,
            StartsAt = DateTime.UtcNow,
            EndsAt = DateTime.UtcNow.AddDays(1)
        };

        [Fact]
        public void Coupon_valid_passes()
        {
            Assert.True(new CreateCouponValidator().Validate(ValidCoupon()).IsValid);
        }

        [Fact]
        public void Percent_coupon_over_100_fails_but_fixed_over_100_passes()
        {
            var dto = ValidCoupon();
            dto.Value = 150;
            Assert.False(new CreateCouponValidator().Validate(dto).IsValid);

            dto.Type = CouponType.Fixed;
            Assert.True(new CreateCouponValidator().Validate(dto).IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData("bad code")]
        [InlineData("a$b")]
        public void Coupon_code_with_invalid_characters_fails(string code)
        {
            var dto = ValidCoupon();
            dto.Code = code;

            Assert.False(new CreateCouponValidator().Validate(dto).IsValid);
        }

        [Fact]
        public void Coupon_end_before_start_fails()
        {
            var dto = ValidCoupon();
            dto.EndsAt = dto.StartsAt.AddDays(-1);

            Assert.False(new CreateCouponValidator().Validate(dto).IsValid);
        }

        private static PayOrderDto ValidPayment() => new()
        {
            OrderId = Guid.NewGuid(),
            CardNumber = "4242 4242 4242 4242",
            ExpMonth = 12,
            ExpYear = 2030,
            Cvv = "123",
            CardHolder = "Test User"
        };

        [Fact]
        public void Payment_valid_passes()
        {
            Assert.True(new PayOrderValidator().Validate(ValidPayment()).IsValid);
        }

        [Theory]
        [InlineData("12")]
        [InlineData("abcd")]
        [InlineData("")]
        public void Payment_invalid_card_number_fails(string number)
        {
            var dto = ValidPayment();
            dto.CardNumber = number;

            Assert.False(new PayOrderValidator().Validate(dto).IsValid);
        }

        [Theory]
        [InlineData("12")]
        [InlineData("12345")]
        [InlineData("abc")]
        public void Payment_invalid_cvv_fails(string cvv)
        {
            var dto = ValidPayment();
            dto.Cvv = cvv;

            Assert.False(new PayOrderValidator().Validate(dto).IsValid);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(13)]
        public void Payment_invalid_month_fails(int month)
        {
            var dto = ValidPayment();
            dto.ExpMonth = month;

            Assert.False(new PayOrderValidator().Validate(dto).IsValid);
        }

        [Fact]
        public void Register_requires_valid_email_and_strong_enough_password()
        {
            var validator = new RegisterValidator();

            Assert.True(validator.Validate(new RegisterDto { FullName = "A", Email = "a@a.com", Password = "Abcdefg1" }).IsValid);
            Assert.False(validator.Validate(new RegisterDto { FullName = "A", Email = "nope", Password = "Abcdefg1" }).IsValid);
            Assert.False(validator.Validate(new RegisterDto { FullName = "A", Email = "a@a.com", Password = "short" }).IsValid);
            Assert.False(validator.Validate(new RegisterDto { FullName = "", Email = "a@a.com", Password = "Abcdefg1" }).IsValid);
        }

        [Fact]
        public void Login_requires_both_fields()
        {
            var validator = new LoginValidator();

            Assert.True(validator.Validate(new LoginDto { Login = "x", Password = "y" }).IsValid);
            Assert.False(validator.Validate(new LoginDto { Login = "", Password = "y" }).IsValid);
            Assert.False(validator.Validate(new LoginDto { Login = "x", Password = "" }).IsValid);
        }

        [Fact]
        public void Dashboard_period_over_92_days_fails()
        {
            var validator = new DashboardQueryValidator();

            Assert.True(validator.Validate(new DashboardQueryDto
            {
                From = new DateTime(2026, 11, 1),
                To = new DateTime(2026, 11, 30)
            }).IsValid);

            Assert.False(validator.Validate(new DashboardQueryDto
            {
                From = new DateTime(2026, 1, 1),
                To = new DateTime(2026, 6, 1)
            }).IsValid);

            Assert.False(validator.Validate(new DashboardQueryDto
            {
                From = new DateTime(2026, 11, 10),
                To = new DateTime(2026, 11, 1)
            }).IsValid);
        }

        [Theory]
        [InlineData("b", false)]
        [InlineData("", false)]
        [InlineData("burger", true)]
        public void Menu_search_requires_at_least_two_characters(string query, bool valid)
        {
            Assert.Equal(valid, new MenuSearchValidator().Validate(new MenuSearchDto { Query = query }).IsValid);
        }
    }
}