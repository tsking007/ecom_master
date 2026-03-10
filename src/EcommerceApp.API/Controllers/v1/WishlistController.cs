using EcommerceApp.Application.Features.Wishlist.Commands;
using EcommerceApp.Application.Features.Wishlist.DTOs;
using EcommerceApp.Application.Features.Wishlist.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/wishlist")]
[Authorize]
public class WishlistController : ControllerBase
{
    private readonly ISender _sender;

    public WishlistController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(WishlistDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWishlist(CancellationToken ct)
    {
        var result = await _sender.Send(new GetWishlistQuery(), ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddToWishlist(
        [FromBody] AddToWishlistRequest request,
        CancellationToken ct)
    {
        await _sender.Send(new AddToWishlistCommand(request.ProductId), ct);
        return Ok(new { message = "Item added to wishlist." });
    }

    [HttpDelete("{wishlistId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveWishlistItem(
        Guid wishlistId,
        CancellationToken ct)
    {
        await _sender.Send(new RemoveWishlistItemCommand(wishlistId), ct);
        return Ok(new { message = "Wishlist item removed." });
    }

    [HttpPost("{wishlistId:guid}/move-to-cart")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MoveToCart(
        Guid wishlistId,
        CancellationToken ct)
    {
        await _sender.Send(new MoveWishlistItemToCartCommand(wishlistId), ct);
        return Ok(new { message = "Wishlist item moved to cart." });
    }

    public class AddToWishlistRequest
    {
        public Guid ProductId { get; set; }
    }
}