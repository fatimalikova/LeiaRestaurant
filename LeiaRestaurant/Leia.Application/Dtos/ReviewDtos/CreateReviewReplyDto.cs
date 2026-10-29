using FluentValidation;

namespace Leia.Application.Dtos.ReviewDtos
{
    public class CreateReviewReplyDto
    {
        public string Content { get; set; } = string.Empty;
    }

    public class CreateReviewReplyValidator : AbstractValidator<CreateReviewReplyDto>
    {
        public CreateReviewReplyValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Reply is required.")
                .MaximumLength(1000).WithMessage("Reply cannot exceed 1000 characters.");
        }
    }
}