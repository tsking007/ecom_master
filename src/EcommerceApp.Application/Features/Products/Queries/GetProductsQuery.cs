using AutoMapper;
using EcommerceApp.Application.Features.Products.DTOs;
using EcommerceApp.Domain.Common;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Products.Queries;

// ── Query ─────────────────────────────────────────────────────────────────────

public record GetProductsQuery(
    int PageNumber = 1,
    int PageSize = 20,
    string? Category = null,
    string? SubCategory = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    double? MinRating = null,
    string? Brand = null,
    string? SortBy = null,
    bool SortDescending = true,
    bool? IsActive = true   // ← ADDED: null = all (admin), true = active only (public)
) : IRequest<PagedResult<ProductListDto>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public class GetProductsQueryHandler
    : IRequestHandler<GetProductsQuery, PagedResult<ProductListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProductsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<ProductListDto>> Handle(
        GetProductsQuery query,
        CancellationToken cancellationToken)
    {
        var pageSize = Math.Min(query.PageSize, 100);

        var paged = await _unitOfWork.Products.GetPagedAsync(
            pageNumber: query.PageNumber,
            pageSize: pageSize,
            category: query.Category,
            subCategory: query.SubCategory,
            minPrice: query.MinPrice,
            maxPrice: query.MaxPrice,
            minRating: query.MinRating,
            brand: query.Brand,
            sortBy: query.SortBy,
            sortDescending: query.SortDescending,
            isActive: query.IsActive,   // ← CHANGED: was hardcoded true
            cancellationToken: cancellationToken);

        var dtos = _mapper.Map<IReadOnlyList<ProductListDto>>(paged.Items);

        return PagedResult<ProductListDto>.Create(
            dtos, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }
}