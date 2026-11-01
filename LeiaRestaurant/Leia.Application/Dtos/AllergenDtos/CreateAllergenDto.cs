using FluentValidation;

namespace Leia.Application.Dtos.AllergenDtos
{
    public class CreateAllergenDto
    {
        public string Name { get; set; } = string.Empty;
        public string? IconUrl { get; set; }
    }

    public class CreateAllergenValidator : AbstractValidator<CreateAllergenDto>
    {
        public CreateAllergenValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Allergen name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

            RuleFor(x => x.IconUrl)
                .MaximumLength(500).WithMessage("Icon URL cannot exceed 500 characters.")
                .When(x => x.IconUrl != null);
        }
    }
}