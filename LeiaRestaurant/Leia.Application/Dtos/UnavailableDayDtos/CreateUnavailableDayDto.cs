using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Leia.Application.Dtos.UnavailableDayDtos
{
    public class CreateUnavailableDayDto
    {
        public DateTime Date { get; set; }
        public string? Reason { get; set; }
    }

    public class CreateUnavailableDayValidator : AbstractValidator<CreateUnavailableDayDto>
    {
        public CreateUnavailableDayValidator()
        {
            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("Date is required.")
                .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Date cannot be in the past.");

            RuleFor(x => x.Reason)
                .MaximumLength(250).WithMessage("Reason cannot exceed 250 characters.")
                .When(x => x.Reason != null);
        }
    }
}
