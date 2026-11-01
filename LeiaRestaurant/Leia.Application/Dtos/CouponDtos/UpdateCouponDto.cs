using FluentValidation;
using Leia.Domain.Enums;

namespace Leia.Application.Dtos.CouponDtos
{
    public class UpdateCouponDto
    {
        public CouponType Type { get; set; }
        public decimal Value { get; set; }
        public decimal MinOrderAmount { get; set; }
        public decimal? MaxDiscount { get; set; }
        public DateTime StartsAt { get; set; }
        public DateTime EndsAt { get; set; }
        public int? UsageLimit { get; set; }
        public int? PerUserLimit { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateCouponValidator : AbstractValidator<UpdateCouponDto>
    {
        public UpdateCouponValidator()
        {
            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Coupon type is invalid.");

            RuleFor(x => x.Value)
                .GreaterThan(0).WithMessage("Value must be greater than 0.");

            RuleFor(x => x.Value)
                .LessThanOrEqualTo(100).WithMessage("Percent value cannot exceed 100.")
                .When(x => x.Type == CouponType.Percent);

            RuleFor(x => x.MinOrderAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum order amount cannot be negative.");

            RuleFor(x => x.MaxDiscount)
                .GreaterThan(0).WithMessage("Maximum discount must be greater than 0.")
                .When(x => x.MaxDiscount.HasValue);

            RuleFor(x => x.EndsAt)
                .GreaterThan(x => x.StartsAt).WithMessage("End date must be after start date.");

            RuleFor(x => x.UsageLimit)
                .GreaterThanOrEqualTo(1).WithMessage("Usage limit must be at least 1.")
                .When(x => x.UsageLimit.HasValue);

            RuleFor(x => x.PerUserLimit)
                .GreaterThanOrEqualTo(1).WithMessage("Per-user limit must be at least 1.")
                .When(x => x.PerUserLimit.HasValue);
        }
    }
}