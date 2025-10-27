using System.Linq.Expressions;
using ZiyoMarket.Domain.Common;
using ZiyoMarket.Domain.Entities.Notifications;

namespace ZiyoMarket.Data.IRepositories;

public interface IRepository<T> where T : BaseEntity
{
    // Query operations
    Task<T?> GetByIdAsync(long id, string[]? includes = null);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    Task<bool> AnyAsync(Expression<Func<T, bool>> expression);

    // Select methods
    Task<T?> SelectAsync(Expression<Func<T, bool>> expression, string[]? includes = null);
    IQueryable<T> SelectAll(Expression<Func<T, bool>>? expression = null, string[]? includes = null);
    Task<IList<T>> SelectAllAsync(Expression<Func<T, bool>>? expression = null, string[]? includes = null);

    // Paging
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);

    // Command operations
    Task<T> InsertAsync(T entity);
    Task<T> AddAsync(T entity);
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);

    // Update operations
    Task<T> Update(T entity, long id);
    Task<T> UpdateAsync(T entity);
    Task<T> UpdateByAsync(Expression<Func<T, bool>> predicate, T entity);
    void UpdateRange(IEnumerable<T> entities);

    // Delete operations
    Task<bool> DeleteAsync(long id);
    void Delete(T entity);
    void DeleteRange(IEnumerable<T> entities);

    // Soft Delete
    void SoftDelete(T entity);
    void SoftDeleteRange(IEnumerable<T> entities);

    // Include for eager loading
    IQueryable<T> Include(params Expression<Func<T, object>>[] includes);

    // Table access
    IQueryable<T> Table { get; }

    // Save
    Task<bool> SaveAsync();
}