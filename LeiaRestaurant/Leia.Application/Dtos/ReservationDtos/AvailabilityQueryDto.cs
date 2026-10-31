using FluentValidation;

namespace Leia.Application.Dtos.ReservationDtos
{
    public class AvailabilityQueryDto
    {
        public Guid RestaurantId { get; set; }
        public DateTime Date { get; set; }
        public int Guests { get; set; } = 2;
    }

    public class AvailabilityQueryValidator : AbstractValidator<AvailabilityQueryDto>
    {
        public AvailabilityQueryValidator()
        {
            RuleFor(x => x.RestaurantId)
                .NotEmpty().WithMessage("RestaurantId is required.");

            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("Date is required.");

            RuleFor(x => x.Guests)
                .InclusiveBetween(1, 30).WithMessage("Guests must be between 1 and 30.");
        }
    }
}