using Microsoft.EntityFrameworkCore.Storage;
using ZiyoMarket.Data.Context;
using ZiyoMarket.Data.IRepositories;
using ZiyoMarket.Data.Repositories;
using ZiyoMarket.Domain.Entities.Content;
using ZiyoMarket.Domain.Entities.Delivery;
using ZiyoMarket.Domain.Entities.Notifications;
using ZiyoMarket.Domain.Entities.Orders;
using ZiyoMarket.Domain.Entities.Products;
using ZiyoMarket.Domain.Entities.Support;
using ZiyoMarket.Domain.Entities.Systems;
using ZiyoMarket.Domain.Entities.Users;

namespace ZiyoMarket.Data.UnitOfWorks;

public class UnitOfWork : IUnitOfWork
{
    private readonly ZiyoMarketDbContext _context;
    private IDbContextTransaction? _transaction;

    // User Repositories
    public IRepository<Customer> Customers { get; private set; }
    public IRepository<Seller> Sellers { get; private set; }
    public IRepository<Admin> Admins { get; private set; }

    // Product Repositories
    public IRepository<Category> Categories { get; private set; }
    public IRepository<Product> Products { get; private set; }
    public IRepository<ProductLike> ProductLikes { get; private set; }
    public IRepository<CartItem> CartItems { get; private set; }

    // Order Repositories
    public IRepository<Order> Orders { get; private set; }
    public IRepository<OrderItem> OrderItems { get; private set; }
    public IRepository<OrderDiscount> OrderDiscounts { get; private set; }
    public IRepository<DiscountReason> DiscountReasons { get; private set; }
    public IRepository<CashbackTransaction> CashbackTransactions { get; private set; }

    // Delivery Repositories
    public IRepository<DeliveryPartner> DeliveryPartners { get; private set; }
    public IRepository<OrderDelivery> OrderDeliveries { get; private set; }

    // Notification Repositories
    public IRepository<Notification> Notifications { get; private set; }

    // Support Repositories
    public IRepository<SupportChat> SupportChats { get; private set; }
    public IRepository<SupportMessage> SupportMessages { get; private set; }

    // Content & System Repositories
    public IRepository<Content> Contents { get; private set; }
    public IRepository<SystemSetting> SystemSettings { get; private set; }
    public IRepository<DailySalesSummary> DailySalesSummaries { get; private set; }

    public UnitOfWork(ZiyoMarketDbContext context)
    {
        _context = context;

        // Initialize all repositories
        Customers = new Repository<Customer>(_context);
        Sellers = new Repository<Seller>(_context);
        Admins = new Repository<Admin>(_context);
        Categories = new Repository<Category>(_context);
        Products = new Repository<Product>(_context);
        ProductLikes = new Repository<ProductLike>(_context);
        CartItems = new Repository<CartItem>(_context);
        Orders = new Repository<Order>(_context);
        OrderItems = new Repository<OrderItem>(_context);
        OrderDiscounts = new Repository<OrderDiscount>(_context);
        DiscountReasons = new Repository<DiscountReason>(_context);
        CashbackTransactions = new Repository<CashbackTransaction>(_context);
        DeliveryPartners = new Repository<DeliveryPartner>(_context);
        OrderDeliveries = new Repository<OrderDelivery>(_context);
        Notifications = new Repository<Notification>(_context);
        SupportChats = new Repository<SupportChat>(_context);
        SupportMessages = new Repository<SupportMessage>(_context);
        Contents = new Repository<Content>(_context);
        SystemSettings = new Repository<SystemSetting>(_context);
        DailySalesSummaries = new Repository<DailySalesSummary>(_context);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context?.Dispose();
    }
}