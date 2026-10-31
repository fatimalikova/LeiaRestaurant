using FluentValidation;
using Leia.Domain.Enums;

namespace Leia.Application.Dtos.ReservationDtos
{
    public class ReservationFilterDto
    {
        public ReservationStatus? Status { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class ReservationFilterValidator : AbstractValidator<ReservationFilterDto>
    {
        public ReservationFilterValidator()
        {
            RuleFor(x => x.To)
                .GreaterThanOrEqualTo(x => x.From).WithMessage("'To' date must be after 'From' date.")
                .When(x => x.From.HasValue && x.To.HasValue);

            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
        }
    }
}