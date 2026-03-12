using EcommerceApp.API.Models.Requests.Products;
using EcommerceApp.Application.Features.Products.Commands;
using EcommerceApp.Application.Features.Products.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApp.API.Controllers.Admin;

/// <summary>
/// Admin product management — create, update, stock update, soft-delete.
/// Route: /api/v1/admin/products
/// All endpoints require the "Admin" role.
/// </summary>
[ApiController]
[Route("api/v1/admin/products")]
[Authorize(Roles = "Admin")]
public class AdminProductsController : ControllerBase
{
    private readonly ISender _sender;

    public AdminProductsController(ISender sender) => _sender = sender;

    // -------------------------------------------------------------------------
    // POST /api/v1/admin/products
    // -------------------------------------------------------------------------
    /// <summary>Creates a new product. Slug is auto-generated from the name.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductRequest req,
        CancellationToken ct)
    {
        var command = new CreateProductCommand(
            Name: req.Name,
            Description: req.Description,
            ShortDescription: req.ShortDescription,
            Price: req.Price,
            DiscountedPrice: req.DiscountedPrice,
            StockQuantity: req.StockQuantity,
            Category: req.Category,
            SubCategory: req.SubCategory,
            Brand: req.Brand,
            ImageUrls: req.ImageUrls,
            VideoUrl: req.VideoUrl,
            Tags: req.Tags,
            IsFeatured: req.IsFeatured,
            Weight: req.Weight,
            Dimensions: req.Dimensions);

        var result = await _sender.Send(command, ct);

        // 201 with Location header pointing to the new resource
        return CreatedAtAction(
            actionName: nameof(Update),   // closest named action we have
            routeValues: new { id = result.Id },
            value: result);
    }

    // -------------------------------------------------------------------------
    // PUT /api/v1/admin/products/{id}
    // -------------------------------------------------------------------------
    /// <summary>
    /// Full product update. Slug is regenerated only if the name changes.
    /// Publishes ProductUpdatedNotification (and PriceDroppedNotification if price fell).
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateProductRequest req,
        CancellationToken ct)
    {
        var command = new UpdateProductCommand(
            ProductId: id,
            Name: req.Name,
            Description: req.Description,
            ShortDescription: req.ShortDescription,
            Price: req.Price,
            DiscountedPrice: req.DiscountedPrice,
            StockQuantity: req.StockQuantity,
            Category: req.Category,
            SubCategory: req.SubCategory,
            Brand: req.Brand,
            ImageUrls: req.ImageUrls,
            VideoUrl: req.VideoUrl,
            Tags: req.Tags,
            IsActive: req.IsActive,
            IsFeatured: req.IsFeatured,
            Weight: req.Weight,
            Dimensions: req.Dimensions);

        return Ok(await _sender.Send(command, ct));
    }

    // -------------------------------------------------------------------------
    // PUT /api/v1/admin/products/{id}/stock
    // -------------------------------------------------------------------------
    /// <summary>
    /// Updates the physical stock quantity.
    /// Rejects the request if NewQuantity is below the current ReservedQuantity
    /// (units locked by pending Stripe checkout sessions).
    /// </summary>
    [HttpPut("{id:guid}/stock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStock(
        Guid id,
        [FromBody] UpdateStockRequest req,
        CancellationToken ct)
    {
        await _sender.Send(new UpdateStockCommand(id, req.NewQuantity), ct);
        return Ok();
    }

    // -------------------------------------------------------------------------
    // DELETE /api/v1/admin/products/{id}
    // -------------------------------------------------------------------------
    /// <summary>
    /// Soft-deletes the product (IsDeleted = true).
    /// The product is hidden from all public queries but order history is preserved.
    /// Publishes ProductDeletedNotification so Elasticsearch is updated.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteProductCommand(id), ct);
        return NoContent();
    }
}