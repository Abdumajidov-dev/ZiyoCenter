using Microsoft.EntityFrameworkCore;
using ZiyoMarket.Domain.Entities.Delivery;
using ZiyoMarket.Domain.Entities.Orders;
using ZiyoMarket.Domain.Entities.Products;
using ZiyoMarket.Domain.Entities.System;
using ZiyoMarket.Domain.Entities.Users;

namespace ZiyoMarket.Data.DbContexts;

public class ZiyoMarketDbContext : DbContext
{
    public ZiyoMarketDbContext(DbContextOptions<ZiyoMarketDbContext> options)
        : base(options) { }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Seller> Sellers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<CashbackTransaction> CashbackTransactions { get; set; }
    public DbSet<DeliveryPartner> DeliveryPartners { get; set; }
    public DbSet<SystemSetting> SystemSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
