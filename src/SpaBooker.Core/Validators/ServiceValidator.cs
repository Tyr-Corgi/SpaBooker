using FluentValidation;
using SpaBooker.Core.Entities;

namespace SpaBooker.Core.Validators;

public class ServiceValidator : AbstractValidator<SpaService>
{
    public ServiceValidator()
    {
        RuleFor(s => s.Name)
            .NotEmpty().WithMessage("Service name is required")
            .MaximumLength(200).WithMessage("Service name cannot exceed 200 characters")
            .Matches(@"^[a-zA-Z0-9\s\-&',\.()]*$").WithMessage("Service name contains invalid characters");

        RuleFor(s => s.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

        RuleFor(s => s.BasePrice)
            .GreaterThan(0).WithMessage("Price must be greater than zero")
            .LessThan(100000).WithMessage("Price seems unreasonably high");

        RuleFor(s => s.DurationMinutes)
            .GreaterThan(0).WithMessage("Duration must be greater than zero")
            .LessThan(1440).WithMessage("Duration cannot exceed 24 hours");

        RuleFor(s => s.LocationId)
            .GreaterThan(0).WithMessage("Location is required");
    }
}

