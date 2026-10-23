using FluentValidation;

namespace Leia.Application.Dtos.MenuCategoryDtos
{
    public class UpdateMenuCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateMenuCategoryValidator : AbstractValidator<UpdateMenuCategoryDto>
    {
        public UpdateMenuCategoryValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Sort order cannot be negative.");
        }
    }
}