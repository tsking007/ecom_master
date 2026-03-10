using EcommerceApp.Application.Common.Validators;
using EcommerceApp.Application.Features.Auth.Commands;
using FluentValidation;

namespace EcommerceApp.Application.Features.Auth.Validators;

public class ForgotPasswordCommandValidator
    : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .ApplyEmailRules();
    }
}