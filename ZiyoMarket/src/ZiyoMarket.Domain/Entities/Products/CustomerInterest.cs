using ZiyoMarket.Domain.Common;

namespace ZiyoMarket.Domain.Entities.Products;

/// <summary>
/// Mijoz mahsulot sahifasini necha marta va qachon oxirgi marta ochgani.
/// Har bir (mijoz, mahsulot) juftligi uchun bitta qator: tavsiyalar uchun signal.
/// </summary>
public class ProductView : BaseEntity
{
    public int CustomerId { get; set; }
    public int ProductId { get; set; }
    public int ViewCount { get; set; } = 1;
    public DateTime LastViewedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Mijoz qidirgan so'z (kichik harfda). Har bir (mijoz, so'z) juftligi uchun bitta qator.
/// </summary>
public class SearchQuery : BaseEntity
{
    public int CustomerId { get; set; }
    public string Term { get; set; } = string.Empty;
    public int SearchCount { get; set; } = 1;
    public DateTime LastSearchedAt { get; set; } = DateTime.UtcNow;
}
