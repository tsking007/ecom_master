using FluentValidation;

namespace EcommerceApp.Application.Features.Wishlist.Commands;

public class AddToWishlistCommandValidator
    : AbstractValidator<AddToWishlistCommand>
{
    public AddToWishlistCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();
    }
}