namespace EcommerceApp.Domain.Interfaces;

public interface IPaymentService
{
    /// <summary>
    /// Creates a Stripe Checkout Session for the given order.
    /// Returns the Stripe session URL to redirect the customer to.
    /// </summary>
    Task<string> CreateCheckoutSessionAsync(
        CreateCheckoutSessionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the current status of a Stripe Checkout Session.
    /// Used on the return page to confirm payment result.
    /// </summary>
    Task<PaymentSessionResult> GetSessionStatusAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies that the Stripe-Signature header matches the webhook secret.
    /// Always call this before processing any webhook payload.
    /// </summary>
    bool ValidateWebhookSignature(string payload, string signature);

    /// <summary>
    /// Parses and routes the Stripe webhook event to the correct handler:
    ///   checkout.session.completed     → mark order Paid, fulfill
    ///   payment_intent.payment_failed  → mark order PaymentFailed, release stock
    /// </summary>
    Task ProcessWebhookAsync(
        string payload,
        string signature,
        CancellationToken cancellationToken = default);
}

// ── Supporting value objects ───────────────────────────────────────────────────

/// <summary>Full request needed to build a Stripe Checkout Session.</summary>
public record CreateCheckoutSessionRequest(
    Guid UserId,
    Guid OrderId,
    string OrderNumber,
    IReadOnlyList<CheckoutLineItem> LineItems,
    string CustomerEmail,
    string SuccessUrl,
    string CancelUrl
);

/// <summary>One product line in the Stripe Checkout Session.</summary>
public record CheckoutLineItem(
    string ProductName,
    string? Description,
    string? ImageUrl,
    decimal UnitAmount,
    int Quantity
);

/// <summary>Returned by GetSessionStatusAsync after the customer returns from Stripe.</summary>
public record PaymentSessionResult(
    string SessionId,
    string? PaymentIntentId,
    string PaymentStatus,
    bool IsPaid
);