using ZiyoMarket.Data.IRepositories;
using ZiyoMarket.Domain.Entities.Content;
using ZiyoMarket.Domain.Entities.Delivery;
using ZiyoMarket.Domain.Entities.Notifications;
using ZiyoMarket.Domain.Entities.Orders;
using ZiyoMarket.Domain.Entities.Products;
using ZiyoMarket.Domain.Entities.Support;
using ZiyoMarket.Domain.Entities.Systems;
using ZiyoMarket.Domain.Entities.Users;

namespace ZiyoMarket.Data.UnitOfWorks;

public interface IUnitOfWork : IDisposable
{
    // User Repositories
    IRepository<Customer> Customers { get; }
    IRepository<Seller> Sellers { get; }
    IRepository<Admin> Admins { get; }

    // Product Repositories
    IRepository<Category> Categories { get; }
    IRepository<Product> Products { get; }
    IRepository<ProductLike> ProductLikes { get; }
    IRepository<CartItem> CartItems { get; }

    // Order Repositories
    IRepository<Order> Orders { get; }
    IRepository<OrderItem> OrderItems { get; }
    IRepository<OrderDiscount> OrderDiscounts { get; }
    IRepository<DiscountReason> DiscountReasons { get; }
    IRepository<CashbackTransaction> CashbackTransactions { get; }

    // Delivery Repositories
    IRepository<DeliveryPartner> DeliveryPartners { get; }
    IRepository<OrderDelivery> OrderDeliveries { get; }

    // Notification Repositories
    IRepository<Notification> Notifications { get; }

    // Support Repositories
    IRepository<SupportChat> SupportChats { get; }
    IRepository<SupportMessage> SupportMessages { get; }

    // Content & System Repositories
    IRepository<Content> Contents { get; }
    IRepository<SystemSetting> SystemSettings { get; }
    IRepository<DailySalesSummary> DailySalesSummaries { get; }

    // Transaction Management
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
