using FluentValidation;

namespace Leia.Application.Dtos.RestaurantImageDtos
{
    public class AddRestaurantImageDto
    {
        public string ImageUrl { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }

    public class AddRestaurantImageValidator : AbstractValidator<AddRestaurantImageDto>
    {
        public AddRestaurantImageValidator()
        {
            RuleFor(x => x.ImageUrl)
                .NotEmpty().WithMessage("Image URL is required.")
                .MaximumLength(500).WithMessage("Image URL cannot exceed 500 characters.");

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Sort order cannot be negative.");
        }
    }
}