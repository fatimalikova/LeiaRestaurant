using FluentValidation;
using Leia.Domain.Enums;

namespace Leia.Application.Dtos.RestaurantDtos
{
    public class UpdateRestaurantStatusDto
    {
        public RestaurantStatus Status { get; set; }
    }

    public class UpdateRestaurantStatusValidator : AbstractValidator<UpdateRestaurantStatusDto>
    {
        public UpdateRestaurantStatusValidator()
        {
            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Status is invalid.");
        }
    }
}