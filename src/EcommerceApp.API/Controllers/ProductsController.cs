using EcommerceApp.API.Models.Requests.Products;
using EcommerceApp.Application.Features.Products.DTOs;
using EcommerceApp.Application.Features.Products.Queries;
using EcommerceApp.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApp.API.Controllers;

/// <summary>
/// Public product catalogue endpoints — no authentication required.
/// All queries use IsActive: true so inactive products are invisible to shoppers.
/// </summary>
[ApiController]
[Route("api/products")]
[AllowAnonymous]
public class ProductsController : ControllerBase
{
    private readonly ISender _sender;

    public ProductsController(ISender sender) => _sender = sender;

    // ── GET /api/products ──────────────────────────────────────────────────────
    // ?pageNumber=1&pageSize=20&category=Shoes&brand=Nike&minPrice=500&sortBy=price
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] GetProductsRequest req,
        CancellationToken ct)
    {
        var query = new GetProductsQuery(
            PageNumber: req.PageNumber,
            PageSize: req.PageSize,
            Category: req.Category,
            SubCategory: req.SubCategory,
            MinPrice: req.MinPrice,
            MaxPrice: req.MaxPrice,
            MinRating: req.MinRating,
            Brand: req.Brand,
            SortBy: req.SortBy,
            SortDescending: req.SortDescending,
            IsActive: true);           // public always sees active only

        return Ok(await _sender.Send(query, ct));
    }

    // ── GET /api/products/categories ──────────────────────────────────────────
    [HttpGet("categories")]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories(CancellationToken ct)
        => Ok(await _sender.Send(new GetCategoriesQuery(), ct));

    // ── GET /api/products/bestsellers?count=8 ─────────────────────────────────
    [HttpGet("bestsellers")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBestsellers(
        [FromQuery] int count = 8,
        CancellationToken ct = default)
        => Ok(await _sender.Send(new GetBestsellersQuery(count), ct));

    // ── GET /api/products/{slug} ───────────────────────────────────────────────
    // Must be declared LAST — {slug} is a catch-all and would swallow
    // /categories and /bestsellers if placed before them.
    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
        => Ok(await _sender.Send(new GetProductBySlugQuery(Slug: slug), ct));
}