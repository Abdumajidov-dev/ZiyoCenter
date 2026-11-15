using Microsoft.EntityFrameworkCore;
using ZiyoMarket.Data.Context;
using ZiyoMarket.Domain.Common;
using ZiyoMarket.Domain.Entities.Content;
using ZiyoMarket.Domain.Entities.Finance;
using ZiyoMarket.Domain.Entities.Notifications;
using ZiyoMarket.Domain.Entities.Orders;
using ZiyoMarket.Domain.Entities.Products;
using ZiyoMarket.Domain.Entities.Support;
using ZiyoMarket.Domain.Entities.Systems;
using ZiyoMarket.Domain.Entities.Users;
using ZiyoMarket.Domain.Enums;

namespace ZiyoMarket.Api.Seed;

/// <summary>
/// Admin panelni sinash uchun "jonli" ma'lumotlar: qo'shimcha mijozlar, sotuvchilar, cheklangan
/// adminlar, so'nggi 60 kundagi buyurtmalar (barcha holatlarda), keshbek, xarajatlar, yangiliklar,
/// FAQ, bildirishnomalar, qo'llab-quvvatlash chatlari va ko'rishlar.
///
/// Bir marta ishlaydi: "DEMO-ORD-" bilan boshlanadigan buyurtma bo'lsa, qayta qo'shilmaydi.
/// Akkaunt parollari DemoData__*Password o'zgaruvchilaridan olinadi (repo ochiq).
/// </summary>
public static class DemoMockDataSeeder
{
    public const string OrderPrefix = "DEMO-ORD-";

    // Qo'shimcha sinov akkauntlari (asosiy uchtasi DemoDataSeeder'da)
    public static readonly (string Username, string Phone, string First, string Last, string[] Permissions)[] ExtraAdmins =
    {
        ("demo_operator", "+998900000006", "Dilnoza", "Operator",
            new[] { AdminPermissions.OrdersView, AdminPermissions.OrdersManage, AdminPermissions.OrdersPayments,
                    AdminPermissions.CustomersView, AdminPermissions.SupportManage, AdminPermissions.ReportsView }),
        ("demo_kontent", "+998900000007", "Jasur", "Kontent",
            new[] { AdminPermissions.ProductsManage, AdminPermissions.CategoriesManage, AdminPermissions.ContentManage,
                    AdminPermissions.NotificationsSend }),
    };

    public static readonly (string Phone, string First, string Last, string Role)[] ExtraSellers =
    {
        ("+998900000004", "Bobur", "Sotuvchi", "Seller"),
        ("+998900000005", "Malika", "Menejer", "Manager"),
    };

    private static readonly (string First, string Last, string City)[] MockCustomers =
    {
        ("Aziza", "Karimova", "Toshkent, Yunusobod"), ("Sherzod", "Tursunov", "Toshkent, Mirzo Ulug'bek"),
        ("Madina", "Rahimova", "Samarqand"), ("Otabek", "Yusupov", "Buxoro"),
        ("Gulnoza", "Ergasheva", "Namangan"), ("Javohir", "Alimov", "Andijon"),
        ("Nodira", "Saidova", "Farg'ona"), ("Bekzod", "Hamidov", "Toshkent, Chilonzor"),
        ("Shahnoza", "Qodirova", "Qarshi"), ("Ulug'bek", "Nazarov", "Nukus"),
        ("Feruza", "Mirzayeva", "Jizzax"), ("Sardor", "Xolmatov", "Termiz"),
    };

    public static string MockCustomerPhone(int index) => $"+9989000001{index:00}";

    public static async Task SeedAsync(ZiyoMarketDbContext db, DemoDataSettings settings,
        Func<string, string, Task<string?>> upload, ILogger logger)
    {
        if (await db.Orders.IgnoreQueryFilters().AnyAsync(o => o.OrderNumber.StartsWith(OrderPrefix)))
            return;

        var rnd = new Random(20261115); // har safar bir xil natija
        var serverNow = DateTime.UtcNow; // OrderDate UTC vaqtida saqlanadi (CreateOrder ham shunday yozadi)
        string Stamp(DateTime d) => d.ToString("yyyy-MM-dd HH:mm:ss");
        static bool Has(string? s) => !string.IsNullOrWhiteSpace(s);

        // ── Akkauntlar ─────────────────────────────────────────────────────
        if (Has(settings.AdminPassword))
        {
            foreach (var a in ExtraAdmins)
            {
                if (await db.Admins.IgnoreQueryFilters().AnyAsync(x => x.Username == a.Username || x.Phone == a.Phone)) continue;
                var admin = new Admin
                {
                    FirstName = a.First, LastName = a.Last, Username = a.Username, Phone = a.Phone,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(settings.AdminPassword), Role = "Admin", IsActive = true
                };
                admin.SetPermissions(a.Permissions);
                db.Admins.Add(admin);
            }
        }

        if (Has(settings.SellerPassword))
        {
            foreach (var s in ExtraSellers)
            {
                if (await db.Sellers.IgnoreQueryFilters().AnyAsync(x => x.Phone == s.Phone)) continue;
                db.Sellers.Add(new Seller
                {
                    FirstName = s.First, LastName = s.Last, Phone = s.Phone, Role = s.Role, IsActive = true,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(settings.SellerPassword)
                });
            }
        }

        // Mijozlar OTP bilan kiradi; parol faqat "parol bilan kirish" uchun zaxira
        var customerHash = BCrypt.Net.BCrypt.HashPassword(Has(settings.CustomerPassword) ? settings.CustomerPassword : Guid.NewGuid().ToString());
        for (var i = 0; i < MockCustomers.Length; i++)
        {
            var phone = MockCustomerPhone(i + 1);
            if (await db.Customers.IgnoreQueryFilters().AnyAsync(c => c.Phone == phone)) continue;
            var c = MockCustomers[i];
            db.Customers.Add(new Customer
            {
                FirstName = c.First, LastName = c.Last, Phone = phone, Address = c.City,
                PasswordHash = customerHash, IsActive = i != MockCustomers.Length - 1 // bittasi bloklangan: sinov uchun
            });
        }
        await db.SaveChangesAsync();

        var customerPhones = Enumerable.Range(1, MockCustomers.Length).Select(MockCustomerPhone)
            .Append(DemoDataSeeder.CustomerPhone).ToList();
        var customers = await db.Customers.Where(c => customerPhones.Contains(c.Phone)).ToListAsync();
        var sellerPhones = ExtraSellers.Select(s => s.Phone).Append(DemoDataSeeder.SellerPhone).ToList();
        var sellers = await db.Sellers.Where(s => sellerPhones.Contains(s.Phone)).ToListAsync();
        var products = await db.Products.Where(p => p.QrCode.StartsWith("DEMO-")).ToListAsync();
        var demoAdmin = await db.Admins.FirstOrDefaultAsync(a => a.Username == DemoDataSeeder.AdminUsername);
        if (customers.Count == 0 || products.Count == 0)
        {
            logger.LogWarning("Mock buyurtmalar uchun mijoz yoki demo mahsulot yo'q");
            return;
        }

        // ── Buyurtmalar: so'nggi 60 kun, barcha holatlar ──────────────────
        var receiptUrl = await upload("receipts", "demo-receipt.webp");
        var statusPlan = new (OrderStatus Status, int Count)[]
        {
            (OrderStatus.Delivered, 30), (OrderStatus.Shipped, 4), (OrderStatus.ReadyForPickup, 3),
            (OrderStatus.Preparing, 4), (OrderStatus.Confirmed, 4), (OrderStatus.AwaitingConfirmation, 5),
            (OrderStatus.Pending, 4), (OrderStatus.Cancelled, 6)
        };
        var orders = new List<Order>();
        var n = 0;
        foreach (var (status, count) in statusPlan)
        {
            for (var k = 0; k < count; k++)
            {
                n++;
                // Yakunlanganlar uzoqroq o'tmishda, jarayondagilar yaqin kunlarda
                var daysAgo = status switch
                {
                    OrderStatus.Delivered or OrderStatus.Cancelled => rnd.Next(3, 60),
                    OrderStatus.Pending or OrderStatus.AwaitingConfirmation => rnd.Next(0, 2),
                    _ => rnd.Next(0, 6)
                };
                var date = serverNow.AddDays(-daysAgo).AddMinutes(-rnd.Next(0, 600));
                var customer = customers[rnd.Next(customers.Count)];
                var bySeller = sellers.Count > 0 && rnd.NextDouble() < 0.3 && status != OrderStatus.AwaitingConfirmation;

                var items = products.OrderBy(_ => rnd.Next()).Take(rnd.Next(1, 4))
                    .Select(p => new OrderItem { ProductId = p.Id, ProductName = p.Name, Quantity = rnd.Next(1, 3), UnitPrice = p.Price })
                    .ToList();
                var total = items.Sum(i => i.UnitPrice * i.Quantity);
                var delivery = bySeller ? DeliveryType.Pickup : (DeliveryType)rnd.Next(1, 4);
                var fee = delivery switch { DeliveryType.Courier => 20000m, DeliveryType.Postal => 15000m, _ => 0m };
                var discount = bySeller && rnd.NextDouble() < 0.5 ? Math.Round(total * 0.05m / 1000) * 1000 : 0m;
                var paid = status is OrderStatus.Confirmed or OrderStatus.Preparing or OrderStatus.ReadyForPickup
                    or OrderStatus.Shipped or OrderStatus.Delivered;
                var method = bySeller ? PaymentMethod.Cash : PaymentMethod.Card;

                orders.Add(new Order
                {
                    OrderNumber = $"{OrderPrefix}{n:0000}",
                    CustomerId = customer.Id,
                    SellerId = bySeller ? sellers[rnd.Next(sellers.Count)].Id : null,
                    OrderDate = Stamp(date),
                    Status = status,
                    PaymentMethod = method,
                    DeliveryType = delivery,
                    DeliveryAddress = delivery == DeliveryType.Pickup ? null : customer.Address,
                    DeliveryFee = fee,
                    TotalPrice = total,
                    DiscountApplied = discount,
                    FinalPrice = total - discount + fee,
                    PaidAt = paid ? Stamp(date.AddHours(1)) : null,
                    PaymentReceiptUrl = method == PaymentMethod.Card && status != OrderStatus.Pending ? receiptUrl : null,
                    CustomerNotes = rnd.NextDouble() < 0.3 ? "Iltimos, kechqurun yetkazing" : null,
                    AdminNotes = status == OrderStatus.Cancelled ? "Mijoz so'rovi bilan bekor qilindi (demo)" : null,
                    OrderItems = items
                });
            }
        }
        db.Orders.AddRange(orders);
        await db.SaveChangesAsync();

        // ── Keshbek: yetkazilgan buyurtmalardan 2% ─────────────────────────
        var tx = 0;
        foreach (var o in orders.Where(o => o.Status == OrderStatus.Delivered))
        {
            var amount = Math.Round(o.TotalPrice * 0.02m);
            db.CashbackTransactions.Add(new CashbackTransaction
            {
                CustomerId = o.CustomerId, OrderId = o.Id, Type = CashbackTransactionType.Earned,
                Amount = amount, RemainingAmount = amount, EarnedAt = o.OrderDate,
                ExpiresAt = Stamp(DateTime.ParseExact(o.OrderDate, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture).AddDays(30)),
                Description = $"{o.OrderNumber} buyurtmasi uchun keshbek", TransactionNumber = $"DEMO-CB-{++tx:0000}"
            });
            var customer = customers.First(c => c.Id == o.CustomerId);
            customer.CashbackBalance += amount;
        }

        // ── Xarajatlar (moliya bo'limi) ────────────────────────────────────
        var expenses = new (string Title, ExpenseCategory Cat, decimal Amount, int DaysAgo)[]
        {
            ("Do'kon ijarasi (oktyabr)", ExpenseCategory.Rent, 6_000_000, 45), ("Do'kon ijarasi (noyabr)", ExpenseCategory.Rent, 6_000_000, 14),
            ("Sotuvchilar maoshi (oktyabr)", ExpenseCategory.Salary, 9_500_000, 40), ("Sotuvchilar maoshi (noyabr)", ExpenseCategory.Salary, 9_500_000, 10),
            ("Instagram reklama", ExpenseCategory.Marketing, 1_200_000, 25), ("Telegram kanal reklama", ExpenseCategory.Marketing, 800_000, 6),
            ("Elektr va internet", ExpenseCategory.Utilities, 650_000, 20), ("Kuryer xizmati", ExpenseCategory.Logistics, 1_400_000, 12),
            ("Javon va vitrina", ExpenseCategory.Equipment, 3_200_000, 50), ("Qadoqlash materiallari", ExpenseCategory.Consumption, 450_000, 4),
        };
        foreach (var e in expenses)
            db.Expenses.Add(new Expense
            {
                Title = e.Title, Category = e.Cat, Amount = e.Amount, Description = "Sinov ma'lumoti",
                ExpenseDate = serverNow.Date.AddDays(-e.DaysAgo)
            });

        // ── To'lov kartasi: faol karta bo'lmasa, sinov kartasi ──────────────
        if (!await db.PaymentCards.AnyAsync(c => c.IsActive))
            db.PaymentCards.Add(new PaymentCard
            {
                CardNumber = "8600 0000 0000 0000", CardHolder = "DEMO KARTA", BankName = "Sinov banki", IsActive = true,
                Note = "Faqat sinov uchun. Haqiqiy kartani qo'shib, buni o'chiring."
            });

        // ── Yangiliklar va FAQ ─────────────────────────────────────────────
        var newsImages = new[] { "banner-1.webp", "banner-2.webp", "banner-3.webp" };
        var news = new (string Title, string Body)[]
        {
            ("Yangi kitoblar keldi", "Do'konimizga O'zbek va jahon adabiyotidan yangi kitoblar keldi. Ilovada \"Siz uchun\" bo'limida sizga mos kitoblarni ko'ring."),
            ("Bepul yetkazib berish", "100 000 so'mdan ortiq buyurtmalar Toshkent bo'ylab bepul yetkaziladi. Aksiya oy oxirigacha amal qiladi."),
            ("Keshbek tizimi ishga tushdi", "Har bir xariddan 2% keshbek oling va keyingi xaridda ishlating. Balansingizni profil bo'limida ko'ring."),
        };
        for (var i = 0; i < news.Length; i++)
        {
            if (await db.Contents.AnyAsync(c => c.ContentType == ContentType.Article && c.Title == news[i].Title)) continue;
            db.Contents.Add(new ZiyoMarket.Domain.Entities.Content.Content
            {
                ContentType = ContentType.Article, Title = news[i].Title, Description = news[i].Body,
                ContentUrl = await upload("banners", newsImages[i]), IsActive = true, Language = "uz",
                ValidFrom = DateTime.UtcNow.AddDays(-(i * 5 + 1)), TargetAudience = "All"
            });
        }

        var faqs = new (string Q, string A, string Cat)[]
        {
            ("Buyurtma qancha vaqtda yetkaziladi?", "Toshkent bo'ylab 1–2 kun, viloyatlarga pochta orqali 2–4 kun.", "Yetkazib berish"),
            ("Qanday to'lov qilsam bo'ladi?", "Ilovada ko'rsatilgan kartaga o'tkazib, chek rasmini yuborasiz. Do'konda naqd ham to'lash mumkin.", "To'lov"),
            ("Chekni yubordim, keyin nima bo'ladi?", "Admin to'lovni tekshiradi va buyurtma \"Tasdiqlandi\" holatiga o'tadi. Holatni \"Shartnomalar\" bo'limida kuzating.", "To'lov"),
            ("Keshbek nima?", "Har bir xariddan 2% balansingizga qaytadi va 30 kun ichida keyingi xaridda ishlatiladi.", "Keshbek"),
            ("Buyurtmani bekor qilsam bo'ladimi?", "Ha, buyurtma tasdiqlanmaguncha \"Shartnomalar\" bo'limidan bekor qilishingiz mumkin.", "Buyurtma"),
            ("Mahsulotni qaytarsam bo'ladimi?", "Brak bo'lsa 14 kun ichida almashtiramiz. Qo'llab-quvvatlash bo'limiga yozing.", "Buyurtma"),
        };
        for (var i = 0; i < faqs.Length; i++)
        {
            if (await db.Faqs.AnyAsync(f => f.Question == faqs[i].Q)) continue;
            db.Faqs.Add(new Faq { Question = faqs[i].Q, Answer = faqs[i].A, Category = faqs[i].Cat, SortOrder = i + 1, IsActive = true });
        }

        // ── Bildirishnomalar ───────────────────────────────────────────────
        var demoCustomer = customers.FirstOrDefault(c => c.Phone == DemoDataSeeder.CustomerPhone);
        if (demoCustomer != null)
        {
            var mine = orders.Where(o => o.CustomerId == demoCustomer.Id).Take(3).ToList();
            foreach (var o in mine)
                db.Notifications.Add(new Notification
                {
                    UserId = demoCustomer.Id, UserType = UserType.Customer, NotificationType = NotificationType.OrderStatusChanged,
                    Title = "Buyurtma holati o'zgardi", Message = $"{o.OrderNumber} buyurtmangiz holati: {o.Status}",
                    IsRead = o.Status == OrderStatus.Delivered, Data = $"{{\"order_id\":{o.Id}}}"
                });
            db.Notifications.Add(new Notification
            {
                UserId = demoCustomer.Id, UserType = UserType.Customer, NotificationType = NotificationType.Promotion,
                Title = "Hafta aksiyasi", Message = "Bolalar kitoblariga 15% chegirma. Ilovada \"Bolalar kitoblari\" bo'limini oching."
            });
        }
        if (demoAdmin != null)
        {
            foreach (var o in orders.Where(o => o.Status == OrderStatus.AwaitingConfirmation).Take(3))
                db.Notifications.Add(new Notification
                {
                    UserId = demoAdmin.Id, UserType = UserType.Admin, NotificationType = NotificationType.PaymentReceiptUploaded,
                    Title = "Yangi to'lov cheki", Message = $"{o.OrderNumber} buyurtmasi uchun chek yuborildi. Tekshiring.",
                    Priority = "High", Data = $"{{\"order_id\":{o.Id}}}"
                });
            db.Notifications.Add(new Notification
            {
                UserId = demoAdmin.Id, UserType = UserType.Admin, NotificationType = NotificationType.LowStock,
                Title = "Mahsulot tugayapti", Message = "\"Erkaklar to'ni (milliy, beqasam)\" omborda 3 ta qoldi."
            });
        }

        // Kam qolgan mahsulot: dashboard'dagi "kam qolgan" ko'rsatkichi uchun
        var lowStock = products.FirstOrDefault(p => p.QrCode == "DEMO-021");
        if (lowStock != null) lowStock.StockQuantity = 3;

        // ── Qo'llab-quvvatlash chatlari ────────────────────────────────────
        var chatPlan = new (string Subject, SupportChatStatus Status, string[] Messages)[]
        {
            ("Buyurtmam qachon keladi?", SupportChatStatus.Open,
                new[] { "C:Assalomu alaykum, 3 kun oldin buyurtma bergandim, hali kelmadi. Qachon yetkaziladi?" }),
            ("To'lov chekini yubordim", SupportChatStatus.InProgress,
                new[] { "C:Kartaga pul o'tkazib, chekni yubordim. Tasdiqlanishi qancha vaqt oladi?",
                        "A:Va alaykum assalom! Chekingizni ko'rdik, 1 soat ichida tasdiqlaymiz." }),
            ("Kitob sahifasi yirtilgan", SupportChatStatus.Closed,
                new[] { "C:Olgan kitobimning 2 ta sahifasi yirtilgan ekan.", "A:Uzr so'raymiz! Ertaga kuryer yangisini olib boradi.",
                        "C:Rahmat, almashtirib berishdi." }),
        };
        for (var i = 0; i < chatPlan.Length; i++)
        {
            var customer = customers[i % customers.Count];
            var plan = chatPlan[i];
            var chat = new SupportChat
            {
                CustomerId = customer.Id, CreatedBy = customer.Id, Subject = plan.Subject, Status = plan.Status,
                Priority = i == 0 ? "High" : "Normal", Category = "Buyurtma", StartedAt = serverNow.AddHours(-(i * 20 + 2)),
                AdminId = plan.Status == SupportChatStatus.Open ? null : demoAdmin?.Id,
                CloseReason = plan.Status == SupportChatStatus.Closed ? "Hal qilindi" : null,
                CustomerRating = plan.Status == SupportChatStatus.Closed ? 5 : null,
                Resolution = plan.Status == SupportChatStatus.Closed ? "Mahsulot almashtirildi" : string.Empty
            };
            db.SupportChats.Add(chat);
            await db.SaveChangesAsync();
            foreach (var m in plan.Messages)
            {
                var fromAdmin = m.StartsWith("A:");
                db.SupportMessages.Add(new SupportMessage
                {
                    ChatId = chat.Id, SenderType = fromAdmin ? UserType.Admin : UserType.Customer,
                    SenderId = fromAdmin ? demoAdmin?.Id ?? 0 : customer.Id, Message = m[2..],
                    IsRead = plan.Status != SupportChatStatus.Open || fromAdmin
                });
            }
        }

        // ── Ko'rishlar va yoqtirishlar: tavsiyalar va "ommabop" uchun ──────
        foreach (var customer in customers.Where(c => c.Phone != DemoDataSeeder.CustomerPhone))
        {
            foreach (var p in products.OrderBy(_ => rnd.Next()).Take(rnd.Next(3, 8)))
                db.ProductViews.Add(new ProductView
                {
                    CustomerId = customer.Id, ProductId = p.Id, ViewCount = rnd.Next(1, 5),
                    LastViewedAt = DateTime.UtcNow.AddDays(-rnd.Next(0, 20))
                });
            foreach (var p in products.OrderBy(_ => rnd.Next()).Take(rnd.Next(0, 3)))
                db.ProductLikes.Add(new ProductLike { CustomerId = customer.Id, ProductId = p.Id });
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Demo mock ma'lumotlar qo'shildi: {Orders} buyurtma, {Customers} mijoz", orders.Count, customers.Count);
    }
}
