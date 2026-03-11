namespace EcommerceApp.Application.Common.Exceptions;

/// <summary>
/// Thrown when a user/IP exceeds the allowed number of attempts
/// for a rate-limited action. Maps to HTTP 429 in ExceptionHandlingMiddleware.
/// </summary>
public class RateLimitExceededException : Exception
{
    public TimeSpan? RetryAfter { get; }

    public RateLimitExceededException(string message, TimeSpan? retryAfter = null)
        : base(message)
    {
        RetryAfter = retryAfter;
    }
}