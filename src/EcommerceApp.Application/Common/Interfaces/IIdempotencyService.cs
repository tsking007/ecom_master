using EcommerceApp.Application.Features.Payments.DTOs;

namespace EcommerceApp.Application.Common.Interfaces;

public interface IIdempotencyService
{
    /// <summary>
    /// Returns a cached CheckoutSessionResponseDto if this key was already
    /// processed successfully for this user. Returns null if it's a new key.
    /// </summary>
    Task<CheckoutSessionResponseDto?> GetExistingResponseAsync(
        string idempotencyKey,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists the key + response after a successful checkout session creation.
    /// Must be called inside the same transaction as the order creation.
    /// </summary>
    Task StoreResponseAsync(
        string idempotencyKey,
        Guid userId,
        string stripeSessionId,
        CheckoutSessionResponseDto response,
        CancellationToken cancellationToken = default);
}