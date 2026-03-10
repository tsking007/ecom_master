using EcommerceApp.Application.Common.Validators;
using EcommerceApp.Application.Features.Products.Commands;
using FluentValidation;

namespace EcommerceApp.Application.Features.Products.Validators;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .ApplyGuidRules("Product ID");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(300);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(5000);

        RuleFor(x => x.ShortDescription)
            .MaximumLength(1000)
            .When(x => x.ShortDescription is not null);

        RuleFor(x => x.Price)
            .ApplyPriceRules("Price");

        When(x => x.DiscountedPrice.HasValue, () =>
        {
            RuleFor(x => x.DiscountedPrice!.Value)
                .GreaterThan(0)
                    .WithMessage("Discounted price must be greater than 0.")
                .LessThan(x => x.Price)
                    .WithMessage("Discounted price must be less than the base price.");
        });

        RuleFor(x => x.StockQuantity)
            .ApplyStockQuantityRules();

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required.")
            .MaximumLength(100);

        RuleFor(x => x.SubCategory)
            .MaximumLength(100).When(x => x.SubCategory is not null);

        RuleFor(x => x.Brand)
            .MaximumLength(100).When(x => x.Brand is not null);
    }
}