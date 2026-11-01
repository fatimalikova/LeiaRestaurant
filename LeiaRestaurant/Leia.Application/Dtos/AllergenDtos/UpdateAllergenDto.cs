using FluentValidation;

namespace Leia.Application.Dtos.AllergenDtos
{
    public class UpdateAllergenDto
    {
        public string Name { get; set; } = string.Empty;
        public string? IconUrl { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateAllergenValidator : AbstractValidator<UpdateAllergenDto>
    {
        public UpdateAllergenValidator()
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