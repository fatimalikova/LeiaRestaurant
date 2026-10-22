using FluentValidation;

namespace Leia.Application.Dtos.UserDtos
{
    public class Verify2FADto
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class Verify2FAValidator : AbstractValidator<Verify2FADto>
    {
        public Verify2FAValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email format is invalid.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Code is required.")
                .Length(6).WithMessage("Code must be 6 digits.");
        }
    }
}