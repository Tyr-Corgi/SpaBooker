using FluentValidation;

namespace SpaBooker.Core.Validators;

public class UserRegistrationModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
}

public class UserRegistrationValidator : AbstractValidator<UserRegistrationModel>
{
    public UserRegistrationValidator()
    {
        RuleFor(u => u.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(256).WithMessage("Email cannot exceed 256 characters");

        RuleFor(u => u.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(12).WithMessage("Password must be at least 12 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");

        RuleFor(u => u.ConfirmPassword)
            .Equal(u => u.Password).WithMessage("Passwords do not match");

        RuleFor(u => u.FirstName)
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters")
            .Matches(@"^[a-zA-Z\s\-']*$").WithMessage("First name contains invalid characters")
            .When(u => !string.IsNullOrWhiteSpace(u.FirstName));

        RuleFor(u => u.LastName)
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters")
            .Matches(@"^[a-zA-Z\s\-']*$").WithMessage("Last name contains invalid characters")
            .When(u => !string.IsNullOrWhiteSpace(u.LastName));

        RuleFor(u => u.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format")
            .When(u => !string.IsNullOrWhiteSpace(u.PhoneNumber));
    }
}

