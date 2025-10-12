using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using ZiyoMarket.Data.Context;
using ZiyoMarket.Data.IRepositories;
using ZiyoMarket.Domain.Common;

namespace ZiyoMarket.Data.Repositories;

/// <summary>
/// Generic Repository implementation for all entities
/// </summary>
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    private readonly ZiyoMarketDbContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(ZiyoMarketDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public IQueryable<T> Table => _dbSet.AsNoTracking();

    // ===================== CREATE =====================

    public async Task<T> InsertAsync(T entity)
    {
        var entry = await _dbSet.AddAsync(entity);
        return entry.Entity;
    }

    public async Task<T> AddAsync(T entity)
    {
        return await InsertAsync(entity);
    }

    public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
        return entities;
    }

    // ===================== READ =====================

    public async Task<T?> GetByIdAsync(long id, string[]? includes = null)
    {
        IQueryable<T> query = _dbSet;

        if (includes != null && includes.Length > 0)
        {
            foreach (var include in includes)
                query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    public async Task<T?> SelectAsync(Expression<Func<T, bool>> expression, string[]? includes = null)
    {
        IQueryable<T> query = _dbSet;

        if (includes != null && includes.Length > 0)
        {
            foreach (var include in includes)
                query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync(expression);
    }

    public IQueryable<T> SelectAll(Expression<Func<T, bool>>? expression = null, string[]? includes = null)
    {
        IQueryable<T> query = _dbSet;

        if (includes != null && includes.Length > 0)
        {
            foreach (var include in includes)
                query = query.Include(include);
        }

        if (expression != null)
            query = query.Where(expression);

        return query;
    }

    public async Task<IList<T>> SelectAllAsync(Expression<Func<T, bool>>? expression = null, string[]? includes = null)
    {
        IQueryable<T> query = _dbSet;

        if (includes != null && includes.Length > 0)
        {
            foreach (var include in includes)
                query = query.Include(include);
        }

        if (expression != null)
            query = query.Where(expression);

        return await query.ToListAsync();
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> expression)
    {
        return await _dbSet.AnyAsync(expression);
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        if (predicate != null)
            return await _dbSet.CountAsync(predicate);
        return await _dbSet.CountAsync();
    }

    // ===================== PAGINATION =====================

    public async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
    {
        var query = _dbSet.AsQueryable();

        if (filter != null)
            query = query.Where(filter);

        var totalCount = await query.CountAsync();

        if (orderBy != null)
            query = orderBy(query);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    // ===================== UPDATE =====================

    public async Task<T> Update(T entity, long id)
    {
        var existing = await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        if (existing == null)
            throw new Exception($"{typeof(T).Name} with Id = {id} not found");

        entity.Id = (int)id; // Ensure ID is preserved
        _context.Entry(entity).State = EntityState.Modified;
        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        var existing = await _dbSet.FindAsync(entity.Id);
        if (existing == null)
            throw new Exception($"{typeof(T).Name} with Id = {entity.Id} not found");

        _context.Entry(existing).CurrentValues.SetValues(entity);
        return existing;
    }

    public async Task<T> UpdateByAsync(Expression<Func<T, bool>> predicate, T entity)
    {
        var existing = await _dbSet.FirstOrDefaultAsync(predicate);
        if (existing == null)
            throw new Exception($"{typeof(T).Name} not found by given predicate");

        entity.Id = existing.Id;
        _context.Entry(existing).CurrentValues.SetValues(entity);
        return existing;
    }

    public void UpdateRange(IEnumerable<T> entities)
    {
        _dbSet.UpdateRange(entities);
    }

    // ===================== DELETE =====================

    public async Task<bool> DeleteAsync(long id)
    {
        var entity = await _dbSet.FirstOrDefaultAsync(e => e.Id == id);
        if (entity == null)
            return false;

        _dbSet.Remove(entity);
        return true;
    }

    public void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    public void DeleteRange(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    // ===================== SOFT DELETE =====================

    public void SoftDelete(T entity)
    {
        entity.DeletedAt = DateTime.UtcNow.ToString();
        _context.Entry(entity).State = EntityState.Modified;
    }

    public void SoftDeleteRange(IEnumerable<T> entities)
    {
        foreach (var entity in entities)
        {
            entity.DeletedAt = DateTime.UtcNow.ToString();
            _context.Entry(entity).State = EntityState.Modified;
        }
    }

    // ===================== INCLUDE =====================

    public IQueryable<T> Include(params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;

        foreach (var include in includes)
            query = query.Include(include);

        return query;
    }

    // ===================== SAVE =====================

    public async Task<bool> SaveAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}