using EcommerceApp.Application.Common.Validators;
using EcommerceApp.Application.Features.Auth.Commands;
using FluentValidation;

namespace EcommerceApp.Application.Features.Auth.Validators;

public class ResetPasswordCommandValidator
    : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
                .WithMessage("Password reset token is required.");

        RuleFor(x => x.NewPassword)
            .ApplyPasswordRules();

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
                .WithMessage("Password confirmation is required.")
            .Equal(x => x.NewPassword)
                .WithMessage("Passwords do not match.");
    }
}