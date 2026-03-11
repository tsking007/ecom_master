using EcommerceApp.Application.Common.Exceptions;
using EcommerceApp.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EcommerceApp.Infrastructure.Notifications;

public class RateLimitService : IRateLimitService
{
    private const int OtpSendMaxAttempts = 5;
    private const int OtpVerifyMaxFailures = 5;

    private static readonly TimeSpan OtpSendWindow = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan OtpVerifyBlockWindow = TimeSpan.FromMinutes(30);

    private const string OtpSendAction = "OTP_SEND";
    private const string OtpVerifyAction = "OTP_VERIFY";

    private readonly IRateLimitRepository _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RateLimitService> _logger;

    public RateLimitService(
        IRateLimitRepository repo,
        IUnitOfWork unitOfWork,
        ILogger<RateLimitService> logger)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    // ── OTP Send ──────────────────────────────────────────────────────────────

    public async Task<bool> IsOtpSendAllowedAsync(
        string identifier,
        CancellationToken cancellationToken = default)
    {
        // Only fetch — do NOT create here.
        // If no row exists yet, the identifier has made zero attempts → allowed.
        var log = await _repo.GetAsync(identifier, OtpSendAction, cancellationToken);

        if (log is null)
            return true;

        // If the 15-min window has expired, treat as fresh start → allowed
        if (IsWindowExpired(log.WindowStartedAt, OtpSendWindow))
            return true;

        return log.AttemptCount < OtpSendMaxAttempts;
    }

    public async Task RecordOtpSendAsync(
        string identifier,
        CancellationToken cancellationToken = default)
    {
        var log = await _repo.GetAsync(identifier, OtpSendAction, cancellationToken);
        var now = DateTime.UtcNow;

        if (log is null)
        {
            // First ever attempt — insert a fresh row
            await _repo.GetOrCreateAsync(identifier, OtpSendAction, cancellationToken);

            // Save the new row first so the unique index row exists in DB
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Re-fetch the now-persisted row and set count to 1
            log = await _repo.GetAsync(identifier, OtpSendAction, cancellationToken);
            log!.AttemptCount = 1;
            log.LastAttemptAt = now;
            log.WindowStartedAt = now;
            log.UpdatedAt = now;
        }
        else if (IsWindowExpired(log.WindowStartedAt, OtpSendWindow))
        {
            // Window expired — reset and start a new window
            log.AttemptCount = 1;
            log.WindowStartedAt = now;
            log.LastAttemptAt = now;
            log.UpdatedAt = now;
        }
        else
        {
            // Within the same window — just increment
            log.AttemptCount++;
            log.LastAttemptAt = now;
            log.UpdatedAt = now;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "OTP send recorded for {Identifier}. Count: {Count}/{Max}.",
            identifier, log.AttemptCount, OtpSendMaxAttempts);
    }

    // ── OTP Verify ────────────────────────────────────────────────────────────

    public async Task<bool> IsOtpVerifyAllowedAsync(
        string identifier,
        CancellationToken cancellationToken = default)
    {
        // Only fetch — no create here either
        var log = await _repo.GetAsync(identifier, OtpVerifyAction, cancellationToken);

        if (log is null)
            return true;

        if (log.IsCurrentlyBlocked)
        {
            _logger.LogWarning(
                "OTP verify blocked for {Identifier} until {BlockedUntil}.",
                identifier, log.BlockedUntil);
            return false;
        }

        return true;
    }

    public async Task RecordOtpVerifyFailAsync(
        string identifier,
        CancellationToken cancellationToken = default)
    {
        var log = await _repo.GetAsync(identifier, OtpVerifyAction, cancellationToken);
        var now = DateTime.UtcNow;

        if (log is null)
        {
            // First failure — insert row, save it, then re-fetch to update
            await _repo.GetOrCreateAsync(identifier, OtpVerifyAction, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            log = await _repo.GetAsync(identifier, OtpVerifyAction, cancellationToken);
            log!.FailedAttemptCount = 1;
            log.LastAttemptAt = now;
            log.UpdatedAt = now;
        }
        else
        {
            log.FailedAttemptCount++;
            log.LastAttemptAt = now;
            log.UpdatedAt = now;
        }

        if (log.FailedAttemptCount >= OtpVerifyMaxFailures)
        {
            log.BlockedUntil = now.Add(OtpVerifyBlockWindow);

            _logger.LogWarning(
                "OTP verify blocked for {Identifier} after {Failures} failures. " +
                "Blocked until {BlockedUntil}.",
                identifier, log.FailedAttemptCount, log.BlockedUntil);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RecordOtpVerifySuccessAsync(
        string identifier,
        CancellationToken cancellationToken = default)
    {
        var log = await _repo.GetAsync(identifier, OtpVerifyAction, cancellationToken);

        // No row means no failures were ever recorded — nothing to reset
        if (log is null)
            return;

        var now = DateTime.UtcNow;
        log.FailedAttemptCount = 0;
        log.BlockedUntil = null;
        log.LastAttemptAt = now;
        log.UpdatedAt = now;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "OTP verify success for {Identifier}. Block cleared.", identifier);
    }

    // ── Remaining block duration ──────────────────────────────────────────────

    public async Task<TimeSpan?> GetRemainingBlockDurationAsync(
        string identifier,
        string action,
        CancellationToken cancellationToken = default)
    {
        var log = await _repo.GetAsync(identifier, action, cancellationToken);

        if (log is null || !log.IsCurrentlyBlocked)
            return null;

        return log.BlockedUntil!.Value - DateTime.UtcNow;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static bool IsWindowExpired(DateTime windowStartedAt, TimeSpan window)
        => DateTime.UtcNow - windowStartedAt > window;
}