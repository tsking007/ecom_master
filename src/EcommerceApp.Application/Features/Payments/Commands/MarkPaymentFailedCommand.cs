using EcommerceApp.Domain.Enums;
using EcommerceApp.Domain.Interfaces;
using EcommerceApp.Application.Common.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Payments.Commands;

public record MarkPaymentFailedCommand(
    Guid OrderId,
    string? PaymentIntentId = null,
    string? FailureReason = null) : IRequest;

public class MarkPaymentFailedCommandHandler
    : IRequestHandler<MarkPaymentFailedCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICheckoutTransactionExecutor _checkoutTransactionExecutor;

    public MarkPaymentFailedCommandHandler(
        IUnitOfWork unitOfWork,
        ICheckoutTransactionExecutor checkoutTransactionExecutor)
    {
        _unitOfWork = unitOfWork;
        _checkoutTransactionExecutor = checkoutTransactionExecutor;
    }

    public async Task Handle(
        MarkPaymentFailedCommand request,
        CancellationToken cancellationToken)
    {
        await _checkoutTransactionExecutor.ExecuteAsync(async ct =>
        {
            var order = await _unitOfWork.Orders.GetWithItemsAsync(
                request.OrderId,
                ct);

            if (order == null)
                return true;

            if (order.PaymentStatus == PaymentStatus.Paid)
                return true;

            if (string.IsNullOrWhiteSpace(order.StripeSessionId))
            {
                order.PaymentStatus = PaymentStatus.Failed;
                order.StripePaymentIntentId ??= request.PaymentIntentId;
                order.Notes = request.FailureReason;
                order.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Orders.UpdateAsync(order, ct);
                return true;
            }

            var reservations = await _unitOfWork.StockReservations.GetActiveBySessionIdAsync(
                order.StripeSessionId,
                ct);

            foreach (var reservation in reservations)
            {
                var product = reservation.Product;

                product.ReservedQuantity = Math.Max(0, product.ReservedQuantity - reservation.Quantity);

                reservation.IsReleased = true;
                reservation.ReleasedAt = DateTime.UtcNow;

                await _unitOfWork.Products.UpdateAsync(product, ct);
                await _unitOfWork.StockReservations.UpdateAsync(reservation, ct);
            }

            order.PaymentStatus = PaymentStatus.Failed;
            order.StripePaymentIntentId ??= request.PaymentIntentId;
            order.Notes = request.FailureReason;
            order.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Orders.UpdateAsync(order, ct);

            return true;
        }, cancellationToken);
    }
}