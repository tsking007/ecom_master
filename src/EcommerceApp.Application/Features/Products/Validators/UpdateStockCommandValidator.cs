using EcommerceApp.Application.Common.Validators;
using EcommerceApp.Application.Features.Products.Commands;
using FluentValidation;

namespace EcommerceApp.Application.Features.Products.Validators;

public class UpdateStockCommandValidator : AbstractValidator<UpdateStockCommand>
{
    public UpdateStockCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .ApplyGuidRules("Product ID");

        RuleFor(x => x.NewQuantity)
            .ApplyStockQuantityRules();
    }
}