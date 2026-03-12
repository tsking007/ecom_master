using EcommerceApp.Domain.Entities;
using EcommerceApp.Domain.Interfaces;
using EcommerceApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Infrastructure.Persistence.Repositories;

public class AddressRepository : GenericRepository<Address>, IAddressRepository
{
    public AddressRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Address>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // Default address is always first, then ordered by creation date
        return await _context.Addresses
            .Where(a => a.UserId == userId)          // query filter handles IsDeleted
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Address?> GetByIdAndUserIdAsync(
        Guid addressId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Addresses
            .FirstOrDefaultAsync(
                a => a.Id == addressId && a.UserId == userId,
                cancellationToken);
    }

    public async Task<Address?> GetDefaultAddressAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Addresses
            .FirstOrDefaultAsync(
                a => a.UserId == userId && a.IsDefault,
                cancellationToken);
    }

    public async Task<int> CountByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Addresses
            .CountAsync(a => a.UserId == userId, cancellationToken);
    }
}