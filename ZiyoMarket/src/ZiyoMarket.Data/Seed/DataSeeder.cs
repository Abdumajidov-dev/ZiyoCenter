using ZiyoMarket.Data.Context;
using ZiyoMarket.Domain.Entities.Delivery;
using ZiyoMarket.Domain.Entities.Orders;
using ZiyoMarket.Domain.Entities.Products;
using ZiyoMarket.Domain.Entities.Systems;
using ZiyoMarket.Domain.Entities.Users;
using ZiyoMarket.Domain.Enums;


namespace ZiyoMarket.Data.Seed;
public static class DataSeeder
{
    public static async Task SeedAsync(ZiyoMarketDbContext context)
    {
        // System Settings
        if (!context.SystemSettings.Any())
        {
            var settings = new List<SystemSetting>
                {
                    new() { SettingKey  = "CashbackPercentage", SettingValue = "2", DataType = "Decimal", Category = "Cashback", Description = "Cashback percentage per purchase" },
                    new() { SettingKey = "CashbackExpiryDays", SettingValue = "30", DataType = "Integer", Category = "Cashback", Description = "Cashback expiry days" },
                    new() { SettingKey = "MinOrderAmount", SettingValue = "10000", DataType = "Decimal", Category = "Order", Description = "Minimum order amount" },
                    new() { SettingKey = "FreeDeliveryAmount", SettingValue = "100000", DataType = "Decimal", Category = "Delivery", Description = "Free delivery minimum amount" },
                    new() { SettingKey = "MaxDiscountPercent", SettingValue = "20", DataType = "Integer", Category = "Discount", Description = "Maximum discount for regular sellers" }
                };

            await context.SystemSettings.AddRangeAsync(settings);
        }

        // Categories
        if (!context.Categories.Any())
        {
            var categories = new List<Category>
                {
                    new() { Name = "Kitoblar", Description = "Barcha turdagi kitoblar", DisplayOrder = 1 },
                    new() { Name = "Diniy Kitoblar", Description = "Diniy adabiyotlar", DisplayOrder = 2 },
                    new() { Name = "Diniy Liboslar", Description = "Erkaklar va ayollar liboslari", DisplayOrder = 3 },
                    new() { Name = "Sovg'alar", Description = "Turli sovg'alar va aksessuarlar", DisplayOrder = 4 },
                    new() { Name = "Ofis Buyumlari", Description = "Yozuv qurollari va ofis jihozlari", DisplayOrder = 5 }
                };

            await context.Categories.AddRangeAsync(categories);
        }

        // Discount Reasons
        if (!context.DiscountReasons.Any())
        {
            var reasons = new List<DiscountReason>
                {
                    new() { Name = "Doimiy mijoz", Description = "Doimiy mijozlarga chegirma" },
                    new() { Name = "Ommaviy xarid", Description = "Ko'p miqdorda xarid qilganlarga" },
                    new() { Name = "Aksiya", Description = "Maxsus aksiya doirasida" },
                    new() { Name = "Tanishtiruv", Description = "Yangi mijozni tanishtirganlarga" },
                    new() { Name = "Shikastlangan mahsulot", Description = "Ozgina shikastlangan mahsulotlarga" }
                };

            await context.DiscountReasons.AddRangeAsync(reasons);
        }

        // Delivery Partners
        if (!context.DeliveryPartners.Any())
        {
            var partners = new List<DeliveryPartner>
                {
                    new() { Name = "O'zbekiston Pochtasi", Phone = "+998712345678", DeliveryType = "Postal", PricePerDelivery = 15000, EstimatedDays = 2 },
                    new() { Name = "Express Pochta", Phone = "+998712345679", DeliveryType = "Express", PricePerDelivery = 25000, EstimatedDays = 1 },
                    new() { Name = "Tez Kuryer", Phone = "+998712345680", DeliveryType = "Courier", PricePerDelivery = 20000, EstimatedDays = 1 }
                };

            await context.DeliveryPartners.AddRangeAsync(partners);
        }

        // Admin User
        if (!context.Admins.Any())
        {
            var admin = new Admin
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), // Change this!
                Role = "SuperAdmin"
            };

            await context.Admins.AddAsync(admin);
        }

        await context.SaveChangesAsync();
    }
}
