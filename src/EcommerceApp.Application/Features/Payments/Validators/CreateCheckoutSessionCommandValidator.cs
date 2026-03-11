using FluentValidation;

namespace EcommerceApp.Application.Features.Payments.Commands;

public class CreateCheckoutSessionCommandValidator
    : AbstractValidator<CreateCheckoutSessionCommand>
{
    public CreateCheckoutSessionCommandValidator()
    {
        RuleFor(x => x.AddressId)
            .Must(id => !id.HasValue || id.Value != Guid.Empty)
            .WithMessage("AddressId must be a valid GUID when provided.");
    }
}