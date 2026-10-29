using FluentValidation;

namespace Leia.Application.Dtos.RestaurantDtos
{
    public class RestaurantFilterDto
    {
        public string? Search { get; set; }
        public Guid? CuisineId { get; set; }
        public double? MinRating { get; set; }
        public bool? AcceptsDelivery { get; set; }
        public bool? AcceptsPickup { get; set; }
        public bool? OpenNow { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class RestaurantFilterValidator : AbstractValidator<RestaurantFilterDto>
    {
        public RestaurantFilterValidator()
        {
            RuleFor(x => x.MinRating)
                .InclusiveBetween(0, 5).WithMessage("Minimum rating must be between 0 and 5.")
                .When(x => x.MinRating.HasValue);

            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
        }
    }
}