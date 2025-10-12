using Microsoft.EntityFrameworkCore;
using ZiyoMarket.Domain.Entities.Delivery;

namespace ZiyoMarket.Data.Extensions
{
    public static class SeedDataExtensions
    {
        public static void SeedDefaultData(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DeliveryPartner>().HasData(
                new DeliveryPartner { Id = 1, Name = "O'zbekiston Pochtasi", PricePerDelivery = 15000, EstimatedDays = 2 },
                new DeliveryPartner { Id = 2, Name = "Express Pochta", PricePerDelivery = 25000, EstimatedDays = 1 },
                new DeliveryPartner { Id = 3, Name = "Tez Kuryer", PricePerDelivery = 20000, EstimatedDays = 1 }
            );
        }
    }
}
