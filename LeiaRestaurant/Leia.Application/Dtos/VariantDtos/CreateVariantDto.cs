using FluentValidation;

namespace Leia.Application.Dtos.VariantDtos
{
    public class CreateVariantDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal PriceDelta { get; set; }
        public int SortOrder { get; set; }
    }

    public class CreateVariantValidator : AbstractValidator<CreateVariantDto>
    {
        public CreateVariantValidator()
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