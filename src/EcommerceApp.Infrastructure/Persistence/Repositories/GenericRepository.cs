using EcommerceApp.Domain.Common;
using EcommerceApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Infrastructure.Persistence.Repositories;

/// <summary>
/// Base repository implementation for all entities.
/// Global soft-delete query filter (set in each IEntityTypeConfiguration) means
/// all standard queries automatically exclude IsDeleted = true rows.
/// HardDelete bypasses the filter — use only for housekeeping jobs.
/// </summary>
public class GenericRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    // ── Single-entity reads ───────────────────────────────────────────────────

    public virtual async Task<T?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // FirstOrDefaultAsync respects the global query filter.
        // FindAsync does NOT — so we avoid it here.
        return await _dbSet
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    // ── Collection reads ──────────────────────────────────────────────────────

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    // ── Writes ────────────────────────────────────────────────────────────────

    public virtual async Task<T> AddAsync(
        T entity,
        CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public virtual Task UpdateAsync(
        T entity,
        CancellationToken cancellationToken = default)
    {
        _context.Entry(entity).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    //public virtual Task SoftDeleteAsync(
    //    T entity,
    //    CancellationToken cancellationToken = default)
    //{
    //    entity.IsDeleted = true;
    //    entity.UpdatedAt = DateTime.UtcNow;
    //    _context.Entry(entity).State = EntityState.Modified;
    //    return Task.CompletedTask;
    //}

    public virtual Task HardDeleteAsync(
        T entity,
        CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    // ── Existence / count ─────────────────────────────────────────────────────

    public virtual async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(e => e.Id == id, cancellationToken);
    }

    public virtual async Task<int> CountAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(cancellationToken);
    }

    // ── Raw query access ──────────────────────────────────────────────────────

    public virtual IQueryable<T> Query()
        => _dbSet.AsQueryable();
}