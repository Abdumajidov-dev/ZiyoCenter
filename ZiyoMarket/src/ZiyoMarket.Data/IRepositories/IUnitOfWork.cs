using ZiyoMarket.Domain.Entities.Content;
using ZiyoMarket.Domain.Entities.Delivery;
using ZiyoMarket.Domain.Entities.Notifications;
using ZiyoMarket.Domain.Entities.Orders;
using ZiyoMarket.Domain.Entities.Products;
using ZiyoMarket.Domain.Entities.Support;
using ZiyoMarket.Domain.Entities.System;
using ZiyoMarket.Domain.Entities.Users;

namespace ZiyoMarket.Data.IRepositories;

public interface IUnitOfWork : IDisposable
{
    // Users
    IRepository<Customer> Customers { get; }
    IRepository<Seller> Sellers { get; }
    IRepository<Admin> Admins { get; }

    // Products
    IRepository<Product> Products { get; }
    IRepository<ProductLike> ProductLikes { get; }
    IRepository<Category> Categories { get; }
    IRepository<CartItem> CartItems { get; }

    // Orders
    IRepository<Order> Orders { get; }
    IRepository<OrderItem> OrderItems { get; }
    IRepository<OrderDiscount> OrderDiscounts { get; }
    IRepository<CashbackTransaction> CashbackTransactions { get; }

    // Delivery
    IRepository<DeliveryPartner> DeliveryPartners { get; }
    IRepository<OrderDelivery> OrderDeliveries { get; }

    // Support
    IRepository<SupportChat> SupportChats { get; }
    IRepository<SupportMessage> SupportMessages { get; }

    // Notifications
    IRepository<Notification> Notifications { get; }

    // System
    IRepository<SystemSetting> SystemSettings { get; }
    IRepository<DailySalesSummary> DailySalesSummaries { get; }

    // Content
    IRepository<Content> Contents { get; }

    // Commit & Transaction
    Task<bool> SaveChangesAsync();
}
