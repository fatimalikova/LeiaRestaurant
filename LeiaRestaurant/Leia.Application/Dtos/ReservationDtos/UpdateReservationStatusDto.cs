using FluentValidation;
using Leia.Domain.Enums;

namespace Leia.Application.Dtos.ReservationDtos
{
    public class UpdateReservationStatusDto
    {
        public ReservationStatus Status { get; set; }
        public string? Comment { get; set; }
    }

    public class UpdateReservationStatusValidator : AbstractValidator<UpdateReservationStatusDto>
    {
        public UpdateReservationStatusValidator()
        {
            RuleFor(x => x.Status)
                .Must(s => s is ReservationStatus.Confirmed or ReservationStatus.Rejected
                    or ReservationStatus.Completed or ReservationStatus.NoShow)
                .WithMessage("Status must be Confirmed, Rejected, Completed or NoShow.");

            RuleFor(x => x.Comment)
                .NotEmpty().WithMessage("Reason is required when rejecting a reservation.")
                .When(x => x.Status == ReservationStatus.Rejected);

            RuleFor(x => x.Comment)
                .MaximumLength(500).WithMessage("Comment cannot exceed 500 characters.")
                .When(x => x.Comment != null);
        }
    }
}