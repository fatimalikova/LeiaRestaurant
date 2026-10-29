using FluentValidation;

namespace Leia.Application.Dtos.AddressDtos
{
    public class CreateAddressDto
    {
        public string Title { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public string? Note { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool IsDefault { get; set; }
    }

    public class CreateAddressValidator : AbstractValidator<CreateAddressDto>
    {
        public CreateAddressValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(50).WithMessage("Title cannot exceed 50 characters.");

            RuleFor(x => x.AddressLine)
                .NotEmpty().WithMessage("Address is required.")
                .MaximumLength(300).WithMessage("Address cannot exceed 300 characters.");

            RuleFor(x => x.Note)
                .MaximumLength(300).WithMessage("Note cannot exceed 300 characters.")
                .When(x => x.Note != null);

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("Latitude is invalid.")
                .When(x => x.Latitude.HasValue);

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("Longitude is invalid.")
                .When(x => x.Longitude.HasValue);
        }
    }
}