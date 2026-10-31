using FluentValidation;

namespace Leia.Application.Dtos.ReservationDtos
{
    public class ReservationSettingsDto
    {
        public bool AcceptsReservations { get; set; }
        public bool AutoConfirmReservations { get; set; }
        public int ReservationDurationMinutes { get; set; } = 90;
    }

    public class ReservationSettingsValidator : AbstractValidator<ReservationSettingsDto>
    {
        public ReservationSettingsValidator()
        {
            RuleFor(x => x.ReservationDurationMinutes)
                .InclusiveBetween(30, 240).WithMessage("Duration must be between 30 and 240 minutes.");
        }
    }
}