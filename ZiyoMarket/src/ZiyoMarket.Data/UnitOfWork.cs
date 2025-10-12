using System;
using System.Threading.Tasks;
using ZiyoMarket.Data.DbContexts;
using ZiyoMarket.Data.IRepositories;
using ZiyoMarket.Data.Services; // 👈 RepositoryService shu namespace ichida
using ZiyoMarket.Domain.Entities.Content;
using ZiyoMarket.Domain.Entities.Delivery;
using ZiyoMarket.Domain.Entities.Notifications;
using ZiyoMarket.Domain.Entities.Orders;
using ZiyoMarket.Domain.Entities.Products;
using ZiyoMarket.Domain.Entities.Support;
using ZiyoMarket.Domain.Entities.System;
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


        public UnitOfWork(ZiyoMarketDbContext context)
        {
            _context = context;

            // Users
            Customers = new RepositoryService<Customer>(_context);
            Sellers = new RepositoryService<Seller>(_context);
            Admins = new RepositoryService<Admin>(_context);

            // Products
            Products = new RepositoryService<Product>(_context);
            Categories = new RepositoryService<Category>(_context);
            CartItems = new RepositoryService<CartItem>(_context);
            ProductLikes = new RepositoryService<ProductLike>(_context);

            // Orders
            Orders = new RepositoryService<Order>(_context);
            OrderItems = new RepositoryService<OrderItem>(_context);
            OrderDiscounts = new RepositoryService<OrderDiscount>(_context);
            CashbackTransactions = new RepositoryService<CashbackTransaction>(_context);

            // Delivery
            DeliveryPartners = new RepositoryService<DeliveryPartner>(_context);
            OrderDeliveries = new RepositoryService<OrderDelivery>(_context);

            // Support
            SupportChats = new RepositoryService<SupportChat>(_context);
            SupportMessages = new RepositoryService<SupportMessage>(_context);

            // Notifications
            Notifications = new RepositoryService<Notification>(_context);

            // System
            SystemSettings = new RepositoryService<SystemSetting>(_context);
            DailySalesSummaries = new RepositoryService<DailySalesSummary>(_context);

            // Content
            Contents = new RepositoryService<Content>(_context);
        }

        public async Task<bool> SaveChangesAsync()
            => await _context.SaveChangesAsync() > 0;

        public void Dispose()
            => _context.Dispose();
    }
}
