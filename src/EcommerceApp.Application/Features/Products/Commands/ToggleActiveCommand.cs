using EcommerceApp.Application.Common.Exceptions;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Products.Commands;

// ── Command ───────────────────────────────────────────────────────────────────

/// <summary>
/// Toggles a product's IsActive flag.
/// Deactivated products vanish from the public catalog immediately
/// but remain accessible for historical order lookups.
/// Returns the new IsActive value.
/// </summary>
public record ToggleActiveCommand(Guid ProductId) : IRequest<bool>;

// ── Handler ───────────────────────────────────────────────────────────────────

public class ToggleActiveCommandHandler : IRequestHandler<ToggleActiveCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public ToggleActiveCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(
        ToggleActiveCommand command,
        CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(
            command.ProductId, cancellationToken)
            ?? throw new NotFoundException("Product", command.ProductId);

        product.IsActive = !product.IsActive;

        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return product.IsActive;
    }
}