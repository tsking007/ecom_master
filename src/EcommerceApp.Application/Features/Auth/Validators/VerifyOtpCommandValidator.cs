using EcommerceApp.Application.Common.Validators;
using EcommerceApp.Application.Features.Auth.Commands;
using EcommerceApp.Domain.Enums;
using FluentValidation;

namespace EcommerceApp.Application.Features.Auth.Validators;

public class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommand>
{
    public VerifyOtpCommandValidator()
    {
        RuleFor(x => x.Identifier)
            .NotEmpty()
                .WithMessage("Identifier (email or phone) is required.")
            .MaximumLength(256)
                .WithMessage("Identifier must not exceed 256 characters.");

        RuleFor(x => x.Otp)
            .NotEmpty()
                .WithMessage("OTP is required.")
            .Length(6)
                .WithMessage("OTP must be exactly 6 digits.")
            .Matches(@"^\d{6}$")
                .WithMessage("OTP must contain only digits.");

        RuleFor(x => x.Purpose)
            .IsInEnum()
                .WithMessage(
                    "Invalid OTP purpose. " +
                    "Valid values: " +
                    string.Join(", ", Enum.GetNames<OtpPurpose>()));
    }
}