using EcommerceApp.Application.Common;
using EcommerceApp.Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using Stripe.Checkout;

namespace EcommerceApp.Infrastructure.Payments;

public class StripePaymentService : IStripePaymentService
{
    private readonly StripeSettings _settings;

    public StripePaymentService(IOptions<StripeSettings> options)
    {
        _settings = options.Value;
    }

    public async Task<StripeCheckoutSessionResult> CreateCheckoutSessionAsync(
        StripeCheckoutSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var service = new SessionService();

        var options = new SessionCreateOptions
        {
            Mode = "payment",
            SuccessUrl = request.SuccessUrl,
            CancelUrl = request.CancelUrl,
            CustomerEmail = request.CustomerEmail,
            Currency = request.Currency.ToLowerInvariant(),
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = request.Items.Select(item => new SessionLineItemOptions
            {
                Quantity = item.Quantity,
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = request.Currency.ToLowerInvariant(),
                    UnitAmount = item.UnitAmount,
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.Name,
                        Description = item.Description,
                        Images = string.IsNullOrWhiteSpace(item.ImageUrl)
                            ? null
                            : new List<string> { item.ImageUrl }
                    }
                }
            }).ToList(),
            Metadata = request.Metadata,
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                Metadata = request.Metadata
            }
        };

        var session = await service.CreateAsync(
            options,
            requestOptions: null,
            cancellationToken: cancellationToken);

        return new StripeCheckoutSessionResult(
            SessionId: session.Id,
            SessionUrl: session.Url!,
            PaymentIntentId: session.PaymentIntentId);
    }

    public async Task<StripeCheckoutSessionStatusResult> GetSessionStatusAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        var service = new SessionService();

        var session = await service.GetAsync(
            sessionId,
            requestOptions: null,
            cancellationToken: cancellationToken);

        return new StripeCheckoutSessionStatusResult(
            SessionId: session.Id,
            SessionStatus: session.Status,
            PaymentStatus: session.PaymentStatus,
            PaymentIntentId: session.PaymentIntentId);
    }
}