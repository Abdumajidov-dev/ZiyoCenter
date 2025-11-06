using Bogus;
using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Data.Context;
using ZiyoMarket.Domain.Entities.Users;
using ZiyoMarket.Domain.Entities.Products;
using ZiyoMarket.Domain.Entities.Orders;
using ZiyoMarket.Domain.Entities.Delivery;
using ZiyoMarket.Domain.Entities.Notifications;
using ZiyoMarket.Domain.Entities.Support;
using ZiyoMarket.Domain.Entities.Systems;
using ZiyoMarket.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using ContentEntity = ZiyoMarket.Domain.Entities.Content.Content;

namespace ZiyoMarket.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeedDataController : ControllerBase
{
    private readonly ZiyoMarketDbContext _context;

    public SeedDataController(ZiyoMarketDbContext context)
    {
        _context = context;
    }

    // ==================== CUSTOMERS ====================
    [HttpPost("customers")]
    public async Task<IActionResult> SeedCustomers()
    {
        var faker = new Faker<Customer>()
            .RuleFor(c => c.FirstName, f => f.Name.FirstName())
            .RuleFor(c => c.LastName, f => f.Name.LastName())
            .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber("+998#########"))
            .RuleFor(c => c.Email, f => f.Internet.Email())
            .RuleFor(c => c.PasswordHash, f => BCrypt.Net.BCrypt.HashPassword("Test123!"))
            .RuleFor(c => c.Address, f => f.Address.FullAddress())
            .RuleFor(c => c.CashbackBalance, f => f.Random.Decimal(0, 50000))
            .RuleFor(c => c.IsActive, f => f.Random.Bool(0.9f))
            .RuleFor(c => c.CreatedAt, f => f.Date.Past(1).ToString("yyyy-MM-dd HH:mm:ss"));

        var customers = faker.Generate(10);
        await _context.Customers.AddRangeAsync(customers);
        await _context.SaveChangesAsync();

        return Ok(new { message = "10 customers created successfully", data = customers.Select(c => new { c.Id, c.FirstName, c.LastName, c.Phone }) });
    }

    // ==================== SELLERS ====================
    [HttpPost("sellers")]
    public async Task<IActionResult> SeedSellers()
    {
        var faker = new Faker<Seller>()
            .RuleFor(s => s.FirstName, f => f.Name.FirstName())
            .RuleFor(s => s.LastName, f => f.Name.LastName())
            .RuleFor(s => s.Phone, f => f.Phone.PhoneNumber("+998#########"))
            .RuleFor(s => s.PasswordHash, f => BCrypt.Net.BCrypt.HashPassword("Test123!"))
            .RuleFor(s => s.Role, f => f.PickRandom("Seller", "Manager"))
            .RuleFor(s => s.IsActive, f => f.Random.Bool(0.9f))
            .RuleFor(s => s.CreatedAt, f => f.Date.Past(1).ToString("yyyy-MM-dd HH:mm:ss"));

        var sellers = faker.Generate(10);
        await _context.Sellers.AddRangeAsync(sellers);
        await _context.SaveChangesAsync();

        return Ok(new { message = "10 sellers created successfully", data = sellers.Select(s => new { s.Id, s.FirstName, s.LastName, s.Role }) });
    }

    // ==================== ADMINS ====================
    [HttpPost("admins")]
    public async Task<IActionResult> SeedAdmins()
    {
        var faker = new Faker<Admin>()
            .RuleFor(a => a.FirstName, f => f.Name.FirstName())
            .RuleFor(a => a.LastName, f => f.Name.LastName())
            .RuleFor(a => a.Username, f => f.Internet.UserName())
            .RuleFor(a => a.Phone, f => f.Phone.PhoneNumber("+998#########"))
            .RuleFor(a => a.Email, f => f.Internet.Email())
            .RuleFor(a => a.PasswordHash, f => BCrypt.Net.BCrypt.HashPassword("Admin123!"))
            .RuleFor(a => a.Role, f => f.PickRandom("Admin", "SuperAdmin"))
            .RuleFor(a => a.IsActive, f => f.Random.Bool(0.95f))
            .RuleFor(a => a.LastLoginAt, f => f.Date.Recent(7))
            .RuleFor(a => a.CreatedAt, f => f.Date.Past(1).ToString("yyyy-MM-dd HH:mm:ss"));

        var admins = faker.Generate(10);
        await _context.Admins.AddRangeAsync(admins);
        await _context.SaveChangesAsync();

        return Ok(new { message = "10 admins created successfully", data = admins.Select(a => new { a.Id, a.Username, a.Email, a.Role }) });
    }

    // ==================== CATEGORIES ====================
    [HttpPost("categories")]
    public async Task<IActionResult> SeedCategories()
    {
        var faker = new Faker<Category>()
            .RuleFor(c => c.Name, f => f.Commerce.Categories(1)[0])
            .RuleFor(c => c.Description, f => f.Lorem.Sentence())
            .RuleFor(c => c.IsActive, f => f.Random.Bool(0.9f))
            .RuleFor(c => c.DisplayOrder, f => f.Random.Int(1, 100))
            .RuleFor(c => c.CreatedAt, f => f.Date.Past(1).ToString("yyyy-MM-dd HH:mm:ss"));

        var categories = faker.Generate(10);
        await _context.Categories.AddRangeAsync(categories);
        await _context.SaveChangesAsync();

        return Ok(new { message = "10 categories created successfully", data = categories.Select(c => new { c.Id, c.Name }) });
    }

    // ==================== PRODUCTS ====================
    [HttpPost("products")]
    public async Task<IActionResult> SeedProducts()
    {
        var categories = await _context.Categories.ToListAsync();
        if (!categories.Any())
            return BadRequest("Please seed categories first");

        var faker = new Faker<Product>()
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
            .RuleFor(p => p.QrCode, f => f.Random.AlphaNumeric(12))
            .RuleFor(p => p.Price, f => f.Random.Decimal(1000, 500000))
            .RuleFor(p => p.StockQuantity, f => f.Random.Int(0, 1000))
            .RuleFor(p => p.CategoryId, f => f.PickRandom(categories).Id)
            .RuleFor(p => p.Status, f => f.PickRandom<ProductStatus>())
            .RuleFor(p => p.IsActive, f => f.Random.Bool(0.9f))
            .RuleFor(p => p.MinStockLevel, f => f.Random.Int(5, 20))
            .RuleFor(p => p.Weight, f => f.Random.Decimal(10, 5000))
            .RuleFor(p => p.Manufacturer, f => f.Company.CompanyName())
            .RuleFor(p => p.DisplayOrder, f => f.Random.Int(1, 100))
            .RuleFor(p => p.CreatedAt, f => f.Date.Past(1).ToString("yyyy-MM-dd HH:mm:ss"));

        var products = faker.Generate(10);
        await _context.Products.AddRangeAsync(products);
        await _context.SaveChangesAsync();

        return Ok(new { message = "10 products created successfully", data = products.Select(p => new { p.Id, p.Name, p.Price }) });
    }

    // ==================== CART ITEMS ====================
    [HttpPost("cart-items")]
    public async Task<IActionResult> SeedCartItems()
    {
        var customers = await _context.Customers.ToListAsync();
        var products = await _context.Products.ToListAsync();

        if (!customers.Any() || !products.Any())
            return BadRequest("Please seed customers and products first");

        var faker = new Faker<CartItem>()
            .RuleFor(ci => ci.CustomerId, f => f.PickRandom(customers).Id)
            .RuleFor(ci => ci.ProductId, f => f.PickRandom(products).Id)
            .RuleFor(ci => ci.Quantity, f => f.Random.Int(1, 10))
            .RuleFor(ci => ci.CreatedAt, f => f.Date.Past(0).ToString("yyyy-MM-dd HH:mm:ss"));

        var cartItems = faker.Generate(10);
        await _context.CartItems.AddRangeAsync(cartItems);
        await _context.SaveChangesAsync();

        return Ok(new { message = "10 cart items created successfully", data = cartItems.Select(ci => new { ci.Id, ci.CustomerId, ci.ProductId, ci.Quantity }) });
    }

    // ==================== PRODUCT LIKES ====================
    [HttpPost("product-likes")]
    public async Task<IActionResult> SeedProductLikes()
    {
        var customers = await _context.Customers.ToListAsync();
        var products = await _context.Products.ToListAsync();

        if (!customers.Any() || !products.Any())
            return BadRequest("Please seed customers and products first");

        var faker = new Faker<ProductLike>()
            .RuleFor(pl => pl.CustomerId, f => f.PickRandom(customers).Id)
            .RuleFor(pl => pl.ProductId, f => f.PickRandom(products).Id)
            .RuleFor(pl => pl.CreatedAt, f => f.Date.Past(0).ToString("yyyy-MM-dd HH:mm:ss"));

        var likes = faker.Generate(10);
        await _context.ProductLikes.AddRangeAsync(likes);
        await _context.SaveChangesAsync();

        return Ok(new { message = "10 product likes created successfully" });
    }

    // ==================== DISCOUNT REASONS ====================
    [HttpPost("discount-reasons")]
    public async Task<IActionResult> SeedDiscountReasons()
    {
        var reasons = new List<DiscountReason>
        {
            new() { Name = "Doimiy mijoz", Description = "Doimiy mijozlar uchun chegirma", IsActive = true, CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new() { Name = "Ommaviy xarid", Description = "Ko'p mahsulot sotib olgani uchun", IsActive = true, CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new() { Name = "Promo aksiya", Description = "Promo aksiya chegirmasi", IsActive = true, CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new() { Name = "Maxsus taklif", Description = "Maxsus mijoz uchun taklif", IsActive = true, CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new() { Name = "Shikastlangan mahsulot", Description = "Mahsulot zararlangan", IsActive = true, CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new() { Name = "Amal muddati yaqin", Description = "Amal muddati tugashga yaqin", IsActive = true, CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new() { Name = "VIP mijoz", Description = "VIP mijozlar uchun maxsus chegirma", IsActive = true, CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new() { Name = "Tug'ilgan kun", Description = "Mijozning tug'ilgan kuni", IsActive = true, CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new() { Name = "Birinchi xarid", Description = "Birinchi bor xarid qilgani uchun", IsActive = true, CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new() { Name = "Boshqa", Description = "Boshqa sabablar", IsActive = true, CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
        };

        await _context.DiscountReasons.AddRangeAsync(reasons);
        await _context.SaveChangesAsync();

        return Ok(new { message = "10 discount reasons created successfully", data = reasons.Select(r => new { r.Id, r.Name }) });
    }

    // ==================== ORDERS ====================
    [HttpPost("orders")]
    public async Task<IActionResult> SeedOrders()
    {
        var customers = await _context.Customers.ToListAsync();
        var sellers = await _context.Sellers.ToListAsync();

        if (!customers.Any())
            return BadRequest("Please seed customers first");

        var faker = new Faker<Order>()
            .RuleFor(o => o.OrderNumber, f => $"ORD-{f.Date.Past(0):yyyyMMdd}-{f.Random.Int(100, 999)}")
            .RuleFor(o => o.CustomerId, f => f.PickRandom(customers).Id)
            .RuleFor(o => o.SellerId, f => sellers.Any() ? f.Random.Bool(0.5f) ? f.PickRandom(sellers).Id : null : null)
            .RuleFor(o => o.OrderDate, f => f.Date.Past(0).ToString("yyyy-MM-dd HH:mm:ss"))
            .RuleFor(o => o.Status, f => f.PickRandom<OrderStatus>())
            .RuleFor(o => o.TotalPrice, f => f.Random.Decimal(10000, 500000))
            .RuleFor(o => o.DiscountApplied, f => f.Random.Decimal(0, 10000))
            .RuleFor(o => o.CashbackUsed, f => f.Random.Decimal(0, 5000))
            .RuleFor(o => o.PaymentMethod, f => f.PickRandom<PaymentMethod>())
            .RuleFor(o => o.DeliveryType, f => f.PickRandom<DeliveryType>())
            .RuleFor(o => o.DeliveryAddress, (f, o) => o.DeliveryType != DeliveryType.Pickup ? f.Address.FullAddress() : null)
            .RuleFor(o => o.DeliveryFee, (f, o) => o.DeliveryType != DeliveryType.Pickup ? f.Random.Decimal(5000, 20000) : 0)
            .RuleFor(o => o.PaidAt, f => f.Random.Bool(0.8f) ? f.Date.Recent(7).ToString("yyyy-MM-dd HH:mm:ss") : null)
            .RuleFor(o => o.CreatedAt, f => f.Date.Past(0).ToString("yyyy-MM-dd HH:mm:ss"))
            .FinishWith((f, o) =>
            {
                o.FinalPrice = o.TotalPrice - o.DiscountApplied - o.CashbackUsed + o.DeliveryFee;
            });

        var orders = faker.Generate(10);
        await _context.Orders.AddRangeAsync(orders);
        await _context.SaveChangesAsync();

        return Ok(new { message = "10 orders created successfully", data = orders.Select(o => new { o.Id, o.OrderNumber, o.TotalPrice, o.Status }) });
    }

    // ==================== ORDER ITEMS ====================
    [HttpPost("order-items")]
    public async Task<IActionResult> SeedOrderItems()
    {
        var orders = await _context.Orders.ToListAsync();
        var products = await _context.Products.ToListAsync();

        if (!orders.Any() || !products.Any())
            return BadRequest("Please seed orders and products first");

        var faker = new Faker<OrderItem>()
            .RuleFor(oi => oi.OrderId, f => f.PickRandom(orders).Id)
            .RuleFor(oi => oi.ProductId, f => f.PickRandom(products).Id)
            .RuleFor(oi => oi.ProductName, f => f.Commerce.ProductName())
            .RuleFor(oi => oi.Quantity, f => f.Random.Int(1, 10))
            .RuleFor(oi => oi.UnitPrice, f => f.Random.Decimal(1000, 100000))
            .RuleFor(oi => oi.DiscountApplied, f => f.Random.Decimal(0, 5000))
            .RuleFor(oi => oi.CreatedAt, f => f.Date.Past(0).ToString("yyyy-MM-dd HH:mm:ss"));

        var orderItems = faker.Generate(10);
        await _context.OrderItems.AddRangeAsync(orderItems);
        await _context.SaveChangesAsync();

        return Ok(new { message = "10 order items created successfully" });
    }

    // ==================== ORDER DISCOUNTS ====================
    [HttpPost("order-discounts")]
    public async Task<IActionResult> SeedOrderDiscounts()
    {
        var orders = await _context.Orders.ToListAsync();
        var reasons = await _context.DiscountReasons.ToListAsync();
        var sellers = await _context.Sellers.ToListAsync();

        if (!orders.Any() || !reasons.Any() || !sellers.Any())
            return BadRequest("Please seed orders, discount reasons, and sellers first");

        var faker = new Faker<OrderDiscount>()
            .RuleFor(od => od.OrderId, f => f.PickRandom(orders).Id)
            .RuleFor(od => od.DiscountReasonId, f => f.PickRandom(reasons).Id)
            .RuleFor(od => od.Amount, f => f.Random.Decimal(1000, 50000))
            .RuleFor(od => od.AppliedBy, f => f.PickRandom(sellers).Id)
            .RuleFor(od => od.Notes, f => f.Lorem.Sentence())
            .RuleFor(od => od.CreatedAt, f => f.Date.Past(0).ToString("yyyy-MM-dd HH:mm:ss"));

        var discounts = faker.Generate(10);
        await _context.OrderDiscounts.AddRangeAsync(discounts);
        await _context.SaveChangesAsync();

        return Ok(new { message = "10 order discounts created successfully" });
    }

    // ==================== CASHBACK TRANSACTIONS ====================
    [HttpPost("cashback-transactions")]
    public async Task<IActionResult> SeedCashbackTransactions()
    {
        var customers = await _context.Customers.ToListAsync();
        var orders = await _context.Orders.ToListAsync();

        if (!customers.Any() || !orders.Any())
            return BadRequest("Please seed customers and orders first");

        var faker = new Faker<CashbackTransaction>()
            .RuleFor(ct => ct.CustomerId, f => f.PickRandom(customers).Id)
            .RuleFor(ct => ct.OrderId, f => f.PickRandom(orders).Id)
            .RuleFor(ct => ct.Amount, f => f.Random.Decimal(100, 10000))
            .RuleFor(ct => ct.Type, f => f.PickRandom<CashbackTransactionType>())
            .RuleFor(ct => ct.Description, f => f.Lorem.Sentence())
            .RuleFor(ct => ct.CreatedAt, f => f.Date.Past(0).ToString("yyyy-MM-dd HH:mm:ss"));

        var transactions = faker.Generate(10);
        await _context.CashbackTransactions.AddRangeAsync(transactions);
        await _context.SaveChangesAsync();

        return Ok(new { message = "10 cashback transactions created successfully" });
    }

    // ==================== DELIVERY PARTNERS ====================
    [HttpPost("delivery-partners")]
    public async Task<IActionResult> SeedDeliveryPartners()
    {
        var faker = new Faker<DeliveryPartner>()
            .RuleFor(dp => dp.Name, f => f.Company.CompanyName())
            .RuleFor(dp => dp.Phone, f => f.Phone.PhoneNumber("+998#########"))
            .RuleFor(dp => dp.Email, f => f.Internet.Email())
            .RuleFor(dp => dp.DeliveryType, f => f.PickRandom("Postal", "Courier", "Express"))
            .RuleFor(dp => dp.PricePerDelivery, f => f.Random.Decimal(5000, 50000))
            .RuleFor(dp => dp.EstimatedDays, f => f.Random.Int(1, 7))
            .RuleFor(dp => dp.IsActive, f => f.Random.Bool(0.9f))
            .RuleFor(dp => dp.CreatedAt, f => f.Date.Past(1).ToString("yyyy-MM-dd HH:mm:ss"));

        var partners = faker.Generate(10);
        await _context.DeliveryPartners.AddRangeAsync(partners);
        await _context.SaveChangesAsync();

        return Ok(new { message = "10 delivery partners created successfully", data = partners.Select(dp => new { dp.Id, dp.Name, dp.Phone }) });
    }

    // ==================== ORDER DELIVERIES ====================
    [HttpPost("order-deliveries")]
    public async Task<IActionResult> SeedOrderDeliveries()
    {
        var orders = await _context.Orders.Where(o => o.DeliveryType != DeliveryType.Pickup).ToListAsync();
        var partners = await _context.DeliveryPartners.ToListAsync();

        if (!orders.Any() || !partners.Any())
            return BadRequest("Please seed orders with delivery and delivery partners first");

        var faker = new Faker<OrderDelivery>()
            .RuleFor(od => od.OrderId, f => f.PickRandom(orders).Id)
            .RuleFor(od => od.DeliveryPartnerId, f => f.PickRandom(partners).Id)
            .RuleFor(od => od.DeliveryStatus, f => f.PickRandom<DeliveryStatus>())
            .RuleFor(od => od.DeliveryAddress, f => f.Address.FullAddress())
            .RuleFor(od => od.DeliveryFee, f => f.Random.Decimal(5000, 30000))
            .RuleFor(od => od.TrackingCode, f => $"TRK-{f.Date.Recent(7):yyyyMMdd}-{f.Random.Int(100000, 999999)}")
            .RuleFor(od => od.AssignedAt, f => f.Date.Recent(7))
            .RuleFor(od => od.PickedUpAt, f => f.Random.Bool(0.7f) ? f.Date.Recent(5) : (DateTime?)null)
            .RuleFor(od => od.DeliveredAt, f => f.Random.Bool(0.5f) ? f.Date.Recent(3) : (DateTime?)null)
            .RuleFor(od => od.Notes, f => f.Lorem.Sentence())
            .RuleFor(od => od.CreatedAt, f => f.Date.Past(0).ToString("yyyy-MM-dd HH:mm:ss"));

        var deliveries = faker.Generate(10);
        await _context.OrderDeliveries.AddRangeAsync(deliveries);
        await _context.SaveChangesAsync();

        return Ok(new { message = "10 order deliveries created successfully" });
    }

    // ==================== NOTIFICATIONS ====================
    [HttpPost("notifications")]
    public async Task<IActionResult> SeedNotifications()
    {
        var customers = await _context.Customers.ToListAsync();

        if (!customers.Any())
            return BadRequest("Please seed customers first");

        var faker = new Faker<Notification>()
            .RuleFor(n => n.UserId, f => f.PickRandom(customers).Id)
            .RuleFor(n => n.UserType, f => UserType.Customer)
            .RuleFor(n => n.NotificationType, f => f.PickRandom<NotificationType>())
            .RuleFor(n => n.Title, f => f.Lorem.Sentence(3))
            .RuleFor(n => n.Message, f => f.Lorem.Paragraph())
            .RuleFor(n => n.Priority, f => f.PickRandom("Low", "Normal", "High"))
            .RuleFor(n => n.IsRead, f => f.Random.Bool(0.3f))
            .RuleFor(n => n.ReadAt, (f, n) => n.IsRead ? f.Date.Recent(7) : (DateTime?)null)
            .RuleFor(n => n.CreatedAt, f => f.Date.Past(0).ToString("yyyy-MM-dd HH:mm:ss"));

        var notifications = faker.Generate(10);
        await _context.Notifications.AddRangeAsync(notifications);
        await _context.SaveChangesAsync();

        return Ok(new { message = "10 notifications created successfully" });
    }

    // ==================== SUPPORT CHATS ====================
    [HttpPost("support-chats")]
    public async Task<IActionResult> SeedSupportChats()
    {
        var customers = await _context.Customers.ToListAsync();
        var admins = await _context.Admins.ToListAsync();

        if (!customers.Any())
            return BadRequest("Please seed customers first");

        var faker = new Faker<SupportChat>()
            .RuleFor(sc => sc.CustomerId, f => f.PickRandom(customers).Id)
            .RuleFor(sc => sc.AdminId, f => admins.Any() && f.Random.Bool(0.7f) ? f.PickRandom(admins).Id : (int?)null)
            .RuleFor(sc => sc.Subject, f => f.Lorem.Sentence(5))
            .RuleFor(sc => sc.Status, f => f.PickRandom<SupportChatStatus>())
            .RuleFor(sc => sc.Priority, f => f.PickRandom("Low", "Normal", "High"))
            .RuleFor(sc => sc.CreatedAt, f => f.Date.Past(0).ToString("yyyy-MM-dd HH:mm:ss"));

        var chats = faker.Generate(10);
        await _context.SupportChats.AddRangeAsync(chats);
        await _context.SaveChangesAsync();

        return Ok(new { message = "10 support chats created successfully", data = chats.Select(sc => new { sc.Id, sc.Subject, sc.Status }) });
    }

    // ==================== SUPPORT MESSAGES ====================
    [HttpPost("support-messages")]
    public async Task<IActionResult> SeedSupportMessages()
    {
        var chats = await _context.SupportChats.ToListAsync();

        if (!chats.Any())
            return BadRequest("Please seed support chats first");

        var faker = new Faker<SupportMessage>()
            .RuleFor(sm => sm.ChatId, f => f.PickRandom(chats).Id)
            .RuleFor(sm => sm.SenderId, f => f.Random.Int(1, 100))
            .RuleFor(sm => sm.SenderType, f => f.PickRandom(UserType.Customer, UserType.Admin))
            .RuleFor(sm => sm.Message, f => f.Lorem.Paragraph())
            .RuleFor(sm => sm.MessageType, f => "Text")
            .RuleFor(sm => sm.IsRead, f => f.Random.Bool(0.8f))
            .RuleFor(sm => sm.ReadAt, (f, sm) => sm.IsRead ? f.Date.Recent(3) : (DateTime?)null)
            .RuleFor(sm => sm.CreatedAt, f => f.Date.Past(0).ToString("yyyy-MM-dd HH:mm:ss"));

        var messages = faker.Generate(10);
        await _context.SupportMessages.AddRangeAsync(messages);
        await _context.SaveChangesAsync();

        return Ok(new { message = "10 support messages created successfully" });
    }

    // ==================== CONTENTS ====================
    [HttpPost("contents")]
    public async Task<IActionResult> SeedContents()
    {
        var faker = new Faker<ContentEntity>()
            .RuleFor(c => c.Title, f => f.Lorem.Sentence(5))
            .RuleFor(c => c.Description, f => f.Lorem.Paragraphs(3))
            .RuleFor(c => c.ContentType, f => f.PickRandom<ContentType>())
            .RuleFor(c => c.TargetAudience, f => f.PickRandom("All", "Customers", "Sellers"))
            .RuleFor(c => c.IsActive, f => f.Random.Bool(0.8f))
            .RuleFor(c => c.ViewCount, f => f.Random.Int(0, 1000))
            .RuleFor(c => c.ClickCount, f => f.Random.Int(0, 500))
            .RuleFor(c => c.ValidFrom, f => f.Date.Past(1))
            .RuleFor(c => c.ValidUntil, f => f.Random.Bool(0.7f) ? f.Date.Future(1) : (DateTime?)null)
            .RuleFor(c => c.CreatedAt, f => f.Date.Past(1).ToString("yyyy-MM-dd HH:mm:ss"));

        var contents = faker.Generate(10);
        await _context.Contents.AddRangeAsync(contents);
        await _context.SaveChangesAsync();

        return Ok(new { message = "10 contents created successfully", data = contents.Select(c => new { c.Id, c.Title, c.ContentType }) });
    }

    // ==================== SYSTEM SETTINGS ====================
    [HttpPost("system-settings")]
    public async Task<IActionResult> SeedSystemSettings()
    {
        var settings = new List<SystemSetting>
        {
            new() { SettingKey = "CashbackPercentage", SettingValue = "2", Description = "Cashback foizi", DataType = "Number", CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new() { SettingKey = "MinOrderAmount", SettingValue = "10000", Description = "Minimal buyurtma summasi", DataType = "Number", CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new() { SettingKey = "DeliveryFeeStandard", SettingValue = "15000", Description = "Standart yetkazish to'lovi", DataType = "Number", CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new() { SettingKey = "DeliveryFeeExpress", SettingValue = "25000", Description = "Tezkor yetkazish to'lovi", DataType = "Number", CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new() { SettingKey = "FreeDeliveryThreshold", SettingValue = "100000", Description = "Bepul yetkazish chegarasi", DataType = "Number", CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new() { SettingKey = "MaxDiscountPercentSeller", SettingValue = "20", Description = "Sotuvchi bera oladigan max chegirma %", DataType = "Number", CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new() { SettingKey = "MaxDiscountPercentManager", SettingValue = "50", Description = "Manager bera oladigan max chegirma %", DataType = "Number", CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new() { SettingKey = "LowStockThreshold", SettingValue = "10", Description = "Kam zaxira chegarasi", DataType = "Number", CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new() { SettingKey = "NotificationExpireDays", SettingValue = "30", Description = "Xabarnoma amal muddati (kunlar)", DataType = "Number", CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new() { SettingKey = "MaintenanceMode", SettingValue = "false", Description = "Texnik ishlar rejimi", DataType = "Boolean", CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
        };

        await _context.SystemSettings.AddRangeAsync(settings);
        await _context.SaveChangesAsync();

        return Ok(new { message = "10 system settings created successfully", data = settings.Select(s => new { s.Id, s.SettingKey, s.SettingValue }) });
    }

    // ==================== DAILY SALES SUMMARIES ====================
    [HttpPost("daily-sales-summaries")]
    public async Task<IActionResult> SeedDailySalesSummaries()
    {
        var faker = new Faker<DailySalesSummary>()
            .RuleFor(dss => dss.SaleDate, f => DateOnly.FromDateTime(f.Date.Past(30)))
            .RuleFor(dss => dss.TotalOrders, f => f.Random.Int(10, 200))
            .RuleFor(dss => dss.TotalRevenue, f => f.Random.Decimal(100000, 5000000))
            .RuleFor(dss => dss.TotalDiscount, f => f.Random.Decimal(10000, 500000))
            .RuleFor(dss => dss.CashbackGiven, f => f.Random.Decimal(2000, 100000))
            .RuleFor(dss => dss.CashbackUsed, f => f.Random.Decimal(1000, 50000))
            .RuleFor(dss => dss.NewCustomers, f => f.Random.Int(0, 20))
            .RuleFor(dss => dss.ReturningCustomers, f => f.Random.Int(0, 50))
            .RuleFor(dss => dss.OnlineOrders, f => f.Random.Int(5, 100))
            .RuleFor(dss => dss.OfflineOrders, f => f.Random.Int(5, 100))
            .RuleFor(dss => dss.DeliveredOrders, f => f.Random.Int(5, 150))
            .RuleFor(dss => dss.CancelledOrders, f => f.Random.Int(0, 10))
            .RuleFor(dss => dss.TotalItemsSold, f => f.Random.Int(20, 500))
            .RuleFor(dss => dss.AverageOrderValue, f => f.Random.Decimal(50000, 300000))
            .RuleFor(dss => dss.CashPayments, f => f.Random.Decimal(50000, 2000000))
            .RuleFor(dss => dss.CardPayments, f => f.Random.Decimal(50000, 3000000))
            .RuleFor(dss => dss.CreatedAt, f => f.Date.Past(0).ToString("yyyy-MM-dd HH:mm:ss"));

        var summaries = faker.Generate(10);
        await _context.DailySalesSummaries.AddRangeAsync(summaries);
        await _context.SaveChangesAsync();

        return Ok(new { message = "10 daily sales summaries created successfully" });
    }

    // ==================== SEED ALL ====================
    [HttpPost("seed-all")]
    public async Task<IActionResult> SeedAll()
    {
        var results = new List<string>();

        try
        {
            // Seed in correct order to maintain referential integrity
            await SeedCustomers();
            results.Add("Customers seeded");

            await SeedSellers();
            results.Add("Sellers seeded");

            await SeedAdmins();
            results.Add("Admins seeded");

            await SeedCategories();
            results.Add("Categories seeded");

            await SeedProducts();
            results.Add("Products seeded");

            await SeedCartItems();
            results.Add("Cart items seeded");

            await SeedProductLikes();
            results.Add("Product likes seeded");

            await SeedDiscountReasons();
            results.Add("Discount reasons seeded");

            await SeedOrders();
            results.Add("Orders seeded");

            await SeedOrderItems();
            results.Add("Order items seeded");

            await SeedOrderDiscounts();
            results.Add("Order discounts seeded");

            await SeedCashbackTransactions();
            results.Add("Cashback transactions seeded");

            await SeedDeliveryPartners();
            results.Add("Delivery partners seeded");

            await SeedOrderDeliveries();
            results.Add("Order deliveries seeded");

            await SeedNotifications();
            results.Add("Notifications seeded");

            await SeedSupportChats();
            results.Add("Support chats seeded");

            await SeedSupportMessages();
            results.Add("Support messages seeded");

            await SeedContents();
            results.Add("Contents seeded");

            await SeedSystemSettings();
            results.Add("System settings seeded");

            await SeedDailySalesSummaries();
            results.Add("Daily sales summaries seeded");

            return Ok(new { message = "All data seeded successfully!", results });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error seeding data", error = ex.Message, results });
        }
    }

    // ==================== CLEAR ALL ====================
    [HttpDelete("clear-all")]
    public async Task<IActionResult> ClearAll()
    {
        try
        {
            // Delete in reverse order to maintain referential integrity
            _context.DailySalesSummaries.RemoveRange(_context.DailySalesSummaries);
            _context.SystemSettings.RemoveRange(_context.SystemSettings);
            _context.Contents.RemoveRange(_context.Contents);
            _context.SupportMessages.RemoveRange(_context.SupportMessages);
            _context.SupportChats.RemoveRange(_context.SupportChats);
            _context.Notifications.RemoveRange(_context.Notifications);
            _context.OrderDeliveries.RemoveRange(_context.OrderDeliveries);
            _context.DeliveryPartners.RemoveRange(_context.DeliveryPartners);
            _context.CashbackTransactions.RemoveRange(_context.CashbackTransactions);
            _context.OrderDiscounts.RemoveRange(_context.OrderDiscounts);
            _context.OrderItems.RemoveRange(_context.OrderItems);
            _context.Orders.RemoveRange(_context.Orders);
            _context.DiscountReasons.RemoveRange(_context.DiscountReasons);
            _context.ProductLikes.RemoveRange(_context.ProductLikes);
            _context.CartItems.RemoveRange(_context.CartItems);
            _context.Products.RemoveRange(_context.Products);
            _context.Categories.RemoveRange(_context.Categories);
            _context.Admins.RemoveRange(_context.Admins);
            _context.Sellers.RemoveRange(_context.Sellers);
            _context.Customers.RemoveRange(_context.Customers);

            await _context.SaveChangesAsync();

            return Ok(new { message = "All data cleared successfully!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error clearing data", error = ex.Message });
        }
    }
}
