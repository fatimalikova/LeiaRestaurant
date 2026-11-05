using Leia.Application.Dtos.PaymentDtos;

namespace Leia.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<ResponsePaymentDto> PayAsync(Guid customerId, PayOrderDto dto);
        Task<ResponsePaymentDto> GetByOrderAsync(Guid userId, Guid orderId);
        Task<ResponsePaymentDto> RefundAsync(Guid orderId, string? reason);
        Task RefundIfPaidAsync(Guid orderId, string reason);
    }
}