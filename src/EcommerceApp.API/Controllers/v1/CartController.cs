using EcommerceApp.Application.Features.Cart.Commands;
using EcommerceApp.Application.Features.Cart.DTOs;
using EcommerceApp.Application.Features.Cart.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/cart")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ISender _sender;

    public CartController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCart(CancellationToken ct)
    {
        var result = await _sender.Send(new GetCartQuery(), ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddToCart(
        [FromBody] AddToCartRequest request,
        CancellationToken ct)
    {
        await _sender.Send(new AddToCartCommand(request.ProductId, request.Quantity), ct);
        return Ok(new { message = "Item added to cart." });
    }

    [HttpPut("{cartItemId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCartItem(
        Guid cartItemId,
        [FromBody] UpdateCartItemRequest request,
        CancellationToken ct)
    {
        await _sender.Send(new UpdateCartItemCommand(cartItemId, request.Quantity), ct);
        return Ok(new { message = "Cart item updated." });
    }

    [HttpDelete("{cartItemId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveCartItem(Guid cartItemId, CancellationToken ct)
    {
        await _sender.Send(new RemoveCartItemCommand(cartItemId), ct);
        return Ok(new { message = "Cart item removed." });
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearCart(CancellationToken ct)
    {
        await _sender.Send(new ClearCartCommand(), ct);
        return Ok(new { message = "Cart cleared." });
    }

    public class AddToCartRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateCartItemRequest
    {
        public int Quantity { get; set; }
    }
}