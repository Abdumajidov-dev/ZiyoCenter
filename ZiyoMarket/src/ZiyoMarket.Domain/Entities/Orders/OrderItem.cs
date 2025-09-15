using ZiyoMarket.Domain.Common;
using ZiyoMarket.Domain.Entities.Products;

namespace ZiyoMarket.Domain.Entities.Orders;

/// <summary>
/// Buyurtma item entity'si
/// </summary>
public class OrderItem : BaseEntity
{
    /// <summary>
    /// Buyurtma ID
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Mahsulot ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Miqdor
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Birlik narxi (buyurtma paytidagi narx)
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Shu item'ga qo'llangan chegirma
    /// </summary>
    public decimal DiscountApplied { get; set; } = 0;

    // Navigation Properties

    /// <summary>
    /// Buyurtma
    /// </summary>
    public virtual Order Order { get; set; } = null!;

    /// <summary>
    /// Mahsulot
    /// </summary>
    public virtual Product Product { get; set; } = null!;

    // Business Methods

    /// <summary>
    /// Jami narx (chegirmasiz)
    /// </summary>
    public decimal SubTotal => Quantity * UnitPrice;

    /// <summary>
    /// Yakuniy narx (chegirma bilan)
    /// </summary>
    public decimal TotalPrice => SubTotal - DiscountApplied;

    /// <summary>
    /// Chegirma foizi
    /// </summary>
    public decimal DiscountPercentage => SubTotal > 0 ? (DiscountApplied / SubTotal) * 100 : 0;

    /// <summary>
    /// Miqdorni yangilash
    /// </summary>
    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Miqdor musbat bo'lishi kerak");

        Quantity = newQuantity;
        MarkAsUpdated();
    }

    /// <summary>
    /// Birlik narxini yangilash
    /// </summary>
    public void UpdateUnitPrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new ArgumentException("Narx manfiy bo'la olmaydi");

        UnitPrice = newPrice;
        MarkAsUpdated();
    }

    /// <summary>
    /// Chegirmani qo'llash
    /// </summary>
    public void ApplyDiscount(decimal discountAmount)
    {
        if (discountAmount < 0)
            throw new ArgumentException("Chegirma summasi manfiy bo'la olmaydi");

        if (discountAmount > SubTotal)
            throw new ArgumentException("Chegirma summasi item narxidan ko'p bo'la olmaydi");

        DiscountApplied = discountAmount;
        MarkAsUpdated();
    }

    /// <summary>
    /// Chegirmani olib tashlash
    /// </summary>
    public void RemoveDiscount()
    {
        DiscountApplied = 0;
        MarkAsUpdated();
    }

    /// <summary>
    /// Item uchun chegirma qo'llash (foiz asosida)
    /// </summary>
    public void ApplyDiscountPercentage(decimal discountPercentage)
    {
        if (discountPercentage < 0 || discountPercentage > 100)
            throw new ArgumentException("Chegirma foizi 0 va 100 orasida bo'lishi kerak");

        var discountAmount = SubTotal * (discountPercentage / 100);
        ApplyDiscount(discountAmount);
    }

    /// <summary>
    /// Mahsulot ma'lumotlarini yangilash (narx o'zgarsa)
    /// </summary>
    public void SyncWithProduct()
    {
        if (Product != null)
        {
            // Faqat pending buyurtmalarda narxni yangilash mumkin
            if (Order?.Status == Enums.OrderStatus.Pending)
            {
                UnitPrice = Product.Price;
                MarkAsUpdated();
            }
        }
    }

    /// <summary>
    /// Item yaroqli holatdami
    /// </summary>
    public bool IsValid()
    {
        return Quantity > 0 &&
               UnitPrice >= 0 &&
               DiscountApplied >= 0 &&
               DiscountApplied <= SubTotal;
    }

    /// <summary>
    /// Mahsulot hali ham mavjudmi
    /// </summary>
    public bool IsProductAvailable()
    {
        return Product != null &&
               Product.IsAvailableForSale &&
               !Product.IsDeleted;
    }

    /// <summary>
    /// Zaxirada yetarli miqdor bormi
    /// </summary>
    public bool HasSufficientStock()
    {
        return Product != null && Product.StockQuantity >= Quantity;
    }

    /// <summary>
    /// Item'ni buyurtmaga qo'shish uchun validation
    /// </summary>
    public Result ValidateForOrder()
    {
        var errors = new List<string>();

        if (Quantity <= 0)
            errors.Add("Miqdor musbat bo'lishi kerak");

        if (UnitPrice < 0)
            errors.Add("Narx manfiy bo'la olmaydi");

        if (DiscountApplied < 0)
            errors.Add("Chegirma manfiy bo'la olmaydi");

        if (DiscountApplied > SubTotal)
            errors.Add("Chegirma summa item narxidan ko'p bo'la olmaydi");

        if (!IsProductAvailable())
            errors.Add($"Mahsulot '{Product?.Name}' sotuvda yo'q");

        if (!HasSufficientStock())
            errors.Add($"Mahsulot '{Product?.Name}' uchun zaxirada yetarli miqdor yo'q");

        return errors.Any() ? Result.Failure(errors) : Result.Success();
    }

    /// <summary>
    /// Item ma'lumotlarini string formatda
    /// </summary>
    public override string ToString()
    {
        var productName = Product?.Name ?? "Unknown Product";
        return $"{productName} x{Quantity} = {TotalPrice:C}";
    }
}