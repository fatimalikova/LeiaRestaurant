using FluentValidation;

namespace Leia.Application.Dtos.DashboardDtos
{
    public class DashboardQueryDto
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }

    public class DashboardQueryValidator : AbstractValidator<DashboardQueryDto>
    {
        public DashboardQueryValidator()
        {
            RuleFor(x => x.To)
                .GreaterThanOrEqualTo(x => x.From).WithMessage("'To' date must be after 'From' date.")
                .When(x => x.From.HasValue && x.To.HasValue);

            RuleFor(x => x)
                .Must(x => (x.To!.Value.Date - x.From!.Value.Date).TotalDays <= 92)
                .WithMessage("The period cannot exceed 92 days.")
                .When(x => x.From.HasValue && x.To.HasValue);
        }
    }
}