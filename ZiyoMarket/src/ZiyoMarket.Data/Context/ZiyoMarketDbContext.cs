using System.Linq.Expressions;
using ZiyoMarket.Domain.Common;
using ZiyoMarket.Domain.Entities.Content;
using ZiyoMarket.Domain.Entities.Delivery;
using ZiyoMarket.Domain.Entities.Notifications;
using ZiyoMarket.Domain.Entities.Orders;
using ZiyoMarket.Domain.Entities.Products;
using ZiyoMarket.Domain.Entities.Support;
using ZiyoMarket.Domain.Entities.Systems;
using ZiyoMarket.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;


namespace ZiyoMarket.Data.Context;

public class ZiyoMarketDbContext : DbContext
{
    public ZiyoMarketDbContext(DbContextOptions<ZiyoMarketDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Seller> Sellers { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductLike> ProductLikes { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<OrderDiscount> OrderDiscounts { get; set; }
    public DbSet<DiscountReason> DiscountReasons { get; set; }
    public DbSet<CashbackTransaction> CashbackTransactions { get; set; }
    public DbSet<DeliveryPartner> DeliveryPartners { get; set; }
    public DbSet<OrderDelivery> OrderDeliveries { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<SupportChat> SupportChats { get; set; }
    public DbSet<SupportMessage> SupportMessages { get; set; }
    public DbSet<Content> Contents { get; set; }
    public DbSet<SystemSetting> SystemSettings { get; set; }
    public DbSet<DailySalesSummary> DailySalesSummaries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ZiyoMarketDbContext).Assembly);

        // Global query filters for soft delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType);
                var deletedAtProperty = Expression.Property(parameter, nameof(BaseEntity.DeletedAt));
                var condition = Expression.Equal(deletedAtProperty, Expression.Constant(null, typeof(string)));
                var lambda = Expression.Lambda(condition, parameter);

                entityType.SetQueryFilter(lambda);
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Auto set audit fields
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
