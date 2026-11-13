using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.EntityFrameworkCore;
using ZiyoMarket.Data.Context;
using ZiyoMarket.Domain.Entities.Content;
using ZiyoMarket.Domain.Entities.Products;
using ZiyoMarket.Domain.Entities.Users;
using ZiyoMarket.Domain.Enums;
using ZiyoMarket.Service.DTOs.Auth;

namespace ZiyoMarket.Api.Seed;

/// <summary>
/// Sinov uchun demo ma'lumotlar: test akkauntlar, kategoriyalar, rasmli mahsulotlar va bannerlar.
/// Faqat <c>DemoData:Enabled=true</c> bo'lganda ishlaydi va faqat yo'q narsalarni qo'shadi:
/// admin o'chirgan demo mahsulot yoki akkaunt qayta yaratilmaydi.
///
/// Parollar repoda emas (repo ochiq): ular muhit o'zgaruvchilaridan olinadi
/// (DemoData__AdminPassword, DemoData__SellerPassword, DemoData__CustomerPassword).
/// Paroli berilmagan akkaunt yaratilmaydi.
/// </summary>
public static class DemoDataSeeder
{
    public const string AdminUsername = "demo_admin";
    public const string AdminPhone = "+998900000001";
    public const string SellerPhone = "+998900000002";
    public const string CustomerPhone = "+998900000003";

    private static readonly string[] DemoLikes = { "DEMO-001", "DEMO-014", "DEMO-020", "DEMO-028", "DEMO-009" };

    public static async Task SeedAsync(ZiyoMarketDbContext db, DemoDataSettings settings, S3Settings s3, ILogger logger)
    {
        if (!settings.Enabled) return;

        var seedDir = Path.Combine(AppContext.BaseDirectory, "Seed");
        var catalogPath = Path.Combine(seedDir, "demo-catalog.json");
        if (!File.Exists(catalogPath))
        {
            logger.LogWarning("Demo katalog topilmadi: {Path}", catalogPath);
            return;
        }

        var catalog = JsonSerializer.Deserialize<DemoCatalog>(await File.ReadAllTextAsync(catalogPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        using var s3Client = BuildS3Client(s3);
        if (s3Client == null)
            logger.LogWarning("S3 sozlanmagan: demo mahsulotlar rasmsiz qo'shiladi");

        async Task<string?> UploadAsync(string folder, string fileName)
        {
            var path = Path.Combine(seedDir, "DemoImages", fileName);
            if (s3Client == null || !File.Exists(path)) return null;
            var key = $"demo/{folder}/{fileName}";
            try
            {
                await s3Client.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = s3.BucketName,
                    Key = key,
                    FilePath = path,
                    ContentType = "image/webp",
                    CannedACL = S3CannedACL.PublicRead
                });
                return $"{s3.PublicUrl.TrimEnd('/')}/{key}";
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Demo rasm yuklanmadi: {Key}", key);
                return null;
            }
        }

        // ── Kategoriyalar ─────────────────────────────────────────────────
        var categories = await db.Categories.ToListAsync();
        Category? FindCategory(string name) =>
            categories.FirstOrDefault(c => string.Equals(c.Name.Trim(), name, StringComparison.OrdinalIgnoreCase));

        var order = 100;
        foreach (var c in catalog.Categories)
        {
            if (FindCategory(c.Name) != null) continue;
            var parent = c.Parent == null ? null : FindCategory(c.Parent);
            var category = new Category
            {
                Name = c.Name,
                Description = c.Description,
                ParentId = parent?.Id,
                IsActive = true,
                DisplayOrder = order++
            };
            db.Categories.Add(category);
            await db.SaveChangesAsync(); // bolalar ota-kategoriya Id'sini ishlatadi
            categories.Add(category);
        }

        // ── Mahsulotlar ───────────────────────────────────────────────────
        var existingQr = (await db.Products.IgnoreQueryFilters()
                .Where(p => p.QrCode.StartsWith("DEMO-"))
                .Select(p => p.QrCode)
                .ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var added = 0;
        foreach (var p in catalog.Products.Where(p => !existingQr.Contains(p.Qr)))
        {
            var category = FindCategory(p.Category);
            if (category == null) continue;

            var imageUrl = await UploadAsync("products", $"{p.Qr.ToLowerInvariant()}.webp");
            var isBook = p.Kind == "book";
            db.Products.Add(new Product
            {
                Name = p.Name,
                Description = p.Description,
                QrCode = p.Qr,
                Price = p.Price,
                StockQuantity = p.Stock,
                CategoryId = category.Id,
                Status = ProductStatus.Active,
                IsActive = true,
                // Kitobda muallif "ishlab chiqaruvchi" o'rnida: tavsiyalar bir muallifning boshqa kitoblarini topadi
                Manufacturer = isBook ? p.Author : p.Manufacturer,
                Publisher = isBook ? p.Publisher : null,
                PageCount = p.Pages,
                Language = isBook ? "uz" : null,
                ImageUrl = imageUrl,
                ImageUrls = imageUrl == null ? new List<string>() : new List<string> { imageUrl }
            });
            added++;
        }
        if (added > 0)
        {
            await db.SaveChangesAsync();
            logger.LogInformation("Demo mahsulotlar qo'shildi: {Count}", added);
        }

        // ── Bannerlar ─────────────────────────────────────────────────────
        var bannerTitles = (await db.Contents.IgnoreQueryFilters()
                .Where(c => c.ContentType == ContentType.Banner)
                .Select(c => c.Title)
                .ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var bannerOrder = -30;
        foreach (var b in catalog.Banners)
        {
            bannerOrder++;
            if (bannerTitles.Contains(b.Title)) continue;
            var url = await UploadAsync("banners", $"{b.Key}.webp");
            if (url == null) continue; // rasmsiz banner bo'sh joy bo'lib ko'rinadi
            db.Contents.Add(new Content
            {
                ContentType = ContentType.Banner,
                Title = b.Title,
                Description = b.Description,
                ContentUrl = url,
                ThumbnailUrl = url,
                IsActive = true,
                DisplayOrder = bannerOrder,
                ValidFrom = DateTime.UtcNow.AddDays(-1),
                TargetAudience = "All",
                Language = "uz"
            });
        }
        await db.SaveChangesAsync();

        // ── Test akkauntlar ───────────────────────────────────────────────
        static bool Has(string? s) => !string.IsNullOrWhiteSpace(s);

        if (Has(settings.AdminPassword)
            && !await db.Admins.IgnoreQueryFilters().AnyAsync(a => a.Username == AdminUsername || a.Phone == AdminPhone))
        {
            db.Admins.Add(new Admin
            {
                FirstName = "Demo",
                LastName = "Admin",
                Username = AdminUsername,
                Phone = AdminPhone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(settings.AdminPassword),
                Role = "SuperAdmin",
                IsActive = true
            });
            logger.LogInformation("Demo admin yaratildi: {Username}", AdminUsername);
        }

        if (Has(settings.SellerPassword)
            && !await db.Sellers.IgnoreQueryFilters().AnyAsync(s => s.Phone == SellerPhone))
        {
            db.Sellers.Add(new Seller
            {
                FirstName = "Demo",
                LastName = "Sotuvchi",
                Phone = SellerPhone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(settings.SellerPassword),
                Role = "Seller",
                IsActive = true
            });
            logger.LogInformation("Demo sotuvchi yaratildi: {Phone}", SellerPhone);
        }

        Customer? newCustomer = null;
        if (Has(settings.CustomerPassword)
            && !await db.Customers.IgnoreQueryFilters().AnyAsync(c => c.Phone == CustomerPhone))
        {
            newCustomer = new Customer
            {
                FirstName = "Demo",
                LastName = "Mijoz",
                Phone = CustomerPhone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(settings.CustomerPassword),
                Address = "Toshkent, Chilonzor tumani",
                CashbackBalance = 20000,
                IsActive = true
            };
            db.Customers.Add(newCustomer);
            logger.LogInformation("Demo mijoz yaratildi: {Phone}", CustomerPhone);
        }
        await db.SaveChangesAsync();

        // Yangi demo mijoz bir nechta mahsulotni yoqtirgan: bosh sahifadagi ommabop bo'lim bo'sh qolmaydi
        if (newCustomer != null)
        {
            var likeIds = await db.Products.Where(p => DemoLikes.Contains(p.QrCode)).Select(p => p.Id).ToListAsync();
            foreach (var id in likeIds)
                db.ProductLikes.Add(new ProductLike { CustomerId = newCustomer.Id, ProductId = id });
            await db.SaveChangesAsync();
        }
    }

    private static AmazonS3Client? BuildS3Client(S3Settings s3)
    {
        if (string.IsNullOrWhiteSpace(s3.AccessKey) || string.IsNullOrWhiteSpace(s3.SecretKey)
            || string.IsNullOrWhiteSpace(s3.BucketName) || string.IsNullOrWhiteSpace(s3.PublicUrl))
            return null;

        var config = new AmazonS3Config { ForcePathStyle = true };
        if (!string.IsNullOrWhiteSpace(s3.ServiceUrl))
            config.ServiceURL = s3.ServiceUrl;
        return new AmazonS3Client(s3.AccessKey, s3.SecretKey, config);
    }

    private sealed class DemoCatalog
    {
        public List<DemoCategory> Categories { get; set; } = new();
        public List<DemoProduct> Products { get; set; } = new();
        public List<DemoBanner> Banners { get; set; } = new();
    }

    private sealed class DemoCategory
    {
        public string Name { get; set; } = "";
        public string? Parent { get; set; }
        public string? Description { get; set; }
    }

    private sealed class DemoProduct
    {
        public string Qr { get; set; } = "";
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? Author { get; set; }
        public string? Publisher { get; set; }
        public string? Manufacturer { get; set; }
        public int? Pages { get; set; }
        public string Kind { get; set; } = "";
        public string? Description { get; set; }
    }

    private sealed class DemoBanner
    {
        public string Key { get; set; } = "";
        public string Title { get; set; } = "";
        public string? Description { get; set; }
    }
}

public class DemoDataSettings
{
    public bool Enabled { get; set; }
    public string? AdminPassword { get; set; }
    public string? SellerPassword { get; set; }
    public string? CustomerPassword { get; set; }
}
