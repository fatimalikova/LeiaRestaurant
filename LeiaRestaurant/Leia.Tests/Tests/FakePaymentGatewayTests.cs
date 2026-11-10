using Leia.Application.Models;
using Leia.Infrastructure.Services;
using Xunit;

namespace Leia.Tests
{
    public class FakePaymentGatewayTests
    {
        private static CardDetails Card(string number, int month = 12, int year = 2099) => new()
        {
            CardNumber = number,
            ExpMonth = month,
            ExpYear = year,
            Cvv = "123",
            CardHolder = "Test User"
        };

        private readonly FakePaymentGateway _gateway = new();

        [Fact]
        public async Task Test_card_succeeds_and_returns_brand_and_last4()
        {
            var result = await _gateway.ChargeAsync(Card("4242 4242 4242 4242"), 10m, "key");

            Assert.True(result.Success);
            Assert.Equal("Visa", result.Brand);
            Assert.Equal("4242", result.Last4);
            Assert.NotNull(result.ProviderPaymentId);
        }

        [Theory]
        [InlineData("4000000000000002", "Card was declined.")]
        [InlineData("4000000000009995", "Insufficient funds.")]
        [InlineData("4000000000000069", "Card has expired.")]
        public async Task Special_cards_fail_with_a_reason(string number, string reason)
        {
            var result = await _gateway.ChargeAsync(Card(number), 10m, "key");

            Assert.False(result.Success);
            Assert.Equal(reason, result.FailureReason);
        }

        [Fact]
        public async Task Card_failing_luhn_is_rejected()
        {
            var result = await _gateway.ChargeAsync(Card("4242424242424241"), 10m, "key");

            Assert.False(result.Success);
        }

        [Fact]
        public async Task Expired_card_is_rejected()
        {
            var result = await _gateway.ChargeAsync(Card("4242424242424242", month: 1, year: 2020), 10m, "key");

            Assert.False(result.Success);
            Assert.Equal("Card has expired.", result.FailureReason);
        }

        [Fact]
        public async Task Refund_succeeds()
        {
            var result = await _gateway.RefundAsync("fake_pay_1", 10m);

            Assert.True(result.Success);
        }
    }
}