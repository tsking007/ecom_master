using EcommerceApp.Application.Features.Search.DTOs;
using EcommerceApp.Application.Features.Search.Queries;
using EcommerceApp.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EcommerceApp.API.Controllers;

/// <summary>
/// Product search endpoint.
///
/// GET /api/search?term=shoes&page=1&pageSize=20
///
/// Routes to whichever ISearchService is active:
///   - SearchSettings:Provider = "Elasticsearch" → full fuzzy search
///   - SearchSettings:Provider = "Sql"           → LIKE-based fallback
///
/// Anonymous — no auth required.
/// Rate limited in Part 22 (token bucket: 100 tokens/min, 2/sec refill).
/// </summary>
[ApiController]
[Route("api/search")]
[AllowAnonymous]
[EnableRateLimiting("search")]
public class SearchController : ControllerBase
{
    private readonly ISender _sender;

    public SearchController(ISender sender) => _sender = sender;

    // ── GET /api/search ───────────────────────────────────────────────────────

    /// <summary>
    /// Search products by text query.
    /// Returns a paginated result set matching the search term.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<SearchResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search(
        [FromQuery] string term,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(
            new SearchProductsQuery(term, page, pageSize), ct);

        return Ok(result);
    }
}