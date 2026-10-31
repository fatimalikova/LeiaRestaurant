using FluentValidation;

namespace Leia.Application.Dtos.TableDtos
{
    public class CreateTableDto
    {
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string? Zone { get; set; }
    }

    public class CreateTableValidator : AbstractValidator<CreateTableDto>
    {
        public CreateTableValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Table name is required.")
                .MaximumLength(50).WithMessage("Name cannot exceed 50 characters.");

            RuleFor(x => x.Capacity)
                .InclusiveBetween(1, 30).WithMessage("Capacity must be between 1 and 30.");

            RuleFor(x => x.Zone)
                .MaximumLength(50).WithMessage("Zone cannot exceed 50 characters.")
                .When(x => x.Zone != null);
        }
    }
}