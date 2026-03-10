using AutoMapper;
using EcommerceApp.Application.Common.Exceptions;
using EcommerceApp.Application.Features.Products.DTOs;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Products.Queries;

// ── Query ─────────────────────────────────────────────────────────────────────

public record GetProductBySlugQuery(
    string? Slug = null,
    Guid? Id = null,
    bool IgnoreActiveFilter = false   // ← ADDED: admin sets this true to see inactive products
) : IRequest<ProductDto>;

// ── Handler ───────────────────────────────────────────────────────────────────

public class GetProductBySlugQueryHandler
    : IRequestHandler<GetProductBySlugQuery, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProductBySlugQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductDto> Handle(
        GetProductBySlugQuery query,
        CancellationToken cancellationToken)
    {
        Domain.Entities.Product? product = null;

        if (query.Slug is not null)
            product = await _unitOfWork.Products.GetBySlugAsync(
                query.Slug, cancellationToken);
        else if (query.Id.HasValue)
            product = await _unitOfWork.Products.GetByIdAsync(
                query.Id.Value, cancellationToken);

        // Public: inactive products are treated as not found
        // Admin: passes IgnoreActiveFilter = true so inactive products are visible
        var notFound = product == null ||
                       (!query.IgnoreActiveFilter && !product.IsActive);

        if (notFound)
            throw new NotFoundException(
                "Product",
                (object?)query.Slug ?? query.Id?.ToString() ?? "unknown");

        return _mapper.Map<ProductDto>(product);
    }
}