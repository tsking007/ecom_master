using EcommerceApp.Application.Common;
using EcommerceApp.Application.Common.Exceptions;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Wishlist.Commands;

public record RemoveWishlistItemCommand(Guid WishlistId) : IRequest;

public class RemoveWishlistItemCommandHandler
    : IRequestHandler<RemoveWishlistItemCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public RemoveWishlistItemCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task Handle(
        RemoveWishlistItemCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var wishlistItem = await _unitOfWork.Wishlists.GetByIdAsync(
            request.WishlistId,
            cancellationToken)
            ?? throw new NotFoundException("WishlistItem", request.WishlistId);

        if (wishlistItem.UserId != userId)
            throw new ForbiddenException(
                "You do not have permission to remove this wishlist item.");

        //await _unitOfWork.Wishlists.SoftDeleteAsync(
        //    wishlistItem,
        //    cancellationToken);

        await _unitOfWork.Wishlists.HardDeleteAsync(
            wishlistItem,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}