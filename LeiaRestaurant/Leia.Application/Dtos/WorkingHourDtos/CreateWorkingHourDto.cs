using FluentValidation;

namespace Leia.Application.Dtos.WorkingHourDtos
{
    public class CreateWorkingHourDto
    {
        public DayOfWeek Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

    public class CreateWorkingHourValidator : AbstractValidator<CreateWorkingHourDto>
    {
        public CreateWorkingHourValidator()
        {
            RuleFor(x => x.Day)
                .IsInEnum().WithMessage("Day is invalid.");

            RuleFor(x => x.StartTime)
                .NotNull().WithMessage("Start time is required.");

            RuleFor(x => x.EndTime)
                .NotNull().WithMessage("End time is required.")
                .GreaterThan(x => x.StartTime).WithMessage("End time must be after start time.");
        }
    }
}   