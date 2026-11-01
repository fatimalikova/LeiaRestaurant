using FluentValidation;

namespace Leia.Application.Dtos.ModifierDtos
{
    public class UpdateModifierGroupDto
    {
        public string Name { get; set; } = string.Empty;
        public int MinSelect { get; set; }
        public int MaxSelect { get; set; } = 1;
        public int SortOrder { get; set; }
    }

    public class UpdateModifierGroupValidator : AbstractValidator<UpdateModifierGroupDto>
    {
        public UpdateModifierGroupValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Group name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

            RuleFor(x => x.MinSelect)
                .InclusiveBetween(0, 20).WithMessage("Minimum selection must be between 0 and 20.");

            RuleFor(x => x.MaxSelect)
                .InclusiveBetween(1, 20).WithMessage("Maximum selection must be between 1 and 20.")
                .GreaterThanOrEqualTo(x => x.MinSelect).WithMessage("Maximum selection cannot be less than minimum.");

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Sort order cannot be negative.");
        }
    }
}