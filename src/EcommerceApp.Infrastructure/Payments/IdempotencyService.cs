using EcommerceApp.Application.Common.Interfaces;
using EcommerceApp.Application.Features.Payments.DTOs;
using EcommerceApp.Domain.Entities;
using EcommerceApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EcommerceApp.Infrastructure.Payments;

public class IdempotencyService : IIdempotencyService
{
    private readonly AppDbContext _dbContext;

    // How long a key stays valid — 24 hours covers any realistic retry window
    private static readonly TimeSpan KeyTtl = TimeSpan.FromHours(24);

    public IdempotencyService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CheckoutSessionResponseDto?> GetExistingResponseAsync(
        string idempotencyKey,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var record = await _dbContext.IdempotencyRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(
                r => r.IdempotencyKey == idempotencyKey &&
                     r.UserId == userId &&
                     r.ExpiresAt > DateTime.UtcNow,   // expired keys are ignored
                cancellationToken);

        if (record is null)
            return null;

        // Deserialize the cached response and return it as-is
        return JsonSerializer.Deserialize<CheckoutSessionResponseDto>(record.ResponseJson);
    }

    public async Task StoreResponseAsync(
        string idempotencyKey,
        Guid userId,
        string stripeSessionId,
        CheckoutSessionResponseDto response,
        CancellationToken cancellationToken = default)
    {
        var record = new IdempotencyRecord
        {
            Id = Guid.NewGuid(),
            IdempotencyKey = idempotencyKey,
            UserId = userId,
            StripeSessionId = stripeSessionId,
            ResponseJson = JsonSerializer.Serialize(response),
            ExpiresAt = DateTime.UtcNow.Add(KeyTtl)
        };

        // Use EF directly — this runs inside the transaction opened by
        // CheckoutTransactionExecutor, so it commits or rolls back atomically
        // with the order and stock reservation writes.
        await _dbContext.IdempotencyRecords.AddAsync(record, cancellationToken);
    }
}