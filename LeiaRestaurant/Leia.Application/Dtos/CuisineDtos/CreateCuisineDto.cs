using FluentValidation;

namespace Leia.Application.Dtos.CuisineDtos
{
    public class CreateCuisineDto
    {
        public string Name { get; set; } = string.Empty;
        public string? IconUrl { get; set; }
        public int SortOrder { get; set; }
    }

    public class CreateCuisineValidator : AbstractValidator<CreateCuisineDto>
    {
        public CreateCuisineValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Cuisine name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

            RuleFor(x => x.IconUrl)
                .MaximumLength(500).WithMessage("Icon URL cannot exceed 500 characters.")
                .When(x => x.IconUrl != null);

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Sort order cannot be negative.");
        }
    }
}