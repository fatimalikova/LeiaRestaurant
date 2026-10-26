using FluentValidation;

namespace Leia.Application.Dtos.SliderDtos
{
    public class UpdateSliderDto
    {
        public string ImageUrl { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public string? Link { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateSliderValidator : AbstractValidator<UpdateSliderDto>
    {
        public UpdateSliderValidator()
        {
            RuleFor(x => x.ImageUrl)
                .NotEmpty().WithMessage("Image URL is required.")
                .MaximumLength(500).WithMessage("Image URL cannot exceed 500 characters.");

            RuleFor(x => x.Title)
                .MaximumLength(150).WithMessage("Title cannot exceed 150 characters.")
                .When(x => x.Title != null);

            RuleFor(x => x.Subtitle)
                .MaximumLength(250).WithMessage("Subtitle cannot exceed 250 characters.")
                .When(x => x.Subtitle != null);

            RuleFor(x => x.Link)
                .MaximumLength(500).WithMessage("Link cannot exceed 500 characters.")
                .When(x => x.Link != null);
        }
    }
}