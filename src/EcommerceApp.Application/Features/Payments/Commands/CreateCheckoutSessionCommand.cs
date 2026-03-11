using EcommerceApp.Application.Common;
using EcommerceApp.Application.Common.Exceptions;
using EcommerceApp.Application.Common.Interfaces;
using EcommerceApp.Application.Features.Payments.DTOs;
using EcommerceApp.Domain.Entities;
using EcommerceApp.Domain.Enums;
using EcommerceApp.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace EcommerceApp.Application.Features.Payments.Commands;

public record CreateCheckoutSessionCommand(Guid? AddressId = null)
    : IRequest<CheckoutSessionResponseDto>;

public class CreateCheckoutSessionCommandHandler
    : IRequestHandler<CreateCheckoutSessionCommand, CheckoutSessionResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStripePaymentService _stripePaymentService;
    private readonly StripeSettings _stripeSettings;
    private readonly ICheckoutTransactionExecutor _checkoutTransactionExecutor;

    public CreateCheckoutSessionCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IStripePaymentService stripePaymentService,
        IOptions<StripeSettings> stripeOptions,
        ICheckoutTransactionExecutor checkoutTransactionExecutor)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _stripePaymentService = stripePaymentService;
        _stripeSettings = stripeOptions.Value;
        _checkoutTransactionExecutor = checkoutTransactionExecutor;
    }

    public async Task<CheckoutSessionResponseDto> Handle(
        CreateCheckoutSessionCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var user = await _unitOfWork.Users.GetWithAddressesAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User", userId);

        var cart = await _unitOfWork.Carts.GetWithItemsAndProductsAsync(userId, cancellationToken)
            ?? throw new ValidationException("Cart is empty.");

        var activeItems = cart.Items
            .Where(i => !i.IsDeleted)
            .ToList();

        if (activeItems.Count == 0)
            throw new ValidationException("Cart is empty.");

        foreach (var item in activeItems)
        {
            if (item.Product == null)
                throw new NotFoundException("Product", item.ProductId);

            if (!item.Product.IsActive)
                throw new ValidationException(
                    $"Product '{item.Product.Name}' is inactive and cannot be checked out.");

            if (item.Quantity > item.Product.AvailableStock)
            {
                throw new ValidationException(
                    $"Insufficient stock for '{item.Product.Name}'. Requested {item.Quantity}, available {item.Product.AvailableStock}.");
            }
        }

        var address = request.AddressId.HasValue
            ? user.Addresses.FirstOrDefault(a => a.Id == request.AddressId.Value && !a.IsDeleted)
            : user.Addresses.FirstOrDefault(a => a.IsDefault && !a.IsDeleted);

        if (address == null)
            throw new ValidationException("A valid shipping address is required for checkout.");

        var subTotal = activeItems.Sum(x => x.Product.EffectivePrice * x.Quantity);
        var totalAmount = subTotal;

        var orderNumber = await _unitOfWork.Orders.GenerateOrderNumberAsync(cancellationToken);

        var addressSnapshot = JsonSerializer.Serialize(new
        {
            address.Id,
            address.FullName,
            address.PhoneNumber,
            address.AddressLine1,
            address.AddressLine2,
            address.City,
            address.State,
            address.PostalCode,
            address.Country,
            address.AddressType
        });

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OrderNumber = orderNumber,
            SubTotal = subTotal,
            DiscountAmount = 0,
            ShippingAmount = 0,
            TaxAmount = 0,
            TotalAmount = totalAmount,
            PaymentStatus = PaymentStatus.Pending,
            TrackingStatus = TrackingStatus.Placed,
            ShippingAddressSnapshot = addressSnapshot,
            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(7),
            Items = activeItems.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.Product.Name,
                ProductImageUrl = item.Product.ImageUrls.FirstOrDefault(),
                ProductSlug = item.Product.Slug,
                UnitPrice = item.Product.Price,
                DiscountedUnitPrice = item.Product.DiscountedPrice,
                Quantity = item.Quantity,
                TotalPrice = item.Product.EffectivePrice * item.Quantity
            }).ToList()
        };

        var stripeRequest = new StripeCheckoutSessionRequest(
            CustomerEmail: user.Email,
            Currency: _stripeSettings.Currency,
            SuccessUrl: _stripeSettings.SuccessUrl,
            CancelUrl: _stripeSettings.CancelUrl,
            Items: activeItems.Select(item => new StripeCheckoutLineItem(
                Name: item.Product.Name,
                Description: item.Product.ShortDescription,
                ImageUrl: item.Product.ImageUrls.FirstOrDefault(),
                UnitAmount: ToStripeAmount(item.Product.EffectivePrice),
                Quantity: item.Quantity
            )).ToList(),
            Metadata: new Dictionary<string, string>
            {
                ["orderId"] = order.Id.ToString(),
                ["orderNumber"] = order.OrderNumber,
                ["userId"] = userId.ToString()
            });

        return await _checkoutTransactionExecutor.ExecuteAsync(async ct =>
        {
            var stripeSession = await _stripePaymentService.CreateCheckoutSessionAsync(
                stripeRequest,
                ct);

            order.StripeSessionId = stripeSession.SessionId;
            order.StripePaymentIntentId = stripeSession.PaymentIntentId;

            await _unitOfWork.Orders.AddAsync(order, ct);

            foreach (var item in activeItems)
            {
                item.Product.ReservedQuantity += item.Quantity;
                await _unitOfWork.Products.UpdateAsync(item.Product, ct);

                var reservation = new StockReservation
                {
                    ProductId = item.ProductId,
                    UserId = userId,
                    Quantity = item.Quantity,
                    StripeSessionId = stripeSession.SessionId,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                    IsReleased = false
                };

                await _unitOfWork.StockReservations.AddAsync(reservation, ct);
            }

            cart.LastActivityAt = DateTime.UtcNow;
            await _unitOfWork.Carts.UpdateAsync(cart, ct);

            return new CheckoutSessionResponseDto
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                SessionId = stripeSession.SessionId,
                SessionUrl = stripeSession.SessionUrl
            };
        }, cancellationToken);
    }

    private static long ToStripeAmount(decimal amount)
        => (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);
}