using FluentValidation;
using Leia.Application.Helpers;

namespace Leia.Application.Dtos.ReservationDtos
{
    public class RescheduleReservationDto
    {
        public DateTime NewStartDateTime { get; set; }
        public int? GuestCount { get; set; }
    }

    public class RescheduleReservationValidator : AbstractValidator<RescheduleReservationDto>
    {
        public RescheduleReservationValidator()
        {
            RuleFor(x => x.NewStartDateTime)
                .Must(d => d > TimeHelper.NowLocal()).WithMessage("Reservation must be for a future time.");

            RuleFor(x => x.GuestCount)
                .InclusiveBetween(1, 30).WithMessage("Guest count must be between 1 and 30.")
                .When(x => x.GuestCount.HasValue);
        }
    }
}