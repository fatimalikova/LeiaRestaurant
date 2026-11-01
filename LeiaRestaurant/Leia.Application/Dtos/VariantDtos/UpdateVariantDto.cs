using FluentValidation;

namespace Leia.Application.Dtos.VariantDtos
{
    public class UpdateVariantDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal PriceDelta { get; set; }
        public int SortOrder { get; set; }
        public bool IsAvailable { get; set; } = true;
    }

    public class UpdateVariantValidator : AbstractValidator<UpdateVariantDto>
    {
        public UpdateVariantValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Variant name is required.")
                .MaximumLength(50).WithMessage("Name cannot exceed 50 characters.");

            RuleFor(x => x.PriceDelta)
                .InclusiveBetween(0, 10000).WithMessage("Price difference must be between 0 and 10000.");

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Sort order cannot be negative.");
        }
    }
}