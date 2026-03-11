using EcommerceApp.Domain.Enums;
using EcommerceApp.Domain.Interfaces;
using EcommerceApp.Application.Common.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Payments.Commands;

public record MarkCheckoutSessionCompletedCommand(
    string SessionId,
    string? PaymentIntentId = null) : IRequest;

public class MarkCheckoutSessionCompletedCommandHandler
    : IRequestHandler<MarkCheckoutSessionCompletedCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICheckoutTransactionExecutor _checkoutTransactionExecutor;

    public MarkCheckoutSessionCompletedCommandHandler(
        IUnitOfWork unitOfWork,
        ICheckoutTransactionExecutor checkoutTransactionExecutor)
    {
        _unitOfWork = unitOfWork;
        _checkoutTransactionExecutor = checkoutTransactionExecutor;
    }

    public async Task Handle(
        MarkCheckoutSessionCompletedCommand request,
        CancellationToken cancellationToken)
    {
        await _checkoutTransactionExecutor.ExecuteAsync(async ct =>
        {
            var order = await _unitOfWork.Orders.GetByStripeSessionIdAsync(
                request.SessionId,
                ct);

            if (order == null)
                return true;

            if (order.PaymentStatus == PaymentStatus.Paid)
                return true;

            var reservations = await _unitOfWork.StockReservations.GetActiveBySessionIdAsync(
                request.SessionId,
                ct);

            foreach (var reservation in reservations)
            {
                var product = reservation.Product;

                product.ReservedQuantity = Math.Max(0, product.ReservedQuantity - reservation.Quantity);
                product.StockQuantity = Math.Max(0, product.StockQuantity - reservation.Quantity);
                product.SoldCount += reservation.Quantity;

                reservation.IsReleased = true;
                reservation.ReleasedAt = DateTime.UtcNow;

                await _unitOfWork.Products.UpdateAsync(product, ct);
                await _unitOfWork.StockReservations.UpdateAsync(reservation, ct);
            }

            order.PaymentStatus = PaymentStatus.Paid;
            order.TrackingStatus = TrackingStatus.Placed;
            order.StripePaymentIntentId ??= request.PaymentIntentId;
            order.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Orders.UpdateAsync(order, ct);

            var cart = await _unitOfWork.Carts.GetByUserIdAsync(order.UserId, ct);
            if (cart != null)
            {
                var purchasedProductIds = order.Items
                    .Where(x => x.ProductId.HasValue)
                    .Select(x => x.ProductId!.Value)
                    .Distinct()
                    .ToList();

                await _unitOfWork.Carts.ClearPurchasedItemsAsync(
                    cart.Id,
                    purchasedProductIds,
                    ct);

                cart.LastActivityAt = DateTime.UtcNow;
                await _unitOfWork.Carts.UpdateAsync(cart, ct);
            }

            return true;
        }, cancellationToken);
    }
}