using EcommerceApp.Domain.Entities;
using EcommerceApp.Domain.Enums;
using EcommerceApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Infrastructure.Persistence.Repositories;

public class OtpStoreRepository
    : GenericRepository<OtpStore>, IOtpStoreRepository
{
    public OtpStoreRepository(AppDbContext context) : base(context) { }

    public async Task<OtpStore?> GetLatestUnusedAsync(
        string identifier,
        OtpPurpose purpose,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(o =>
                o.Identifier == identifier &&
                o.Purpose == purpose &&
                !o.IsUsed &&
                o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task InvalidateAllPreviousAsync(
        string identifier,
        OtpPurpose purpose,
        CancellationToken cancellationToken = default)
    {
        var existing = await _dbSet
            .Where(o =>
                o.Identifier == identifier &&
                o.Purpose == purpose &&
                !o.IsUsed)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var otp in existing)
        {
            otp.IsUsed = true;
            otp.UpdatedAt = now;
        }
    }
}