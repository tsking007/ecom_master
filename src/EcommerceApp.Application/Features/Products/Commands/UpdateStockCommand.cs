using EcommerceApp.Application.Common.Exceptions;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Products.Commands;

// ── Command ───────────────────────────────────────────────────────────────────

/// <summary>
/// Sets the physical stock quantity for a product.
/// Admin-only — called after a warehouse stock count or receiving goods.
/// NewQuantity must cover the currently reserved units (locked by pending Stripe sessions).
/// </summary>
public record UpdateStockCommand(
    Guid ProductId,
    int NewQuantity) : IRequest;

// ── Handler ───────────────────────────────────────────────────────────────────

public class UpdateStockCommandHandler : IRequestHandler<UpdateStockCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateStockCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        UpdateStockCommand command,
        CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(
            command.ProductId, cancellationToken)
            ?? throw new NotFoundException("Product", command.ProductId);

        // Prevent setting stock below the number of units currently reserved
        // by pending Stripe checkout sessions.
        if (command.NewQuantity < product.ReservedQuantity)
            throw new ValidationException(
                "NewQuantity",
                $"Stock cannot be set lower than the currently reserved " +
                $"quantity ({product.ReservedQuantity} units). " +
                $"Wait for pending checkouts to complete or expire.");

        product.StockQuantity = command.NewQuantity;

        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}