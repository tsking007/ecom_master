using EcommerceApp.Application.Features.Auth.Commands;
using FluentValidation;

namespace EcommerceApp.Application.Features.Auth.Validators;

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
                .WithMessage("User ID is required.");

        RuleFor(x => x.RefreshToken)
            .NotEmpty()
                .WithMessage("Refresh token is required.");
    }
}