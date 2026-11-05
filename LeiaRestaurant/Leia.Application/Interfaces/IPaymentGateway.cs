using Leia.Application.Models;

namespace Leia.Application.Interfaces
{
    public interface IPaymentGateway
    {
        Task<GatewayChargeResult> ChargeAsync(CardDetails card, decimal amount, string idempotencyKey);
        Task<GatewayRefundResult> RefundAsync(string providerPaymentId, decimal amount);
    }
}