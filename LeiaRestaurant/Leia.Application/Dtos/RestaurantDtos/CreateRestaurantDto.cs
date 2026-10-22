using FluentValidation;

namespace Leia.Application.Dtos.RestaurantDtos
{
    public class CreateRestaurantDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? CoverUrl { get; set; }
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? ContactEmail { get; set; }
        public string? InstagramUrl { get; set; }
        public string? FacebookUrl { get; set; }
        public bool AcceptsDelivery { get; set; } = true;
        public bool AcceptsPickup { get; set; } = true;
        public decimal MinOrderAmount { get; set; }
        public decimal DeliveryFee { get; set; }
    }

    public class CreateRestaurantValidator : AbstractValidator<CreateRestaurantDto>
    {
        public CreateRestaurantValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Restaurant name is required.")
                .MaximumLength(150).WithMessage("Name cannot exceed 150 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.")
                .When(x => x.Description != null);

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required.")
                .MaximumLength(300).WithMessage("Address cannot exceed 300 characters.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .MaximumLength(30).WithMessage("Phone number cannot exceed 30 characters.");

            RuleFor(x => x.ContactEmail)
                .EmailAddress().WithMessage("Contact email format is invalid.")
                .When(x => !string.IsNullOrEmpty(x.ContactEmail));

            RuleFor(x => x.MinOrderAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum order amount cannot be negative.");

            RuleFor(x => x.DeliveryFee)
                .GreaterThanOrEqualTo(0).WithMessage("Delivery fee cannot be negative.");

            RuleFor(x => x)
                .Must(x => x.AcceptsDelivery || x.AcceptsPickup)
                .WithMessage("Restaurant must accept delivery or pickup.");
        }
    }
}