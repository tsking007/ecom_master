using EcommerceApp.Application.Features.Auth.Commands;
using FluentValidation;

namespace EcommerceApp.Application.Features.Auth.Validators;

public class RefreshTokenCommandValidator
    : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.AccessToken)
            .NotEmpty()
                .WithMessage("Access token is required.");

        RuleFor(x => x.RefreshToken)
            .NotEmpty()
                .WithMessage("Refresh token is required.");
    }
}