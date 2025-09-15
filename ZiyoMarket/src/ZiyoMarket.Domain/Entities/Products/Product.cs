using ZiyoMarket.Domain.Common;
using ZiyoMarket.Domain.Entities.Orders;
using ZiyoMarket.Domain.Enums;

namespace ZiyoMarket.Domain.Entities.Products;

/// <summary>
/// Mahsulot entity'si
/// </summary>
public class Product : BaseAuditableEntity
{
    /// <summary>
    /// Mahsulot nomi
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mahsulot tavsifi
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// QR kod (unique)
    /// </summary>
    public string QrCode { get; set; } = string.Empty;

    /// <summary>
    /// Narx
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Zaxira miqdori
    /// </summary>
    public int StockQuantity { get; set; } = 0;

    /// <summary>
    /// Kategoriya ID
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Mahsulot holati
    /// </summary>
    public ProductStatus Status { get; set; } = ProductStatus.Active;

    /// <summary>
    /// Mahsulot rasmi URL
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Faolmi
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Minimal zaxira chegarasi (ogohlantirish uchun)
    /// </summary>
    public int MinStockLevel { get; set; } = 5;

    /// <summary>
    /// Og'irlik (gram)
    /// </summary>
    public decimal? Weight { get; set; }

    /// <summary>
    /// O'lchamlar (sm)
    /// </summary>
    public string? Dimensions { get; set; }

    /// <summary>
    /// Ishlab chiqaruvchi
    /// </summary>
    public string? Manufacturer { get; set; }

    /// <summary>
    /// Tartiblash uchun
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    // Navigation Properties

    /// <summary>
    /// Mahsulot kategoriyasi
    /// </summary>
    public virtual Category Category { get; set; } = null!;

    /// <summary>
    /// Buyurtma item'lari
    /// </summary>
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    /// <summary>
    /// Savat item'lari
    /// </summary>
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    /// <summary>
    /// Yoqtirishlar
    /// </summary>
    public virtual ICollection<ProductLike> ProductLikes { get; set; } = new List<ProductLike>();

    // Business Methods

    /// <summary>
    /// Zaxira kam qolganmi
    /// </summary>
    public bool IsLowStock => StockQuantity <= MinStockLevel;

    /// <summary>
    /// Zaxirada yo'qmi
    /// </summary>
    public bool IsOutOfStock => StockQuantity <= 0;

    /// <summary>
    /// Sotuvga tayyormi
    /// </summary>
    public bool IsAvailableForSale => IsActive && Status == ProductStatus.Active && !IsOutOfStock && !IsDeleted;

    /// <summary>
    /// Narxni o'zgartirish
    /// </summary>
    public void ChangePrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new ArgumentException("Narx manfiy bo'la olmaydi");

        Price = newPrice;
        MarkAsUpdated();
    }

    /// <summary>
    /// Zaxira qo'shish
    /// </summary>
    public void AddStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Qo'shiladigan miqdor musbat bo'lishi kerak");

        StockQuantity += quantity;

        // Agar zaxira qo'shilsa va status OutOfStock bo'lsa, Active qil
        if (Status == ProductStatus.OutOfStock && StockQuantity > 0)
        {
            Status = ProductStatus.Active;
        }

        MarkAsUpdated();
    }

    /// <summary>
    /// Zaxira kamaytirish (sotilganda)
    /// </summary>
    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Kamaytirilayotgan miqdor musbat bo'lishi kerak");

        if (quantity > StockQuantity)
            throw new InvalidOperationException("Zaxirada yetarli miqdor yo'q");

        StockQuantity -= quantity;

        // Agar zaxira tugasa, status'ni OutOfStock qil
        if (StockQuantity <= 0)
        {
            Status = ProductStatus.OutOfStock;
        }

        MarkAsUpdated();
    }

    /// <summary>
    /// Mahsulot faollashtirish
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        if (StockQuantity > 0)
        {
            Status = ProductStatus.Active;
        }
        MarkAsUpdated();
    }

    /// <summary>
    /// Mahsulot faolsizlashtirish
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        Status = ProductStatus.Inactive;
        MarkAsUpdated();
    }

    /// <summary>
    /// Nomini o'zgartirish
    /// </summary>
    public void ChangeName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Mahsulot nomi bo'sh bo'la olmaydi");

        Name = newName.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Tavsifni yangilash
    /// </summary>
    public void UpdateDescription(string? description)
    {
        Description = description?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// QR kodni o'zgartirish
    /// </summary>
    public void ChangeQrCode(string newQrCode)
    {
        if (string.IsNullOrWhiteSpace(newQrCode))
            throw new ArgumentException("QR kod bo'sh bo'la olmaydi");

        QrCode = newQrCode.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Kategoriyani o'zgartirish
    /// </summary>
    public void ChangeCategory(int categoryId)
    {
        if (categoryId <= 0)
            throw new ArgumentException("Kategoriya ID noto'g'ri");

        CategoryId = categoryId;
        MarkAsUpdated();
    }

    /// <summary>
    /// Rasm URL'ini yangilash
    /// </summary>
    public void UpdateImageUrl(string? imageUrl)
    {
        ImageUrl = imageUrl?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Minimal zaxira chegarasini o'rnatish
    /// </summary>
    public void SetMinStockLevel(int minLevel)
    {
        if (minLevel < 0)
            throw new ArgumentException("Minimal zaxira chegarasi manfiy bo'la olmaydi");

        MinStockLevel = minLevel;
        MarkAsUpdated();
    }

    /// <summary>
    /// Jami sotilgan miqdor
    /// </summary>
    public int GetTotalSoldQuantity()
    {
        return OrderItems.Where(oi => oi.Order != null &&
                                oi.Order.Status == OrderStatus.Delivered)
                        .Sum(oi => oi.Quantity);
    }

    /// <summary>
    /// Mahsulot yoqtirilgan miqdor
    /// </summary>
    public int GetLikesCount()
    {
        return ProductLikes.Count;
    }

    /// <summary>
    /// Mahsulotni qidirish uchun tekst
    /// </summary>
    public string GetSearchText()
    {
        var searchParts = new List<string> { Name };

        if (!string.IsNullOrEmpty(Description))
            searchParts.Add(Description);

        if (!string.IsNullOrEmpty(QrCode))
            searchParts.Add(QrCode);

        if (!string.IsNullOrEmpty(Manufacturer))
            searchParts.Add(Manufacturer);

        return string.Join(" ", searchParts).ToLowerInvariant();
    }
}