using FluentValidation;

namespace Leia.Application.Dtos.MenuItemDtos
{
    public class MenuSearchDto
    {
        public string Query { get; set; } = string.Empty;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class MenuSearchValidator : AbstractValidator<MenuSearchDto>
    {
        public MenuSearchValidator()
        {
            RuleFor(x => x.Query)
                .NotEmpty().WithMessage("Search text is required.")
                .MinimumLength(2).WithMessage("Search text must be at least 2 characters.")
                .MaximumLength(50).WithMessage("Search text cannot exceed 50 characters.");

            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
        }
    }
}