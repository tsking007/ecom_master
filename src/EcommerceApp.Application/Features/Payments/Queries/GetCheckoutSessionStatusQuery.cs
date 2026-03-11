using EcommerceApp.Application.Common;
using EcommerceApp.Application.Common.Exceptions;
using EcommerceApp.Application.Common.Interfaces;
using EcommerceApp.Application.Features.Payments.DTOs;
using EcommerceApp.Domain.Enums;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Payments.Queries;

public record GetCheckoutSessionStatusQuery(string SessionId)
    : IRequest<CheckoutSessionStatusDto>;

public class GetCheckoutSessionStatusQueryHandler
    : IRequestHandler<GetCheckoutSessionStatusQuery, CheckoutSessionStatusDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStripePaymentService _stripePaymentService;

    public GetCheckoutSessionStatusQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IStripePaymentService stripePaymentService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _stripePaymentService = stripePaymentService;
    }

    public async Task<CheckoutSessionStatusDto> Handle(
        GetCheckoutSessionStatusQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var order = await _unitOfWork.Orders.GetByStripeSessionIdAsync(
            request.SessionId,
            cancellationToken)
            ?? throw new NotFoundException("Checkout session", request.SessionId);

        if (order.UserId != userId)
            throw new ForbiddenException("Order", order.Id);

        var stripeStatus = await _stripePaymentService.GetSessionStatusAsync(
            request.SessionId,
            cancellationToken);

        var normalizedStatus = order.PaymentStatus switch
        {
            PaymentStatus.Paid => "paid",
            PaymentStatus.Failed => "failed",
            PaymentStatus.Cancelled => "failed",
            _ => stripeStatus.PaymentStatus?.ToLowerInvariant() switch
            {
                "paid" => "paid",
                "unpaid" => "pending",
                "no_payment_required" => "paid",
                _ => "pending"
            }
        };

        return new CheckoutSessionStatusDto
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            SessionId = request.SessionId,
            Status = normalizedStatus,
            PaymentStatus = order.PaymentStatus.ToString(),
            TrackingStatus = order.TrackingStatus.ToString(),
            IsPaid = order.PaymentStatus == PaymentStatus.Paid,
            StripeSessionStatus = stripeStatus.SessionStatus,
            StripePaymentStatus = stripeStatus.PaymentStatus
        };
    }
}