using FluentValidation;

namespace Leia.Application.Dtos.PaymentDtos
{
    public class PayOrderDto
    {
        public Guid OrderId { get; set; }
        public string CardNumber { get; set; } = string.Empty;
        public int ExpMonth { get; set; }
        public int ExpYear { get; set; }
        public string Cvv { get; set; } = string.Empty;
        public string CardHolder { get; set; } = string.Empty;
    }

    public class PayOrderValidator : AbstractValidator<PayOrderDto>
    {
        public PayOrderValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty().WithMessage("OrderId is required.");

            RuleFor(x => x.CardNumber)
                .NotEmpty().WithMessage("Card number is required.")
                .Must(n => n.Replace(" ", "").Replace("-", "").All(char.IsDigit)
                    && n.Replace(" ", "").Replace("-", "").Length is >= 13 and <= 19)
                .WithMessage("Card number is invalid.");

            RuleFor(x => x.ExpMonth)
                .InclusiveBetween(1, 12).WithMessage("Expiry month must be between 1 and 12.");

            RuleFor(x => x.ExpYear)
                .Must(y => y is >= 0 and <= 99 or >= 2000 and <= 2100)
                .WithMessage("Expiry year is invalid.");

            RuleFor(x => x.Cvv)
                .NotEmpty().WithMessage("CVV is required.")
                .Matches(@"^\d{3,4}$").WithMessage("CVV must be 3 or 4 digits.");

            RuleFor(x => x.CardHolder)
                .NotEmpty().WithMessage("Card holder is required.")
                .MaximumLength(100).WithMessage("Card holder cannot exceed 100 characters.");
        }
    }
}