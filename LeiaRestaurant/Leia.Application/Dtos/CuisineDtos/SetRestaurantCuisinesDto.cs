using FluentValidation;

namespace Leia.Application.Dtos.CuisineDtos
{
    public class SetRestaurantCuisinesDto
    {
        public List<Guid> CuisineIds { get; set; } = new();
    }

    public class SetRestaurantCuisinesValidator : AbstractValidator<SetRestaurantCuisinesDto>
    {
        public SetRestaurantCuisinesValidator()
        {
            RuleFor(x => x.CuisineIds)
                .NotNull().WithMessage("CuisineIds is required.")
                .Must(x => x.Count <= 5).WithMessage("A restaurant can have at most 5 cuisines.");
        }
    }
}