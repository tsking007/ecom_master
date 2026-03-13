using EcommerceApp.Domain.Common;
using EcommerceApp.Domain.Entities;
using EcommerceApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Infrastructure.Persistence.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    // ── Lookup by unique fields ───────────────────────────────────────────────

    public async Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(
                u => u.Email.ToLower() == email.ToLower(),
                cancellationToken);
    }

    public async Task<User?> GetByPhoneNumberAsync(
        string phoneNumber,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(
                u => u.PhoneNumber == phoneNumber,
                cancellationToken);
    }

    public async Task<User?> GetWithAddressesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    // ── Existence checks ──────────────────────────────────────────────────────

    public async Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(
                u => u.Email.ToLower() == email.ToLower(),
                cancellationToken);
    }

    public async Task<bool> PhoneExistsAsync(
        string phoneNumber,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(
                u => u.PhoneNumber == phoneNumber,
                cancellationToken);
    }

    // ── Admin paginated list ──────────────────────────────────────────────────

    public async Task<PagedResult<User>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        // Free-text filter on name or email
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower().Trim();
            query = query.Where(u =>
                u.Email.ToLower().Contains(term) ||
                u.FirstName.ToLower().Contains(term) ||
                u.LastName.ToLower().Contains(term));
        }

        if (isActive.HasValue)
            query = query.Where(u => u.IsActive == isActive.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<User>.Create(items, totalCount, pageNumber, pageSize);
    }
}