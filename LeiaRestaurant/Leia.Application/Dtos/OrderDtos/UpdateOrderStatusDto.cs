using FluentValidation;
using Leia.Domain.Enums;

namespace Leia.Application.Dtos.OrderDtos
{
    public class UpdateOrderStatusDto
    {
        public OrderStatus Status { get; set; }
        public string? Comment { get; set; }
    }

    public class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusDto>
    {
        public UpdateOrderStatusValidator()
        {
            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Status is invalid.");

            RuleFor(x => x.Comment)
                .NotEmpty().WithMessage("Reason is required when rejecting an order.")
                .When(x => x.Status == OrderStatus.Rejected);

            RuleFor(x => x.Comment)
                .MaximumLength(500).WithMessage("Comment cannot exceed 500 characters.")
                .When(x => x.Comment != null);
        }
    }
}