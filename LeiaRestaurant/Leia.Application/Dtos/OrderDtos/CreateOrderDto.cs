using FluentValidation;
using Leia.Domain.Enums;

namespace Leia.Application.Dtos.OrderDtos
{
    public class CreateOrderDto
    {
        public Guid RestaurantId { get; set; }
        public OrderType Type { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? DeliveryAddress { get; set; }
        public string CustomerPhone { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }

    public class CreateOrderItemDto
    {
        public Guid MenuItemId { get; set; }
        public int Quantity { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateOrderValidator : AbstractValidator<CreateOrderDto>
    {
        public CreateOrderValidator()
        {
            RuleFor(x => x.RestaurantId)
                .NotEmpty().WithMessage("RestaurantId is required.");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Order type is invalid.");

            RuleFor(x => x.PaymentMethod)
                .IsInEnum().WithMessage("Payment method is invalid.");

            RuleFor(x => x.CustomerPhone)
                .NotEmpty().WithMessage("Phone number is required.")
                .MaximumLength(30).WithMessage("Phone number cannot exceed 30 characters.");

            RuleFor(x => x.DeliveryAddress)
                .NotEmpty().WithMessage("Delivery address is required for delivery orders.")
                .When(x => x.Type == OrderType.Delivery);

            RuleFor(x => x.DeliveryAddress)
                .MaximumLength(300).WithMessage("Address cannot exceed 300 characters.")
                .When(x => x.DeliveryAddress != null);

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.")
                .When(x => x.Notes != null);

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("Order must contain at least one item.");

            RuleForEach(x => x.Items).SetValidator(new CreateOrderItemValidator());
        }
    }

    public class CreateOrderItemValidator : AbstractValidator<CreateOrderItemDto>
    {
        public CreateOrderItemValidator()
        {
            RuleFor(x => x.MenuItemId)
                .NotEmpty().WithMessage("MenuItemId is required.");

            RuleFor(x => x.Quantity)
                .InclusiveBetween(1, 50).WithMessage("Quantity must be between 1 and 50.");

            RuleFor(x => x.Notes)
                .MaximumLength(300).WithMessage("Item notes cannot exceed 300 characters.")
                .When(x => x.Notes != null);
        }
    }
}