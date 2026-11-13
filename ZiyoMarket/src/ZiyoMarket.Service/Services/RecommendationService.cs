using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ZiyoMarket.Data.UnitOfWorks;
using ZiyoMarket.Domain.Entities.Products;
using ZiyoMarket.Domain.Enums;
using ZiyoMarket.Service.DTOs.Products;
using ZiyoMarket.Service.Interfaces;

namespace ZiyoMarket.Service.Services;

/// <summary>
/// Kontent + xulq-atvorga asoslangan tavsiya algoritmi (katta marketplace'lardagi kabi).
///
/// Mijoz qiziqishi signallardan yig'iladi, har biri vaqt o'tishi bilan susayadi:
///   ko'rish 1 (5 martagacha), yoqtirish 3, savat 4, xarid 5, qidiruv so'zi 2.
/// Har bir mahsulot quyidagilar bo'yicha baholanadi:
///   - shu kategoriya va qo'shni (bir ota-kategoriyadagi) kategoriyalarga qiziqish;
///   - bir xil ishlab chiqaruvchi / nashriyot;
///   - nomi va tavsifi mijoz qidirgan so'zlarga mosligi;
///   - "buni olganlar buni ham oldi" (birga sotib olish) va "buni ko'rganlar buni ham ko'rdi";
///   - narxi mijoz odatda ko'radigan narxlarga yaqinligi.
/// Keyin xilma-xillik qo'llanadi: bitta kategoriya ro'yxatni to'ldirib yubormaydi.
/// Signal yo'q bo'lsa (yangi mijoz yoki mehmon), ommabop va yangi mahsulotlar qaytadi.
/// </summary>
public class RecommendationService : IRecommendationService
{
    private const double ViewWeight = 1, LikeWeight = 3, CartWeight = 4, OrderWeight = 5, SearchWeight = 2;
    private const double InterestHalfLifeDays = 14, SearchHalfLifeDays = 7;

    private readonly IUnitOfWork _uow;
    private readonly IProductService _productService;
    private readonly ILogger<RecommendationService> _logger;

    public RecommendationService(IUnitOfWork uow, IProductService productService, ILogger<RecommendationService> logger)
    {
        _uow = uow;
        _productService = productService;
        _logger = logger;
    }

    // ===================== Signal yozish =====================

    public async Task TrackViewAsync(int customerId, int productId)
    {
        try
        {
            var view = await _uow.ProductViews.SelectAsync(v => v.CustomerId == customerId && v.ProductId == productId);
            if (view == null)
                await _uow.ProductViews.InsertAsync(new ProductView { CustomerId = customerId, ProductId = productId });
            else
            {
                view.ViewCount++;
                view.LastViewedAt = DateTime.UtcNow;
            }
            await _uow.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Signal yozilmasa ham mahsulot sahifasi ochilishi kerak
            _logger.LogWarning(ex, "Ko'rishni yozib bo'lmadi: customer {CustomerId}, product {ProductId}", customerId, productId);
        }
    }

    public async Task TrackSearchAsync(int customerId, string term)
    {
        term = (term ?? string.Empty).Trim().ToLowerInvariant();
        if (term.Length < 2) return;
        if (term.Length > 100) term = term[..100];

        try
        {
            var query = await _uow.SearchQueries.SelectAsync(q => q.CustomerId == customerId && q.Term == term);
            if (query == null)
                await _uow.SearchQueries.InsertAsync(new SearchQuery { CustomerId = customerId, Term = term });
            else
            {
                query.SearchCount++;
                query.LastSearchedAt = DateTime.UtcNow;
            }
            await _uow.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Qidiruvni yozib bo'lmadi: customer {CustomerId}", customerId);
        }
    }

    // ===================== "Siz uchun" =====================

    public async Task<List<ProductListDto>> GetRecommendedAsync(int? customerId, int count = 20)
    {
        count = Math.Clamp(count, 1, 50);
        var catalog = await LoadCatalogAsync();
        if (catalog.Count == 0) return new();

        var popularity = await LoadPopularityAsync(catalog);
        var scores = new Dictionary<int, double>();
        var interest = customerId.HasValue ? await LoadInterestAsync(customerId.Value) : new Interest();

        if (customerId.HasValue)
        {
            if (interest.HasSignals)
                scores = await ScorePersonalAsync(catalog, interest);

            // Savatdagi mahsulotlar tavsiya qilinmaydi, allaqachon olinganlar pastga tushadi
            foreach (var id in interest.InCart) scores.Remove(id);
            foreach (var id in interest.Ordered)
                if (scores.ContainsKey(id)) scores[id] *= 0.2;
        }

        // Shaxsiy natija yetmasa, ommabop mahsulotlar bilan to'ldiriladi
        var ranked = Diversify(scores, catalog, count);
        if (ranked.Count < count)
        {
            var exclude = new HashSet<int>(ranked);
            exclude.UnionWith(interest.InCart);

            var fill = catalog.Values
                .Where(p => !exclude.Contains(p.Id))
                .ToDictionary(p => p.Id, p => popularity.GetValueOrDefault(p.Id) + Freshness(p) + (p.InStock ? 0 : -5));
            ranked.AddRange(Diversify(fill, catalog, count - ranked.Count));
        }

        return await _productService.GetListByIdsAsync(ranked, customerId);
    }

    private async Task<Dictionary<int, double>> ScorePersonalAsync(Dictionary<int, CatalogItem> catalog, Interest interest)
    {
        var parentOf = await _uow.Categories.SelectAll(c => c.DeletedAt == null)
            .Select(c => new { c.Id, c.ParentId })
            .ToDictionaryAsync(c => c.Id, c => c.ParentId);

        int? Parent(int categoryId) => parentOf.GetValueOrDefault(categoryId);

        // Qiziqish profili: kategoriya, ota-kategoriya, brend, narx
        var categoryScore = new Dictionary<int, double>();
        var parentScore = new Dictionary<int, double>();
        var brandScore = new Dictionary<string, double>();
        double weightedLogPrice = 0, totalWeight = 0;

        foreach (var (productId, weight) in interest.ProductWeights)
        {
            if (!catalog.TryGetValue(productId, out var p)) continue;
            Add(categoryScore, p.CategoryId, weight);
            var parent = Parent(p.CategoryId) ?? p.CategoryId;
            Add(parentScore, parent, weight);
            foreach (var brand in p.Brands) Add(brandScore, brand, weight);
            if (p.Price > 0)
            {
                weightedLogPrice += weight * Math.Log((double)p.Price);
                totalWeight += weight;
            }
        }
        double? typicalLogPrice = totalWeight > 0 ? weightedLogPrice / totalWeight : null;

        var coBought = await CoPurchasedAsync(interest.ProductWeights.Keys.ToList());

        var scores = new Dictionary<int, double>();
        foreach (var p in catalog.Values)
        {
            double affinity = categoryScore.GetValueOrDefault(p.CategoryId)
                              + 0.5 * parentScore.GetValueOrDefault(Parent(p.CategoryId) ?? p.CategoryId);

            foreach (var brand in p.Brands)
                affinity += 0.6 * brandScore.GetValueOrDefault(brand);

            // Qidiruv so'zlari nom va tavsifda uchrasa
            foreach (var (stem, weight) in interest.SearchStems)
            {
                if (p.NameStems.Contains(stem)) affinity += SearchWeight * weight;
                else if (p.DescriptionStems.Contains(stem)) affinity += 0.5 * SearchWeight * weight;
            }

            affinity += 1.5 * Math.Min(coBought.GetValueOrDefault(p.Id), 5);
            if (affinity <= 0) continue;

            if (typicalLogPrice.HasValue && p.Price > 0)
                affinity *= 0.6 + 0.4 * Math.Exp(-Math.Abs(Math.Log((double)p.Price) - typicalLogPrice.Value));

            // Mijoz allaqachon ko'rgan/yoqtirgan narsadan ko'ra yangisini ko'rsatish afzal
            if (interest.Viewed.Contains(p.Id)) affinity *= 0.6;
            if (interest.Liked.Contains(p.Id)) affinity *= 0.5;
            if (!p.InStock) affinity *= 0.3;

            scores[p.Id] = affinity;
        }
        return scores;
    }

    // ===================== "O'xshash mahsulotlar" =====================

    public async Task<List<ProductListDto>> GetSimilarAsync(int productId, int? customerId, int count = 12)
    {
        count = Math.Clamp(count, 1, 50);
        var catalog = await LoadCatalogAsync();
        if (!catalog.TryGetValue(productId, out var target))
        {
            // Mahsulot mijozga ko'rinmasa ham (masalan nofaol), uning kategoriyasi bo'yicha o'xshashlar chiqadi
            var raw = await _uow.Products.SelectAll(p => p.Id == productId)
                .Select(p => new { p.Id, p.CategoryId, p.Price, p.Manufacturer, p.Publisher, p.Name, p.Description, p.StockQuantity, p.CreatedAt })
                .FirstOrDefaultAsync();
            if (raw == null) return new();
            target = new CatalogItem(raw.Id, raw.CategoryId, raw.Price, raw.Manufacturer, raw.Publisher, raw.Name, raw.Description, raw.StockQuantity, raw.CreatedAt);
        }

        var parentOf = await _uow.Categories.SelectAll(c => c.DeletedAt == null)
            .Select(c => new { c.Id, c.ParentId })
            .ToDictionaryAsync(c => c.Id, c => c.ParentId);
        var targetParent = parentOf.GetValueOrDefault(target.CategoryId);

        var coBought = await CoPurchasedAsync(new List<int> { productId });
        var coViewed = await CoViewedAsync(productId);
        var popularity = await LoadPopularityAsync(catalog);

        var scores = new Dictionary<int, double>();
        foreach (var p in catalog.Values)
        {
            if (p.Id == productId) continue;
            var parent = parentOf.GetValueOrDefault(p.CategoryId);

            double score = 0;
            if (p.CategoryId == target.CategoryId) score += 3;
            else if (parent.HasValue && parent == targetParent) score += 1.5;               // qo'shni kategoriya
            else if (parent == target.CategoryId || targetParent == p.CategoryId) score += 1; // ota yoki bola kategoriya

            if (p.Brands.Overlaps(target.Brands)) score += 1.5;
            score += 4 * Jaccard(p.NameStems, target.NameStems);
            score += 1.0 * Math.Min(coBought.GetValueOrDefault(p.Id), 5);
            score += 0.5 * Math.Min(coViewed.GetValueOrDefault(p.Id), 6);
            if (score <= 0) continue;

            if (p.Price > 0 && target.Price > 0)
                score *= 0.5 + 0.5 * Math.Exp(-Math.Abs(Math.Log((double)p.Price / (double)target.Price)));

            score += 0.02 * popularity.GetValueOrDefault(p.Id); // tenglikda ommabopi oldinda
            if (!p.InStock) score *= 0.4;
            scores[p.Id] = score;
        }

        var ranked = scores.OrderByDescending(s => s.Value).Take(count).Select(s => s.Key).ToList();
        if (ranked.Count < count)
        {
            var exclude = new HashSet<int>(ranked) { productId };
            ranked.AddRange(catalog.Values
                .Where(p => !exclude.Contains(p.Id))
                .OrderByDescending(p => popularity.GetValueOrDefault(p.Id) + Freshness(p))
                .Take(count - ranked.Count)
                .Select(p => p.Id));
        }

        return await _productService.GetListByIdsAsync(ranked, customerId);
    }

    // ===================== Ma'lumot yuklash =====================

    private async Task<Dictionary<int, CatalogItem>> LoadCatalogAsync()
    {
        var rows = await _uow.Products
            .SelectAll(p => p.DeletedAt == null && p.IsActive
                            && p.Status != ProductStatus.Inactive && p.Status != ProductStatus.Deleted)
            .Select(p => new { p.Id, p.CategoryId, p.Price, p.Manufacturer, p.Publisher, p.Name, p.Description, p.StockQuantity, p.CreatedAt })
            .ToListAsync();

        return rows.ToDictionary(r => r.Id,
            r => new CatalogItem(r.Id, r.CategoryId, r.Price, r.Manufacturer, r.Publisher, r.Name, r.Description, r.StockQuantity, r.CreatedAt));
    }

    /// <summary>Ommaboplik: yoqtirishlar, so'nggi 60 kundagi sotuvlar va ko'rishlar.</summary>
    private async Task<Dictionary<int, double>> LoadPopularityAsync(Dictionary<int, CatalogItem> catalog)
    {
        var since = DateTime.UtcNow.AddDays(-60).ToString("yyyy-MM-dd HH:mm:ss");

        var likes = await _uow.ProductLikes.SelectAll(l => l.DeletedAt == null)
            .GroupBy(l => l.ProductId).Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => (double)x.Count);

        var sold = await _uow.OrderItems
            .SelectAll(i => i.Order.Status != OrderStatus.Cancelled && string.Compare(i.Order.OrderDate, since) >= 0)
            .GroupBy(i => i.ProductId).Select(g => new { g.Key, Qty = g.Sum(i => i.Quantity) })
            .ToDictionaryAsync(x => x.Key, x => (double)x.Qty);

        var views = await _uow.ProductViews.SelectAll()
            .GroupBy(v => v.ProductId).Select(g => new { g.Key, Count = g.Sum(v => v.ViewCount) })
            .ToDictionaryAsync(x => x.Key, x => (double)x.Count);

        return catalog.Keys.ToDictionary(id => id,
            id => 2 * likes.GetValueOrDefault(id) + sold.GetValueOrDefault(id) + 0.2 * views.GetValueOrDefault(id));
    }

    private async Task<Interest> LoadInterestAsync(int customerId)
    {
        var now = DateTime.UtcNow;
        var interest = new Interest();

        var views = await _uow.ProductViews.SelectAll(v => v.CustomerId == customerId)
            .OrderByDescending(v => v.LastViewedAt).Take(200)
            .Select(v => new { v.ProductId, v.ViewCount, v.LastViewedAt })
            .ToListAsync();
        foreach (var v in views)
        {
            interest.Viewed.Add(v.ProductId);
            interest.AddWeight(v.ProductId, ViewWeight * Math.Min(v.ViewCount, 5) * Decay(now - v.LastViewedAt, InterestHalfLifeDays));
        }

        var likes = await _uow.ProductLikes.SelectAll(l => l.CustomerId == customerId && l.DeletedAt == null)
            .Select(l => new { l.ProductId, l.LikedAt }).ToListAsync();
        foreach (var l in likes)
        {
            interest.Liked.Add(l.ProductId);
            interest.AddWeight(l.ProductId, LikeWeight * Decay(now - l.LikedAt, InterestHalfLifeDays * 2));
        }

        var cart = await _uow.CartItems.SelectAll(c => c.CustomerId == customerId && c.DeletedAt == null)
            .Select(c => c.ProductId).ToListAsync();
        foreach (var id in cart)
        {
            interest.InCart.Add(id);
            interest.AddWeight(id, CartWeight);
        }

        var ordered = await _uow.OrderItems
            .SelectAll(i => i.Order.CustomerId == customerId && i.Order.Status != OrderStatus.Cancelled)
            .Select(i => new { i.ProductId, i.Order.OrderDate })
            .ToListAsync();
        foreach (var o in ordered)
        {
            interest.Ordered.Add(o.ProductId);
            var age = DateTime.TryParse(o.OrderDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var d)
                ? now - d.ToUniversalTime() : TimeSpan.Zero;
            interest.AddWeight(o.ProductId, OrderWeight * Decay(age, InterestHalfLifeDays * 3));
        }

        var searches = await _uow.SearchQueries.SelectAll(q => q.CustomerId == customerId)
            .OrderByDescending(q => q.LastSearchedAt).Take(30)
            .Select(q => new { q.Term, q.SearchCount, q.LastSearchedAt })
            .ToListAsync();
        foreach (var s in searches)
        {
            var weight = Math.Min(s.SearchCount, 3) * Decay(now - s.LastSearchedAt, SearchHalfLifeDays);
            foreach (var stem in Stems(s.Term))
                interest.SearchStems[stem] = interest.SearchStems.GetValueOrDefault(stem) + weight;
        }

        return interest;
    }

    /// <summary>Berilgan mahsulotlar bilan bitta buyurtmada sotib olingan boshqa mahsulotlar (buyurtmalar soni).</summary>
    private async Task<Dictionary<int, int>> CoPurchasedAsync(List<int> seedIds)
    {
        if (seedIds.Count == 0) return new();

        var orderIds = await _uow.OrderItems
            .SelectAll(i => seedIds.Contains(i.ProductId) && i.Order.Status != OrderStatus.Cancelled)
            .OrderByDescending(i => i.OrderId)
            .Select(i => i.OrderId).Distinct().Take(500)
            .ToListAsync();
        if (orderIds.Count == 0) return new();

        var pairs = await _uow.OrderItems
            .SelectAll(i => orderIds.Contains(i.OrderId) && !seedIds.Contains(i.ProductId))
            .Select(i => new { i.OrderId, i.ProductId })
            .Distinct()
            .ToListAsync();

        return pairs.GroupBy(p => p.ProductId).ToDictionary(g => g.Key, g => g.Count());
    }

    /// <summary>Shu mahsulotni ko'rgan mijozlar yana qaysi mahsulotlarni ko'rgan (mijozlar soni).</summary>
    private async Task<Dictionary<int, int>> CoViewedAsync(int productId)
    {
        var viewers = await _uow.ProductViews.SelectAll(v => v.ProductId == productId)
            .OrderByDescending(v => v.LastViewedAt).Take(300)
            .Select(v => v.CustomerId).ToListAsync();
        if (viewers.Count == 0) return new();

        return await _uow.ProductViews
            .SelectAll(v => viewers.Contains(v.CustomerId) && v.ProductId != productId)
            .GroupBy(v => v.ProductId).Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);
    }

    // ===================== Yordamchilar =====================

    /// <summary>
    /// Eng yuqori ballilarni tanlaydi, lekin bir kategoriyadan har keyingi mahsulot balini
    /// pasaytiradi — ro'yxatda turli narsalar bo'ladi.
    /// </summary>
    private static List<int> Diversify(Dictionary<int, double> scores, Dictionary<int, CatalogItem> catalog, int count)
    {
        var remaining = scores.Where(s => catalog.ContainsKey(s.Key)).ToDictionary(s => s.Key, s => s.Value);
        var picked = new List<int>();
        var perCategory = new Dictionary<int, int>();

        while (picked.Count < count && remaining.Count > 0)
        {
            var best = remaining
                .Select(s => (s.Key, Score: s.Value * Math.Pow(0.8, perCategory.GetValueOrDefault(catalog[s.Key].CategoryId))))
                .OrderByDescending(s => s.Score)
                .First();
            picked.Add(best.Key);
            remaining.Remove(best.Key);
            var category = catalog[best.Key].CategoryId;
            perCategory[category] = perCategory.GetValueOrDefault(category) + 1;
        }
        return picked;
    }

    /// <summary>So'nggi 30 kunda qo'shilgan mahsulotlarga kichik qo'shimcha ball.</summary>
    private static double Freshness(CatalogItem p)
    {
        if (!DateTime.TryParse(p.CreatedAt, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var created))
            return 0;
        var days = (DateTime.UtcNow - created.ToUniversalTime()).TotalDays;
        return days < 30 ? 1.5 * (1 - days / 30) : 0;
    }

    private static double Decay(TimeSpan age, double halfLifeDays)
        => Math.Pow(0.5, Math.Max(0, age.TotalDays) / halfLifeDays);

    private static void Add<TKey>(Dictionary<TKey, double> map, TKey key, double value) where TKey : notnull
        => map[key] = map.GetValueOrDefault(key) + value;

    private static double Jaccard(HashSet<string> a, HashSet<string> b)
    {
        if (a.Count == 0 || b.Count == 0) return 0;
        var intersection = a.Count(b.Contains);
        return intersection == 0 ? 0 : (double)intersection / (a.Count + b.Count - intersection);
    }

    private static readonly HashSet<string> StopWords = new(StringComparer.Ordinal)
    {
        "va", "uchun", "bilan", "the", "and", "for", "dona", "ta", "lik", "to'liq", "toliq", "yangi"
    };

    private static readonly Regex TokenSplit = new(@"[^\p{L}\p{Nd}]+", RegexOptions.Compiled);

    /// <summary>
    /// So'zlarni kichik harfga o'tkazib, apostroflarni olib tashlab, 5 harfgacha qisqartiradi:
    /// o'zbekcha qo'shimchalar ("kitob", "kitoblar", "kitobni") bir xil o'zakka tushadi.
    /// </summary>
    internal static HashSet<string> Stems(string? text)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(text)) return result;

        var normalized = text.ToLowerInvariant()
            .Replace("'", "").Replace("’", "").Replace("ʻ", "").Replace("ʼ", "").Replace("`", "").Replace("‘", "");
        foreach (var token in TokenSplit.Split(normalized))
        {
            if (token.Length < 3 || StopWords.Contains(token) || token.All(char.IsDigit)) continue;
            result.Add(token.Length > 5 ? token[..5] : token);
        }
        return result;
    }

    private sealed class CatalogItem
    {
        public CatalogItem(int id, int categoryId, decimal price, string? manufacturer, string? publisher,
            string name, string? description, int stock, string createdAt)
        {
            Id = id;
            CategoryId = categoryId;
            Price = price;
            InStock = stock > 0;
            CreatedAt = createdAt;
            NameStems = Stems(name);
            DescriptionStems = Stems(description);
            Brands = new HashSet<string>(
                new[] { manufacturer, publisher }
                    .Where(b => !string.IsNullOrWhiteSpace(b))
                    .Select(b => b!.Trim().ToLowerInvariant()));
        }

        public int Id { get; }
        public int CategoryId { get; }
        public decimal Price { get; }
        public bool InStock { get; }
        public string CreatedAt { get; }
        public HashSet<string> NameStems { get; }
        public HashSet<string> DescriptionStems { get; }
        public HashSet<string> Brands { get; }
    }

    private sealed class Interest
    {
        public Dictionary<int, double> ProductWeights { get; } = new();
        public Dictionary<string, double> SearchStems { get; } = new();
        public HashSet<int> Viewed { get; } = new();
        public HashSet<int> Liked { get; } = new();
        public HashSet<int> InCart { get; } = new();
        public HashSet<int> Ordered { get; } = new();

        public bool HasSignals => ProductWeights.Count > 0 || SearchStems.Count > 0;

        public void AddWeight(int productId, double weight)
            => ProductWeights[productId] = ProductWeights.GetValueOrDefault(productId) + weight;
    }
}
