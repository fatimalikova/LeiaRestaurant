using Leia.Domain.Common;

namespace Leia.Domain.Entities;

public class CouponUsage : BaseEntity
{
    public Guid CouponId { get; set; }
    public Coupon Coupon { get; set; } = null!;

    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public decimal DiscountAmount { get; set; }
}