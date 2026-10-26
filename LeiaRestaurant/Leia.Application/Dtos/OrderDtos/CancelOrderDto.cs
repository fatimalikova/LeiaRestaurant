using FluentValidation;

namespace Leia.Application.Dtos.OrderDtos
{
    public class CancelOrderDto
    {
        public string? Reason { get; set; }
    }

    public class CancelOrderValidator : AbstractValidator<CancelOrderDto>
    {
        public CancelOrderValidator()
        {
            RuleFor(x => x.Reason)
                .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters.")
                .When(x => x.Reason != null);
        }
    }
}