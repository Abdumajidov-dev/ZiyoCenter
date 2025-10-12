using System.Linq.Expressions;
using ZiyoMarket.Domain.Entities.Content;
using ZiyoMarket.Domain.Entities.Delivery;
using ZiyoMarket.Domain.Entities.Notifications;
using ZiyoMarket.Domain.Entities.Orders;
using ZiyoMarket.Domain.Entities.Products;
using ZiyoMarket.Domain.Entities.Support;
using ZiyoMarket.Domain.Entities.Systems;
using ZiyoMarket.Domain.Entities.Users;
using ZiyoMarket.Domain.Common;

namespace ZiyoMarket.Data.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    // Query
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    // Paging
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);

    // Command
    Task<T> AddAsync(T entity);
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void UpdateRange(IEnumerable<T> entities);
    void Delete(T entity);
    void DeleteRange(IEnumerable<T> entities);

    // Soft Delete
    void SoftDelete(T entity);
    void SoftDeleteRange(IEnumerable<T> entities);

    // Include
    IQueryable<T> Include(params Expression<Func<T, object>>[] includes);
}
public interface IUnitOfWork : IDisposable
{
    // Repositories
    IRepository<Customer> Customers { get; }
    IRepository<Seller> Sellers { get; }
    IRepository<Admin> Admins { get; }
    IRepository<Category> Categories { get; }
    IRepository<Product> Products { get; }
    IRepository<ProductLike> ProductLikes { get; }
    IRepository<CartItem> CartItems { get; }
    IRepository<Order> Orders { get; }
    IRepository<OrderItem> OrderItems { get; }
    IRepository<OrderDiscount> OrderDiscounts { get; }
    IRepository<DiscountReason> DiscountReasons { get; }
    IRepository<CashbackTransaction> CashbackTransactions { get; }
    IRepository<DeliveryPartner> DeliveryPartners { get; }
    IRepository<OrderDelivery> OrderDeliveries { get; }
    IRepository<Notification> Notifications { get; }
    IRepository<SupportChat> SupportChats { get; }
    IRepository<SupportMessage> SupportMessages { get; }
    IRepository<Content> Contents { get; }
    IRepository<SystemSetting> SystemSettings { get; }
    IRepository<DailySalesSummary> DailySalesSummaries { get; }

    // Transaction
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}