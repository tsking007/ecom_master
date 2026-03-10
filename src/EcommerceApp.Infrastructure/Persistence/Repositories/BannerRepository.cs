using EcommerceApp.Domain.Entities;
using EcommerceApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Infrastructure.Persistence.Repositories;

public class BannerRepository : IBannerRepository
{
    private readonly AppDbContext _context;

    public BannerRepository(AppDbContext context)
    {
        _context = context;
    }

    // ── Public reads ──────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<Banner>> GetActiveBannersAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await _context.Banners
            .Where(b =>
                !b.IsDeleted &&
                b.IsActive &&
                (b.StartDate == null || b.StartDate <= now) &&
                (b.EndDate == null || b.EndDate >= now))
            .OrderBy(b => b.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<Banner?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => await _context.Banners
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, cancellationToken);

    public async Task<IReadOnlyList<Banner>> GetAllAsync(
        CancellationToken cancellationToken = default)
        => await _context.Banners
            .Where(b => !b.IsDeleted)
            .OrderBy(b => b.DisplayOrder)
            .ToListAsync(cancellationToken);

    // ── Writes ────────────────────────────────────────────────────────────────

    public async Task AddAsync(
        Banner banner,
        CancellationToken cancellationToken = default)
        => await _context.Banners.AddAsync(banner, cancellationToken);

    public void Update(Banner banner)
        => _context.Banners.Update(banner);

    public void SoftDelete(Banner banner)
    {
        banner.IsDeleted = true;
        banner.UpdatedAt = DateTime.UtcNow;
        _context.Banners.Update(banner);
    }

    public async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}