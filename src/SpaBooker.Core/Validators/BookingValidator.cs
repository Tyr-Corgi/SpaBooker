using FluentValidation;
using SpaBooker.Core.Entities;

namespace SpaBooker.Core.Validators;

public class BookingValidator : AbstractValidator<Booking>
{
    public BookingValidator()
    {
        RuleFor(b => b.ClientId)
            .NotEmpty().WithMessage("Client is required");

        RuleFor(b => b.ServiceId)
            .NotEmpty().WithMessage("Service is required");

        RuleFor(b => b.StartTime)
            .NotEmpty().WithMessage("Start time is required")
            .Must(BeInFuture).WithMessage("Start time must be in the future");

        RuleFor(b => b.EndTime)
            .NotEmpty().WithMessage("End time is required")
            .GreaterThan(b => b.StartTime).WithMessage("End time must be after start time");

        RuleFor(b => b.TotalPrice)
            .GreaterThan(0).WithMessage("Total price must be greater than zero")
            .LessThan(100000).WithMessage("Total price seems unreasonably high");

        RuleFor(b => b.DepositAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Deposit amount cannot be negative")
            .LessThanOrEqualTo(b => b.TotalPrice).WithMessage("Deposit amount cannot exceed total price");

        RuleFor(b => b.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters");
    }

    private bool BeInFuture(DateTime startTime)
    {
        return startTime > DateTime.UtcNow;
    }
}

