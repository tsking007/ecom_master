namespace EcommerceApp.Domain.Interfaces;

/// <summary>
/// Database-backed rate limiter for OTP operations.
/// Enforces:
///   - Max 5 OTP sends per 15 minutes per identifier
///   - Block identifier for 30 minutes after 5 failed verifications
/// This is separate from the ASP.NET Core HTTP rate limiters (Part 22),
/// which handle IP-level limits for login and general API traffic.
/// </summary>
public interface IRateLimitService
{
    /// <summary>
    /// Returns true if the identifier is allowed to request a new OTP.
    /// Returns false if they have hit the 5/15min send limit.
    /// </summary>
    Task<bool> IsOtpSendAllowedAsync(
        string identifier,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if the identifier is allowed to attempt OTP verification.
    /// Returns false if they are currently blocked after too many failures.
    /// </summary>
    Task<bool> IsOtpVerifyAllowedAsync(
        string identifier,
        CancellationToken cancellationToken = default);

    /// <summary>Increments the OTP send counter for this identifier.</summary>
    Task RecordOtpSendAsync(
        string identifier,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Increments the failed verification counter.
    /// If count reaches 5, sets BlockedUntil = UtcNow + 30 min.
    /// </summary>
    Task RecordOtpVerifyFailAsync(
        string identifier,
        CancellationToken cancellationToken = default);

    /// <summary>Resets the failed verification counter on successful verify.</summary>
    Task RecordOtpVerifySuccessAsync(
        string identifier,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the time remaining in the current block.
    /// Returns null if the identifier is not blocked.
    /// Used to include Retry-After in the 429 response.
    /// </summary>
    Task<TimeSpan?> GetRemainingBlockDurationAsync(
        string identifier,
        string action,
        CancellationToken cancellationToken = default);
}