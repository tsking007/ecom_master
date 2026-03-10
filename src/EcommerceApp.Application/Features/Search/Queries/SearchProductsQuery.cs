using EcommerceApp.Application.Features.Search.DTOs;
using EcommerceApp.Domain.Common;
using EcommerceApp.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace EcommerceApp.Application.Features.Search.Queries;

// ── Query ─────────────────────────────────────────────────────────────────────

/// <summary>
/// Full-text product search query.
/// Dispatched by SearchController and routed to whichever ISearchService
/// implementation is active (Elasticsearch or SQL fallback).
/// </summary>
public record SearchProductsQuery(
    string Term,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<SearchResultDto>>;

// ── Validator ─────────────────────────────────────────────────────────────────

public class SearchProductsQueryValidator : AbstractValidator<SearchProductsQuery>
{
    public SearchProductsQueryValidator()
    {
        RuleFor(x => x.Term)
            .NotEmpty().WithMessage("Search term is required.")
            .MinimumLength(1).WithMessage("Search term must be at least 1 character.")
            .MaximumLength(200).WithMessage("Search term must not exceed 200 characters.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.");
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public class SearchProductsQueryHandler
    : IRequestHandler<SearchProductsQuery, PagedResult<SearchResultDto>>
{
    private readonly ISearchService _searchService;

    public SearchProductsQueryHandler(ISearchService searchService)
    {
        _searchService = searchService;
    }

    public async Task<PagedResult<SearchResultDto>> Handle(
        SearchProductsQuery query,
        CancellationToken cancellationToken)
    {
        return await _searchService.SearchAsync(
            term: query.Term,
            page: query.Page,
            pageSize: Math.Min(query.PageSize, 100),
            cancellationToken: cancellationToken);
    }
}