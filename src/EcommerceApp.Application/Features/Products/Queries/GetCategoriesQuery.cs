using EcommerceApp.Application.Features.Products.DTOs;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Products.Queries;

// ── Query ─────────────────────────────────────────────────────────────────────

/// <summary>
/// Returns all distinct categories with the count of active products in each.
/// Used on the Home page Category Grid and the All Categories page.
/// </summary>
public record GetCategoriesQuery : IRequest<IReadOnlyList<CategoryDto>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public class GetCategoriesQueryHandler
    : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCategoriesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<CategoryDto>> Handle(
        GetCategoriesQuery query,
        CancellationToken cancellationToken)
    {
        var categoriesWithCount = await _unitOfWork.Products
            .GetCategoriesWithCountAsync(cancellationToken);

        return categoriesWithCount
            .Select(c => new CategoryDto(c.Category, c.Count))
            .ToList();
    }
}