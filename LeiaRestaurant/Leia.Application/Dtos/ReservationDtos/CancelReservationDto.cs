using FluentValidation;

namespace Leia.Application.Dtos.ReservationDtos
{
    public class CancelReservationDto
    {
        public string? Reason { get; set; }
    }

    public class CancelReservationValidator : AbstractValidator<CancelReservationDto>
    {
        public CancelReservationValidator()
        {
            RuleFor(x => x.Reason)
                .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters.")
                .When(x => x.Reason != null);
        }
    }
}