using Leia.Domain.Enums;

namespace Leia.Application.Dtos.PaymentDtos
{
    public class ResponsePaymentDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public string Provider { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public string? Last4 { get; set; }
        public string? FailureReason { get; set; }
        public int AttemptCount { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? RefundedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}