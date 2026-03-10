using EcommerceApp.Application.Common.Exceptions;
using EcommerceApp.Domain.Events;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Products.Commands;

// ── Command ───────────────────────────────────────────────────────────────────

/// <summary>
/// Soft-deletes a product — sets IsDeleted = true.
/// The EF global query filter then hides it from all future queries.
/// Products are never hard-deleted because historical orders reference them.
/// </summary>
public record DeleteProductCommand(Guid ProductId) : IRequest;

// ── Handler ───────────────────────────────────────────────────────────────────

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublisher _publisher;

    public DeleteProductCommandHandler(IUnitOfWork unitOfWork, IPublisher publisher)
    {
        _unitOfWork = unitOfWork;
        _publisher = publisher;
    }

    public async Task Handle(
        DeleteProductCommand command,
        CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(
            command.ProductId, cancellationToken)
            ?? throw new NotFoundException("Product", command.ProductId);

        // SoftDeleteAsync sets IsDeleted = true (Part 5 GenericRepository)
        await _unitOfWork.Products.SoftDeleteAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish event so search sync removes it from Elasticsearch (Part 15)
        await _publisher.Publish(new ProductDeletedNotification(
            ProductId: product.Id,
            Name: product.Name,
            Slug: product.Slug),
            cancellationToken);
    }
}