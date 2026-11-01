using FluentValidation;

namespace Leia.Application.Dtos.CouponDtos
{
    public class ValidateCouponDto
    {
        public Guid RestaurantId { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
    }

    public class ValidateCouponValidator : AbstractValidator<ValidateCouponDto>
    {
        public ValidateCouponValidator()
        {
            RuleFor(x => x.RestaurantId)
                .NotEmpty().WithMessage("RestaurantId is required.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Coupon code is required.")
                .MaximumLength(30).WithMessage("Code cannot exceed 30 characters.");

            RuleFor(x => x.Subtotal)
                .GreaterThan(0).WithMessage("Subtotal must be greater than 0.");
        }
    }
}