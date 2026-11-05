using Leia.Domain.Common;
using Leia.Domain.Enums;

namespace Leia.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string Provider { get; set; } = string.Empty;
    public string? ProviderPaymentId { get; set; }
    public string? Brand { get; set; }
    public string? Last4 { get; set; }
    public string? FailureReason { get; set; }
    public int AttemptCount { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? RefundedAt { get; set; }
}