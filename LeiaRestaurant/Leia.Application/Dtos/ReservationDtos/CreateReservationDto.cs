using FluentValidation;
using Leia.Application.Helpers;

namespace Leia.Application.Dtos.ReservationDtos
{
    public class CreateReservationDto
    {
        public Guid RestaurantId { get; set; }
        public DateTime StartDateTime { get; set; }
        public int GuestCount { get; set; }
        public string ContactPhone { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class CreateReservationValidator : AbstractValidator<CreateReservationDto>
    {
        public CreateReservationValidator()
        {
            RuleFor(x => x.RestaurantId)
                .NotEmpty().WithMessage("RestaurantId is required.");

            RuleFor(x => x.StartDateTime)
                .Must(d => d > TimeHelper.NowLocal()).WithMessage("Reservation must be for a future time.");

            RuleFor(x => x.GuestCount)
                .InclusiveBetween(1, 30).WithMessage("Guest count must be between 1 and 30.");

            RuleFor(x => x.ContactPhone)
                .NotEmpty().WithMessage("Phone number is required.")
                .MaximumLength(30).WithMessage("Phone number cannot exceed 30 characters.");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.")
                .When(x => x.Notes != null);
        }
    }
}