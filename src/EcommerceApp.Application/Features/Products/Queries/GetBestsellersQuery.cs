using AutoMapper;
using EcommerceApp.Application.Features.Products.DTOs;
using EcommerceApp.Application.Common.Interfaces;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Products.Queries;

// ── Query ─────────────────────────────────────────────────────────────────────

/// <summary>
/// Returns the top-N products ordered by SoldCount descending.
/// Used on the Home page "Bestsellers" row.
/// </summary>
public record GetBestsellersQuery(int Count = 8) : IRequest<IReadOnlyList<ProductListDto>>, ICachedQuery { 
    
    public string CacheKey => $"bestsellers:{Count}";
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(15);
}

// ── Handler ───────────────────────────────────────────────────────────────────

public class GetBestsellersQueryHandler
    : IRequestHandler<GetBestsellersQuery, IReadOnlyList<ProductListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetBestsellersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ProductListDto>> Handle(
        GetBestsellersQuery query,
        CancellationToken cancellationToken)
    {
        var count = Math.Clamp(query.Count, 1, 50);
        var products = await _unitOfWork.Products.GetBestsellersAsync(
            count, cancellationToken);

        return _mapper.Map<IReadOnlyList<ProductListDto>>(products);
    }
}