using Leia.Application.Interfaces;
using Leia.Application.Models;

namespace Leia.Infrastructure.Services
{
    public class FakePaymentGateway : IPaymentGateway
    {
        public Task<GatewayChargeResult> ChargeAsync(CardDetails card, decimal amount, string idempotencyKey)
        {
            var number = new string(card.CardNumber.Where(char.IsDigit).ToArray());
            var result = new GatewayChargeResult
            {
                Brand = DetectBrand(number),
                Last4 = number.Length >= 4 ? number[^4..] : number
            };

            var failure = number switch
            {
                "4000000000000002" => "Card was declined.",
                "4000000000009995" => "Insufficient funds.",
                "4000000000000069" => "Card has expired.",
                _ => null
            };

            if (failure == null && !PassesLuhn(number))
                failure = "Card number is invalid.";

            if (failure == null && IsExpired(card.ExpMonth, card.ExpYear))
                failure = "Card has expired.";

            if (failure != null)
            {
                result.Success = false;
                result.FailureReason = failure;
                return Task.FromResult(result);
            }

            result.Success = true;
            result.ProviderPaymentId = $"fake_pay_{Guid.NewGuid():N}"[..24];
            return Task.FromResult(result);
        }

        public Task<GatewayRefundResult> RefundAsync(string providerPaymentId, decimal amount)
        {
            return Task.FromResult(new GatewayRefundResult
            {
                Success = true,
                ProviderRefundId = $"fake_ref_{Guid.NewGuid():N}"[..24]
            });
        }

        private static bool IsExpired(int month, int year)
        {
            var fullYear = year < 100 ? 2000 + year : year;
            var endOfMonth = new DateTime(fullYear, month, 1).AddMonths(1);
            return endOfMonth <= DateTime.UtcNow;
        }

        private static bool PassesLuhn(string number)
        {
            var sum = 0;
            var alternate = false;

            for (var i = number.Length - 1; i >= 0; i--)
            {
                var digit = number[i] - '0';
                if (alternate)
                {
                    digit *= 2;
                    if (digit > 9) digit -= 9;
                }
                sum += digit;
                alternate = !alternate;
            }

            return sum % 10 == 0;
        }

        private static string DetectBrand(string number)
        {
            if (number.StartsWith('4')) return "Visa";
            if (number.Length >= 2 && int.TryParse(number[..2], out var two))
            {
                if (two is >= 51 and <= 55) return "Mastercard";
                if (two is 34 or 37) return "Amex";
            }
            if (number.Length >= 4 && int.TryParse(number[..4], out var four) && four is >= 2221 and <= 2720)
                return "Mastercard";
            return "Card";
        }
    }
}