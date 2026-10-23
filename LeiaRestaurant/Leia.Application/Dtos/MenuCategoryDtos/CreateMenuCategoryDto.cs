using FluentValidation;

namespace Leia.Application.Dtos.MenuCategoryDtos
{
    public class CreateMenuCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }

    public class CreateMenuCategoryValidator : AbstractValidator<CreateMenuCategoryDto>
    {
        public CreateMenuCategoryValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Sort order cannot be negative.");
        }
    }
}