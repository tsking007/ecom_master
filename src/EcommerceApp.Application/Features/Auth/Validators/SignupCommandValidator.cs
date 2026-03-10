using EcommerceApp.Application.Common.Validators;
using EcommerceApp.Application.Features.Auth.Commands;
using FluentValidation;

namespace EcommerceApp.Application.Features.Auth.Validators;

public class SignupCommandValidator : AbstractValidator<SignupCommand>
{
    public SignupCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
                .WithMessage("First name is required.")
            .MinimumLength(2)
                .WithMessage("First name must be at least 2 characters.")
            .MaximumLength(100)
                .WithMessage("First name must not exceed 100 characters.")
            .Matches(@"^[a-zA-Z\s\-']+$")
                .WithMessage(
                    "First name must contain only letters, " +
                    "spaces, hyphens, or apostrophes.");

        RuleFor(x => x.LastName)
            .NotEmpty()
                .WithMessage("Last name is required.")
            .MinimumLength(2)
                .WithMessage("Last name must be at least 2 characters.")
            .MaximumLength(100)
                .WithMessage("Last name must not exceed 100 characters.")
            .Matches(@"^[a-zA-Z\s\-']+$")
                .WithMessage(
                    "Last name must contain only letters, " +
                    "spaces, hyphens, or apostrophes.");

        RuleFor(x => x.Email)
            .ApplyEmailRules();

        RuleFor(x => x.Password)
            .ApplyPasswordRules();

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
                .WithMessage("Password confirmation is required.")
            .Equal(x => x.Password)
                .WithMessage("Passwords do not match.");

        // Phone is optional — only validate format when provided
        When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber), () =>
        {
            RuleFor(x => x.PhoneNumber!)
                .ApplyPhoneRules();
        });
    }
}