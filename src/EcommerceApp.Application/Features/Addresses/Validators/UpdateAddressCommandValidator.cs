using EcommerceApp.Application.Features.Addresses.Commands;
using FluentValidation;

namespace EcommerceApp.Application.Features.Addresses.Validators;

public class UpdateAddressCommandValidator : AbstractValidator<UpdateAddressCommand>
{
    private static readonly string[] AllowedTypes = ["Home", "Work", "Other"];

    public UpdateAddressCommandValidator()
    {
        RuleFor(x => x.AddressId)
            .NotEmpty().WithMessage("Address ID is required.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(200);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .MaximumLength(20);

        RuleFor(x => x.AddressLine1)
            .NotEmpty().WithMessage("Address line 1 is required.")
            .MaximumLength(200);

        RuleFor(x => x.AddressLine2)
            .MaximumLength(200)
            .When(x => x.AddressLine2 != null);

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100);

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("State is required.")
            .MaximumLength(100);

        RuleFor(x => x.PostalCode)
            .NotEmpty().WithMessage("Postal code is required.")
            .MaximumLength(20);

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required.")
            .MaximumLength(100);

        RuleFor(x => x.AddressType)
            .NotEmpty()
            .Must(t => AllowedTypes.Contains(t))
            .WithMessage("Address type must be Home, Work, or Other.");
    }
}