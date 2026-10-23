using FluentValidation;

namespace Leia.Application.Dtos.SystemSettingDtos
{
    public class UpdateSystemSettingDto
    {
        public int MinCancellationMinutes { get; set; }
        public int RestaurantResponseMinutes { get; set; }
        public bool RequireRestaurantApproval { get; set; }
    }

    public class UpdateSystemSettingValidator : AbstractValidator<UpdateSystemSettingDto>
    {
        public UpdateSystemSettingValidator()
        {
            RuleFor(x => x.MinCancellationMinutes)
                .InclusiveBetween(0, 120).WithMessage("Cancellation window must be between 0 and 120 minutes.");

            RuleFor(x => x.RestaurantResponseMinutes)
                .InclusiveBetween(1, 60).WithMessage("Response time must be between 1 and 60 minutes.");
        }
    }
}