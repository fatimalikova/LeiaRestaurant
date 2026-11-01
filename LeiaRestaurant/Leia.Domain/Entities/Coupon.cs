using Leia.Domain.Common;
using Leia.Domain.Enums;

namespace Leia.Domain.Entities;

public class Coupon : BaseEntity
{
    public Guid? RestaurantId { get; set; }
    public Restaurant? Restaurant { get; set; }

    public string Code { get; set; } = string.Empty;
    public CouponType Type { get; set; }
    public decimal Value { get; set; }
    public decimal MinOrderAmount { get; set; }
    public decimal? MaxDiscount { get; set; }

    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }

    public int? UsageLimit { get; set; }
    public int? PerUserLimit { get; set; }
    public int UsedCount { get; set; }
    public bool IsActive { get; set; } = true;
}