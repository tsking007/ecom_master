using EcommerceApp.Application.Common.Validators;
using EcommerceApp.Application.Features.Auth.Commands;
using FluentValidation;

namespace EcommerceApp.Application.Features.Auth.Validators;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .ApplyEmailRules();

        // Do NOT enforce strength rules on login —
        // only the BCrypt verification decides if the password is correct.
        RuleFor(x => x.Password)
            .NotEmpty()
                .WithMessage("Password is required.");
    }
}