using Microsoft.EntityFrameworkCore;
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

        // SuperAdmin — IgnoreQueryFilters() bilan tekshiriladi, soft-delete bo'lgan bo'lsa ham qayta yaratilmaydi
        if (!context.Admins.IgnoreQueryFilters().Any(a => a.Username == "Bek"))
        {
            var superAdmin = new Admin
            {
                FirstName = "Avazbek",
                LastName = "Abdumajidov",
                Username = "Bek",
                Phone = "+998882641919",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("2641919"),
                Role = "SuperAdmin",
                IsActive = true
            };

            await context.Admins.AddAsync(superAdmin);
        }

        await context.SaveChangesAsync();

        // ── Test Customers ──────────────────────────────────────────────────────
        if (!await context.Customers.AnyAsync())
        {
            var customers = new List<Customer>
            {
                new() { FirstName = "Ali",     LastName = "Karimov",   Phone = "+998901234501", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"), Address = "Toshkent, Chilonzor tumani",  CashbackBalance = 15000, IsActive = true },
                new() { FirstName = "Sardor",  LastName = "Toshmatov", Phone = "+998901234502", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"), Address = "Toshkent, Yunusobod tumani",  CashbackBalance = 8500,  IsActive = true },
                new() { FirstName = "Nilufar", LastName = "Yusupova",  Phone = "+998901234503", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"), Address = "Samarqand shahar",             CashbackBalance = 32000, IsActive = true },
                new() { FirstName = "Aziz",    LastName = "Rahimov",   Phone = "+998901234504", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"), Address = "Buxoro shahar",               CashbackBalance = 0,     IsActive = true },
                new() { FirstName = "Kamola",  LastName = "Hasanova",  Phone = "+998901234505", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"), Address = "Toshkent, Shayxontohur",     CashbackBalance = 5200,  IsActive = true },
            };
            await context.Customers.AddRangeAsync(customers);
            await context.SaveChangesAsync();
        }

        // ── Test Products ───────────────────────────────────────────────────────
        if (!await context.Products.AnyAsync())
        {
            var categories = await context.Categories.ToListAsync();
            var catKitob    = categories.FirstOrDefault(c => c.Name == "Kitoblar")?.Id        ?? categories[0].Id;
            var catDiniy    = categories.FirstOrDefault(c => c.Name == "Diniy Kitoblar")?.Id  ?? categories[0].Id;
            var catLibos    = categories.FirstOrDefault(c => c.Name == "Diniy Liboslar")?.Id  ?? categories[0].Id;
            var catSovga    = categories.FirstOrDefault(c => c.Name == "Sovg'alar")?.Id       ?? categories[0].Id;
            var catOfis     = categories.FirstOrDefault(c => c.Name == "Ofis Buyumlari")?.Id  ?? categories[0].Id;

            var products = new List<Product>
            {
                new() { Name = "Qur'on Karim (O'zbek tarjimasi)",  QrCode = "QR-KIT-001", Price = 85000,  StockQuantity = 50,  CategoryId = catDiniy, Status = ProductStatus.Active, IsActive = true, Manufacturer = "Toshkent Islom Universiteti" },
                new() { Name = "O'zbek tili va adabiyoti darsligi", QrCode = "QR-KIT-002", Price = 45000,  StockQuantity = 80,  CategoryId = catKitob, Status = ProductStatus.Active, IsActive = true, Manufacturer = "O'qituvchi nashriyoti" },
                new() { Name = "Python dasturlash asoslari",        QrCode = "QR-KIT-003", Price = 120000, StockQuantity = 30,  CategoryId = catKitob, Status = ProductStatus.Active, IsActive = true, Manufacturer = "IT-Press" },
                new() { Name = "Namoz kitobi (to'liq)",             QrCode = "QR-KIT-004", Price = 35000,  StockQuantity = 100, CategoryId = catDiniy, Status = ProductStatus.Active, IsActive = true, Manufacturer = "Movarounnahr" },
                new() { Name = "Erkaklar do'ppisi (qora)",          QrCode = "QR-LIB-001", Price = 55000,  StockQuantity = 40,  CategoryId = catLibos, Status = ProductStatus.Active, IsActive = true, Manufacturer = "Andijonteks" },
                new() { Name = "Ayollar hijob ro'moli",             QrCode = "QR-LIB-002", Price = 38000,  StockQuantity = 60,  CategoryId = catLibos, Status = ProductStatus.Active, IsActive = true, Manufacturer = "Silk Way" },
                new() { Name = "Tasbeh (yog'och, 33 dona)",         QrCode = "QR-SOV-001", Price = 28000,  StockQuantity = 200, CategoryId = catSovga, Status = ProductStatus.Active, IsActive = true, Manufacturer = "Hunarmand" },
                new() { Name = "Kalit brelok (ZiyoCenter logosi)",  QrCode = "QR-SOV-002", Price = 15000,  StockQuantity = 150, CategoryId = catSovga, Status = ProductStatus.Active, IsActive = true, Manufacturer = "ZiyoMarket" },
                new() { Name = "A4 daftar (96 varaq, chiziqli)",    QrCode = "QR-OFI-001", Price = 12000,  StockQuantity = 300, CategoryId = catOfis,  Status = ProductStatus.Active, IsActive = true, Manufacturer = "Iqtisodiyot" },
                new() { Name = "Rangli qalam to'plami (12 ta)",     QrCode = "QR-OFI-002", Price = 22000,  StockQuantity = 120, CategoryId = catOfis,  Status = ProductStatus.Active, IsActive = true, Manufacturer = "Pilot" },
            };
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }

        // ── Mock Orders (10 ta) ─────────────────────────────────────────────────
        if (!await context.Orders.AnyAsync())
        {
            var customers = await context.Customers.ToListAsync();
            var products  = await context.Products.ToListAsync();
            var now       = DateTime.UtcNow;

            // Mahsulotlarni nomga qarab topish
            Product Prod(string name) => products.First(p => p.Name.Contains(name));

            var orders = new List<Order>
            {
                // 1 — Delivered (to'liq tugallangan, cashback ishlagan)
                new Order
                {
                    OrderNumber = "ORD-20260401-101", CustomerId = customers[0].Id,
                    OrderDate = now.AddDays(-10).ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = OrderStatus.Delivered, PaymentMethod = PaymentMethod.Card,
                    DeliveryType = DeliveryType.Courier, DeliveryAddress = "Toshkent, Chilonzor 5-uy",
                    DeliveryFee = 15000, PaidAt = now.AddDays(-10).ToString("yyyy-MM-dd HH:mm:ss"),
                    TotalPrice = 205000, DiscountApplied = 0, CashbackUsed = 0, FinalPrice = 220000,
                    OrderItems = new List<OrderItem>
                    {
                        new() { ProductId = Prod("Qur'on").Id, ProductName = "Qur'on Karim (O'zbek tarjimasi)", Quantity = 2, UnitPrice = 85000 },
                        new() { ProductId = Prod("Namoz").Id,  ProductName = "Namoz kitobi (to'liq)",           Quantity = 1, UnitPrice = 35000 },
                    }
                },

                // 2 — Delivered (chegirma bilan)
                new Order
                {
                    OrderNumber = "ORD-20260402-102", CustomerId = customers[1].Id,
                    OrderDate = now.AddDays(-8).ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = OrderStatus.Delivered, PaymentMethod = PaymentMethod.Cash,
                    DeliveryType = DeliveryType.Pickup, DeliveryFee = 0,
                    PaidAt = now.AddDays(-8).ToString("yyyy-MM-dd HH:mm:ss"),
                    TotalPrice = 165000, DiscountApplied = 15000, CashbackUsed = 0, FinalPrice = 150000,
                    OrderItems = new List<OrderItem>
                    {
                        new() { ProductId = Prod("Python").Id, ProductName = "Python dasturlash asoslari", Quantity = 1, UnitPrice = 120000 },
                        new() { ProductId = Prod("daftar").Id, ProductName = "A4 daftar (96 varaq, chiziqli)", Quantity = 3, UnitPrice = 12000, DiscountApplied = 15000 },
                    }
                },

                // 3 — Delivered (cashback ishlatgan)
                new Order
                {
                    OrderNumber = "ORD-20260403-103", CustomerId = customers[2].Id,
                    OrderDate = now.AddDays(-6).ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = OrderStatus.Delivered, PaymentMethod = PaymentMethod.Mixed,
                    DeliveryType = DeliveryType.Postal, DeliveryAddress = "Samarqand, Registon ko'chasi 12",
                    DeliveryFee = 20000, PaidAt = now.AddDays(-6).ToString("yyyy-MM-dd HH:mm:ss"),
                    TotalPrice = 93000, DiscountApplied = 0, CashbackUsed = 13000, FinalPrice = 100000,
                    OrderItems = new List<OrderItem>
                    {
                        new() { ProductId = Prod("hijob").Id,    ProductName = "Ayollar hijob ro'moli",          Quantity = 2, UnitPrice = 38000 },
                        new() { ProductId = Prod("Tasbeh").Id,   ProductName = "Tasbeh (yog'och, 33 dona)",      Quantity = 1, UnitPrice = 28000 },
                        new() { ProductId = Prod("brelok").Id,   ProductName = "Kalit brelok (ZiyoCenter logosi)", Quantity = 1, UnitPrice = 15000 },
                    }
                },

                // 4 — Shipped (yo'lda)
                new Order
                {
                    OrderNumber = "ORD-20260405-104", CustomerId = customers[3].Id,
                    OrderDate = now.AddDays(-4).ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = OrderStatus.Shipped, PaymentMethod = PaymentMethod.Card,
                    DeliveryType = DeliveryType.Courier, DeliveryAddress = "Buxoro, Navoiy ko'chasi 7",
                    DeliveryFee = 15000, PaidAt = now.AddDays(-4).ToString("yyyy-MM-dd HH:mm:ss"),
                    TotalPrice = 120000, DiscountApplied = 0, CashbackUsed = 0, FinalPrice = 135000,
                    OrderItems = new List<OrderItem>
                    {
                        new() { ProductId = Prod("Python").Id, ProductName = "Python dasturlash asoslari", Quantity = 1, UnitPrice = 120000 },
                    }
                },

                // 5 — ReadyForPickup
                new Order
                {
                    OrderNumber = "ORD-20260406-105", CustomerId = customers[4].Id,
                    OrderDate = now.AddDays(-3).ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = OrderStatus.ReadyForPickup, PaymentMethod = PaymentMethod.Card,
                    DeliveryType = DeliveryType.Pickup, DeliveryFee = 0,
                    PaidAt = now.AddDays(-3).ToString("yyyy-MM-dd HH:mm:ss"),
                    TotalPrice = 93000, DiscountApplied = 0, CashbackUsed = 0, FinalPrice = 93000,
                    OrderItems = new List<OrderItem>
                    {
                        new() { ProductId = Prod("do'ppisi").Id,     ProductName = "Erkaklar do'ppisi (qora)",    Quantity = 1, UnitPrice = 55000 },
                        new() { ProductId = Prod("Tasbeh").Id,       ProductName = "Tasbeh (yog'och, 33 dona)",  Quantity = 1, UnitPrice = 28000 },
                        new() { ProductId = Prod("brelok").Id,       ProductName = "Kalit brelok (ZiyoCenter logosi)", Quantity = 1, UnitPrice = 15000 },
                    }
                },

                // 6 — Preparing
                new Order
                {
                    OrderNumber = "ORD-20260408-106", CustomerId = customers[0].Id,
                    OrderDate = now.AddDays(-2).ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = OrderStatus.Preparing, PaymentMethod = PaymentMethod.Cash,
                    DeliveryType = DeliveryType.Pickup, DeliveryFee = 0,
                    PaidAt = null,
                    TotalPrice = 90000, DiscountApplied = 0, CashbackUsed = 0, FinalPrice = 90000,
                    OrderItems = new List<OrderItem>
                    {
                        new() { ProductId = Prod("O'zbek tili").Id, ProductName = "O'zbek tili va adabiyoti darsligi", Quantity = 2, UnitPrice = 45000 },
                    }
                },

                // 7 — Confirmed
                new Order
                {
                    OrderNumber = "ORD-20260409-107", CustomerId = customers[1].Id,
                    OrderDate = now.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = OrderStatus.Confirmed, PaymentMethod = PaymentMethod.Card,
                    DeliveryType = DeliveryType.Courier, DeliveryAddress = "Toshkent, Yunusobod 14-mavze",
                    DeliveryFee = 15000, PaidAt = now.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss"),
                    TotalPrice = 123000, DiscountApplied = 0, CashbackUsed = 0, FinalPrice = 138000,
                    OrderItems = new List<OrderItem>
                    {
                        new() { ProductId = Prod("Qur'on").Id,    ProductName = "Qur'on Karim (O'zbek tarjimasi)", Quantity = 1, UnitPrice = 85000 },
                        new() { ProductId = Prod("qalam").Id,     ProductName = "Rangli qalam to'plami (12 ta)",  Quantity = 1, UnitPrice = 22000 },
                        new() { ProductId = Prod("daftar").Id,    ProductName = "A4 daftar (96 varaq, chiziqli)", Quantity = 1, UnitPrice = 16000 },
                    }
                },

                // 8 — Pending (yangi)
                new Order
                {
                    OrderNumber = "ORD-20260410-108", CustomerId = customers[2].Id,
                    OrderDate = now.AddHours(-5).ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = OrderStatus.Pending, PaymentMethod = PaymentMethod.Cash,
                    DeliveryType = DeliveryType.Pickup, DeliveryFee = 0,
                    PaidAt = null,
                    TotalPrice = 55000, DiscountApplied = 0, CashbackUsed = 0, FinalPrice = 55000,
                    OrderItems = new List<OrderItem>
                    {
                        new() { ProductId = Prod("do'ppisi").Id, ProductName = "Erkaklar do'ppisi (qora)", Quantity = 1, UnitPrice = 55000 },
                    }
                },

                // 9 — Pending (yangi, online)
                new Order
                {
                    OrderNumber = "ORD-20260410-109", CustomerId = customers[3].Id,
                    OrderDate = now.AddHours(-2).ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = OrderStatus.Pending, PaymentMethod = PaymentMethod.Card,
                    DeliveryType = DeliveryType.Postal, DeliveryAddress = "Buxoro, Mustaqillik ko'chasi 3",
                    DeliveryFee = 20000, PaidAt = null,
                    TotalPrice = 60000, DiscountApplied = 0, CashbackUsed = 0, FinalPrice = 80000,
                    OrderItems = new List<OrderItem>
                    {
                        new() { ProductId = Prod("Namoz").Id,  ProductName = "Namoz kitobi (to'liq)",          Quantity = 1, UnitPrice = 35000 },
                        new() { ProductId = Prod("Tasbeh").Id, ProductName = "Tasbeh (yog'och, 33 dona)",      Quantity = 1, UnitPrice = 28000 },
                    }
                },

                // 10 — Cancelled
                new Order
                {
                    OrderNumber = "ORD-20260407-110", CustomerId = customers[4].Id,
                    OrderDate = now.AddDays(-5).ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = OrderStatus.Cancelled, PaymentMethod = PaymentMethod.Cash,
                    DeliveryType = DeliveryType.Pickup, DeliveryFee = 0,
                    PaidAt = null,
                    TotalPrice = 85000, DiscountApplied = 0, CashbackUsed = 0, FinalPrice = 85000,
                    AdminNotes = "Bekor qilindi: mijoz so'rovi bilan",
                    OrderItems = new List<OrderItem>
                    {
                        new() { ProductId = Prod("Qur'on").Id, ProductName = "Qur'on Karim (O'zbek tarjimasi)", Quantity = 1, UnitPrice = 85000 },
                    }
                },
            };

            await context.Orders.AddRangeAsync(orders);
            await context.SaveChangesAsync();
        }
    }
}
