using FluentValidation;

namespace Leia.Application.Dtos.PaymentDtos
{
    public class RefundPaymentDto
    {
        public string? Reason { get; set; }
    }

    public class RefundPaymentValidator : AbstractValidator<RefundPaymentDto>
    {
        public RefundPaymentValidator()
        {
            RuleFor(x => x.Reason)
                .MaximumLength(300).WithMessage("Reason cannot exceed 300 characters.")
                .When(x => x.Reason != null);
        }
    }
}