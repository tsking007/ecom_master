//using EcommerceApp.API.Models.Requests.Products;
//using EcommerceApp.Application.Features.Products.Commands;
//using EcommerceApp.Application.Features.Products.DTOs;
//using EcommerceApp.Application.Features.Products.Queries;
//using EcommerceApp.Domain.Common;
//using MediatR;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace EcommerceApp.API.Controllers.Admin;

///// <summary>
///// Admin product management — full CRUD + stock + active toggle.
///// All endpoints require the "Admin" role.
///// Unlike the public ProductsController, inactive products ARE visible here.
///// </summary>
//[ApiController]
//[Route("api/admin/products")]
//[Authorize(Roles = "Admin")]
//public class AdminProductsController : ControllerBase
//{
//    private readonly ISender _sender;

//    public AdminProductsController(ISender sender) => _sender = sender;

//    // ── GET /api/admin/products ───────────────────────────────────────────────
//    // Admin sees all products; optionally filter with ?isActive=true/false
//    [HttpGet]
//    [ProducesResponseType(typeof(PagedResult<ProductListDto>), StatusCodes.Status200OK)]
//    public async Task<IActionResult> GetProducts(
//        [FromQuery] GetProductsRequest req,
//        CancellationToken ct)
//    {
//        var query = new GetProductsQuery(
//            PageNumber: req.PageNumber,
//            PageSize: req.PageSize,
//            Category: req.Category,
//            SubCategory: req.SubCategory,
//            MinPrice: req.MinPrice,
//            MaxPrice: req.MaxPrice,
//            MinRating: req.MinRating,
//            Brand: req.Brand,
//            SortBy: req.SortBy,
//            SortDescending: req.SortDescending,
//            IsActive: req.IsActive);      // null = all, true = active only, false = inactive only

//        return Ok(await _sender.Send(query, ct));
//    }

//    // ── GET /api/admin/products/{id} ──────────────────────────────────────────
//    [HttpGet("{id:guid}")]
//    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status404NotFound)]
//    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
//        => Ok(await _sender.Send(
//            new GetProductBySlugQuery(Id: id, IgnoreActiveFilter: true), ct));

//    // ── POST /api/admin/products ──────────────────────────────────────────────
//    [HttpPost]
//    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
//    [ProducesResponseType(StatusCodes.Status400BadRequest)]
//    public async Task<IActionResult> Create(
//        [FromBody] CreateProductRequest req,
//        CancellationToken ct)
//    {
//        var command = new CreateProductCommand(
//            Name: req.Name,
//            Description: req.Description,
//            ShortDescription: req.ShortDescription,
//            Price: req.Price,
//            DiscountedPrice: req.DiscountedPrice,
//            StockQuantity: req.StockQuantity,
//            Category: req.Category,
//            SubCategory: req.SubCategory,
//            Brand: req.Brand,
//            ImageUrls: req.ImageUrls,
//            VideoUrl: req.VideoUrl,
//            Tags: req.Tags,
//            IsFeatured: req.IsFeatured,
//            Weight: req.Weight,
//            Dimensions: req.Dimensions);

//        var result = await _sender.Send(command, ct);

//        return CreatedAtAction(
//            nameof(GetById),
//            new { id = result.Id },
//            result);
//    }

//    // ── PUT /api/admin/products/{id} ──────────────────────────────────────────
//    [HttpPut("{id:guid}")]
//    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status400BadRequest)]
//    [ProducesResponseType(StatusCodes.Status404NotFound)]
//    public async Task<IActionResult> Update(
//        Guid id,
//        [FromBody] UpdateProductRequest req,
//        CancellationToken ct)
//    {
//        var command = new UpdateProductCommand(
//            ProductId: id,
//            Name: req.Name,
//            Description: req.Description,
//            ShortDescription: req.ShortDescription,
//            Price: req.Price,
//            DiscountedPrice: req.DiscountedPrice,
//            StockQuantity: req.StockQuantity,
//            Category: req.Category,
//            SubCategory: req.SubCategory,
//            Brand: req.Brand,
//            ImageUrls: req.ImageUrls,
//            VideoUrl: req.VideoUrl,
//            Tags: req.Tags,
//            IsActive: req.IsActive,
//            IsFeatured: req.IsFeatured,
//            Weight: req.Weight,
//            Dimensions: req.Dimensions);

//        return Ok(await _sender.Send(command, ct));
//    }

//    // ── DELETE /api/admin/products/{id} ───────────────────────────────────────
//    [HttpDelete("{id:guid}")]
//    [ProducesResponseType(StatusCodes.Status204NoContent)]
//    [ProducesResponseType(StatusCodes.Status404NotFound)]
//    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
//    {
//        await _sender.Send(new DeleteProductCommand(id), ct);
//        return NoContent();
//    }

//    // ── PATCH /api/admin/products/{id}/stock ──────────────────────────────────
//    [HttpPatch("{id:guid}/stock")]
//    [ProducesResponseType(StatusCodes.Status204NoContent)]
//    [ProducesResponseType(StatusCodes.Status400BadRequest)]
//    [ProducesResponseType(StatusCodes.Status404NotFound)]
//    public async Task<IActionResult> UpdateStock(
//        Guid id,
//        [FromBody] UpdateStockRequest req,
//        CancellationToken ct)
//    {
//        await _sender.Send(new UpdateStockCommand(id, req.NewQuantity), ct);
//        return NoContent();
//    }

//    // ── PATCH /api/admin/products/{id}/toggle-active ─────────────────────────
//    [HttpPatch("{id:guid}/toggle-active")]
//    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status404NotFound)]
//    public async Task<IActionResult> ToggleActive(Guid id, CancellationToken ct)
//    {
//        var isActive = await _sender.Send(new ToggleActiveCommand(id), ct);
//        return Ok(new { IsActive = isActive });
//    }
//}