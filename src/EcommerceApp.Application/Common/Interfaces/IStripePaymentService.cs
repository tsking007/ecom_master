namespace EcommerceApp.Application.Common.Interfaces;

public interface IStripePaymentService
{
    Task<StripeCheckoutSessionResult> CreateCheckoutSessionAsync(
        StripeCheckoutSessionRequest request,
        CancellationToken cancellationToken = default);

    Task<StripeCheckoutSessionStatusResult> GetSessionStatusAsync(
        string sessionId,
        CancellationToken cancellationToken = default);
}

public record StripeCheckoutSessionRequest(
    string CustomerEmail,
    string Currency,
    string SuccessUrl,
    string CancelUrl,
    IReadOnlyList<StripeCheckoutLineItem> Items,
    Dictionary<string, string>? Metadata = null);

public record StripeCheckoutLineItem(
    string Name,
    string? Description,
    string? ImageUrl,
    long UnitAmount,
    int Quantity);

public record StripeCheckoutSessionResult(
    string SessionId,
    string SessionUrl,
    string? PaymentIntentId);

public record StripeCheckoutSessionStatusResult(
    string SessionId,
    string? SessionStatus,
    string? PaymentStatus,
    string? PaymentIntentId);