namespace Leia.Application.Dtos.CouponDtos
{
    public class AppliedCouponDto
    {
        public Guid CouponId { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
    }
}