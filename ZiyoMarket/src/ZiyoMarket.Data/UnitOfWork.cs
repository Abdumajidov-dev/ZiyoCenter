using System;
using System.Threading.Tasks;
using ZiyoMarket.Data.Context;
using ZiyoMarket.Data.Repositories;
using ZiyoMarket.Data.IRepositories;
using ZiyoMarket.Domain.Entities.Content;
using ZiyoMarket.Domain.Entities.Delivery;
using ZiyoMarket.Domain.Entities.Notifications;
using ZiyoMarket.Domain.Entities.Orders;
using ZiyoMarket.Domain.Entities.Products;
using ZiyoMarket.Domain.Entities.Support;
using ZiyoMarket.Domain.Entities.Systems;
using ZiyoMarket.Domain.Entities.Users;

namespace ZiyoMarket.Data
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ZiyoMarketDbContext _context;

        // Users
        public IRepository<Customer> Customers { get; private set; }
        public IRepository<Seller> Sellers { get; private set; }
        public IRepository<Admin> Admins { get; private set; }

        // Products
        public IRepository<Product> Products { get; private set; }
        public IRepository<Category> Categories { get; private set; }
        public IRepository<CartItem> CartItems { get; private set; }
        public IRepository<ProductLike> ProductLikes { get; private set; }

        // Orders
        public IRepository<Order> Orders { get; private set; }
        public IRepository<OrderItem> OrderItems { get; private set; }
        public IRepository<OrderDiscount> OrderDiscounts { get; private set; }
        public IRepository<CashbackTransaction> CashbackTransactions { get; private set; }

        // Delivery
        public IRepository<DeliveryPartner> DeliveryPartners { get; private set; }
        public IRepository<OrderDelivery> OrderDeliveries { get; private set; }

        // Support
        public IRepository<SupportChat> SupportChats { get; private set; }
        public IRepository<SupportMessage> SupportMessages { get; private set; }

        // Notifications
        public IRepository<Notification> Notifications { get; private set; }

        // System
        public IRepository<SystemSetting> SystemSettings { get; private set; }
        public IRepository<DailySalesSummary> DailySalesSummaries { get; private set; }

        // Content
        public IRepository<Content> Contents { get; private set; }

        public IRepository<DiscountReason> DiscountReasons => throw new NotImplementedException();

        public UnitOfWork(ZiyoMarketDbContext context)
        {
            _context = context;

            // Users
            Customers = new Repository<Customer>(_context);
            Sellers = new Repository<Seller>(_context);
            Admins = new Repository<Admin>(_context);

            // Products
            Products = new Repository<Product>(_context);
            Categories = new Repository<Category>(_context);
            CartItems = new Repository<CartItem>(_context);
            ProductLikes = new Repository<ProductLike>(_context);

            // Orders
            Orders = new Repository<Order>(_context);
            OrderItems = new Repository<OrderItem>(_context);
            OrderDiscounts = new Repository<OrderDiscount>(_context);
            CashbackTransactions = new Repository<CashbackTransaction>(_context);

            // Delivery
            DeliveryPartners = new Repository<DeliveryPartner>(_context);
            OrderDeliveries = new Repository<OrderDelivery>(_context);

            // Support
            SupportChats = new Repository<SupportChat>(_context);
            SupportMessages = new Repository<SupportMessage>(_context);

            // Notifications
            Notifications = new Repository<Notification>(_context);

            // System
            SystemSettings = new Repository<SystemSetting>(_context);
            DailySalesSummaries = new Repository<DailySalesSummary>(_context);

            // Content
            Contents = new Repository<Content>(_context);
        }

        public async Task<bool> SaveChangesAsync()
            => await _context.SaveChangesAsync() > 0;

        public void Dispose()
            => _context.Dispose();

        Task<int> IUnitOfWork.SaveChangesAsync()
        {
            throw new NotImplementedException();
        }

        public Task BeginTransactionAsync()
        {
            throw new NotImplementedException();
        }

        public Task CommitTransactionAsync()
        {
            throw new NotImplementedException();
        }

        public Task RollbackTransactionAsync()
        {
            throw new NotImplementedException();
        }
    }
}
