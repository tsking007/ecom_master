using EcommerceApp.Domain.Entities;
using EcommerceApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Infrastructure.Persistence.Repositories;

public class TokenStoreRepository
    : GenericRepository<TokenStore>, ITokenStoreRepository
{
    public TokenStoreRepository(AppDbContext context) : base(context) { }

    public async Task<TokenStore?> GetByHashedTokenAsync(
        string hashedRefreshToken,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(t =>
                t.UserId == userId &&
                t.RefreshToken == hashedRefreshToken &&
                !t.IsRevoked &&
                t.ExpiresAt > DateTime.UtcNow,
                cancellationToken);
    }

    public async Task<IReadOnlyList<TokenStore>> GetActiveTokensByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t =>
                t.UserId == userId &&
                !t.IsRevoked &&
                t.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task RevokeAllByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var tokens = await _dbSet
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var token in tokens)
        {
            token.IsRevoked = true;
            token.UpdatedAt = now;
        }
    }
}