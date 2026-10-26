using FluentValidation;

namespace Leia.Application.Dtos.UserDtos
{
    public class EmailConfirmDto
    {
        public Guid UserId { get; set; }
        public string Token { get; set; } = string.Empty;
    }

    public class EmailConfirmValidator : AbstractValidator<EmailConfirmDto>
    {
        public EmailConfirmValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required.");

            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("Token is required.");
        }
    }
}