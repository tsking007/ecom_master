using EcommerceApp.Application.Common.Validators;
using EcommerceApp.Application.Features.Products.Commands;
using FluentValidation;

namespace EcommerceApp.Application.Features.Products.Validators;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(300).WithMessage("Name must not exceed 300 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(5000).WithMessage("Description must not exceed 5000 characters.");

        RuleFor(x => x.ShortDescription)
            .MaximumLength(1000)
            .When(x => x.ShortDescription is not null);

        RuleFor(x => x.Price)
            .ApplyPriceRules("Price");

        // Discounted price: only validate when provided
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
            .MaximumLength(100).WithMessage("Category must not exceed 100 characters.");

        RuleFor(x => x.SubCategory)
            .MaximumLength(100)
            .When(x => x.SubCategory is not null);

        RuleFor(x => x.Brand)
            .MaximumLength(100)
            .When(x => x.Brand is not null);

        RuleFor(x => x.ImageUrls)
            .Must(urls => urls == null || urls.Count <= 10)
                .WithMessage("A product can have at most 10 images.");

        RuleFor(x => x.Tags)
            .Must(tags => tags == null || tags.Count <= 20)
                .WithMessage("A product can have at most 20 tags.");
    }
}