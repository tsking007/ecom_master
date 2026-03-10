using EcommerceApp.Domain.Entities;
using EcommerceApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Infrastructure.Persistence.Repositories;

public class RateLimitRepository
    : GenericRepository<RateLimitLog>, IRateLimitRepository
{
    public RateLimitRepository(AppDbContext context) : base(context) { }

    // ── Primary lookup ────────────────────────────────────────────────────────

    public async Task<RateLimitLog?> GetAsync(
        string identifier,
        string action,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(
                r => r.Identifier == identifier && r.Action == action,
                cancellationToken);
    }

    // ── Get-or-create ─────────────────────────────────────────────────────────

    public async Task<RateLimitLog> GetOrCreateAsync(
        string identifier,
        string action,
        CancellationToken cancellationToken = default)
    {
        var existing = await GetAsync(identifier, action, cancellationToken);
        if (existing != null) return existing;

        var newLog = new RateLimitLog
        {
            Id = Guid.NewGuid(),
            Identifier = identifier,
            Action = action,
            AttemptCount = 0,
            FailedAttemptCount = 0,
            WindowStartedAt = DateTime.UtcNow,
            LastAttemptAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _dbSet.AddAsync(newLog, cancellationToken);
        return newLog;
    }

    // ── Window reset ──────────────────────────────────────────────────────────

    public async Task ResetWindowAsync(
        string identifier,
        string action,
        CancellationToken cancellationToken = default)
    {
        var log = await GetAsync(identifier, action, cancellationToken);
        if (log == null) return;

        var now = DateTime.UtcNow;
        log.AttemptCount = 0;
        log.WindowStartedAt = now;
        log.UpdatedAt = now;

        _context.Entry(log).State = EntityState.Modified;
    }
}