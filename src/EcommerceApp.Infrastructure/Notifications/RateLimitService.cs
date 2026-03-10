using EcommerceApp.Domain.Interfaces;

public class RateLimitService : IRateLimitService
{
    public Task<TimeSpan?> GetRemainingBlockDurationAsync(string identifier, string action, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsOtpSendAllowedAsync(string identifier, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public Task<bool> IsOtpVerifyAllowedAsync(string identifier, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public Task RecordOtpSendAsync(string identifier, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public Task RecordOtpVerifyFailAsync(string identifier, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public Task RecordOtpVerifySuccessAsync(string identifier, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }
}