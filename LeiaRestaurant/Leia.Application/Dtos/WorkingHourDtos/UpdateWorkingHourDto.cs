using FluentValidation;

namespace Leia.Application.Dtos.WorkingHourDtos
{
    public class UpdateWorkingHourDto
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

    public class UpdateWorkingHourValidator : AbstractValidator<UpdateWorkingHourDto>
    {
        public UpdateWorkingHourValidator()
        {
            RuleFor(x => x.StartTime)
                .NotNull().WithMessage("Start time is required.");

            RuleFor(x => x.EndTime)
                .NotNull().WithMessage("End time is required.")
                .GreaterThan(x => x.StartTime).WithMessage("End time must be after start time.");
        }
    }
}