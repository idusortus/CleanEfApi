using CleanEfApi.Application.DTOs;
using FluentValidation;

namespace CleanEfApi.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(e => e.Email)
            .NotEmpty().WithMessage("Email address is required")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(p => p.Password)
            .NotEmpty().WithMessage("Password is required.")
            .Length(6).WithMessage("Password must contain at least six characters");

        RuleFor(c => c.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required.")
            .Equal(p => p.Password).WithMessage("Passwords do not match."); ;
    }
}