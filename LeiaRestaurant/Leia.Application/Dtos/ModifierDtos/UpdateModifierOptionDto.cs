using FluentValidation;

namespace Leia.Application.Dtos.ModifierDtos
{
    public class UpdateModifierOptionDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal PriceDelta { get; set; }
        public int SortOrder { get; set; }
        public bool IsAvailable { get; set; } = true;
    }

    public class UpdateModifierOptionValidator : AbstractValidator<UpdateModifierOptionDto>
    {
        public UpdateModifierOptionValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Option name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

            RuleFor(x => x.PriceDelta)
                .InclusiveBetween(0, 10000).WithMessage("Price must be between 0 and 10000.");

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Sort order cannot be negative.");
        }
    }
}