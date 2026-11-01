using FluentValidation;

namespace Leia.Application.Dtos.AllergenDtos
{
    public class SetMenuItemAllergensDto
    {
        public List<Guid> AllergenIds { get; set; } = new();
    }

    public class SetMenuItemAllergensValidator : AbstractValidator<SetMenuItemAllergensDto>
    {
        public SetMenuItemAllergensValidator()
        {
            RuleFor(x => x.AllergenIds)
                .NotNull().WithMessage("AllergenIds is required.")
                .Must(x => x.Count <= 20).WithMessage("An item can have at most 20 allergens.");
        }
    }
}