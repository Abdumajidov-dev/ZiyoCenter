using ZiyoMarket.Domain.Common;
using ZiyoMarket.Domain.Entities.Users;

namespace ZiyoMarket.Domain.Entities.Products;

/// <summary>
/// Savat item entity'si
/// </summary>
public class CartItem : BaseEntity
{
    /// <summary>
    /// Mijoz ID
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Mahsulot ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Miqdor
    /// </summary>
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// Birlik narxi (savatga qo'shilgan paytdagi narx)
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Savatga qo'shilgan sana
    /// </summary>
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties

    /// <summary>
    /// Mijoz
    /// </summary>
    public virtual Customer Customer { get; set; } = null!;

    /// <summary>
    /// Mahsulot
    /// </summary>
    public virtual Product Product { get; set; } = null!;

    // Business Methods

    /// <summary>
    /// Jami narx (miqdor * birlik narx)
    /// </summary>
    public decimal TotalPrice => Quantity * UnitPrice;

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
    /// Miqdorni oshirish
    /// </summary>
    public void IncreaseQuantity(int amount = 1)
    {
        if (amount <= 0)
            throw new ArgumentException("Qo'shilayotgan miqdor musbat bo'lishi kerak");

        Quantity += amount;
        MarkAsUpdated();
    }

    /// <summary>
    /// Miqdorni kamaytirish
    /// </summary>
    public void DecreaseQuantity(int amount = 1)
    {
        if (amount <= 0)
            throw new ArgumentException("Kamaytirilayotgan miqdor musbat bo'lishi kerak");

        if (amount >= Quantity)
            throw new InvalidOperationException("Kamaytirilayotgan miqdor mavjud miqdordan ko'p");

        Quantity -= amount;
        MarkAsUpdated();
    }

    /// <summary>
    /// Birlik narxini yangilash (mahsulot narxi o'zgarganda)
    /// </summary>
    public void UpdateUnitPrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new ArgumentException("Narx manfiy bo'la olmaydi");

        UnitPrice = newPrice;
        MarkAsUpdated();
    }

    /// <summary>
    /// Savat item'i eskirganmi (24 soatdan ko'p)
    /// </summary>
    public bool IsExpired()
    {
        return DateTime.UtcNow.Subtract(AddedAt).TotalHours > 24;
    }

    /// <summary>
    /// Mahsulot hali ham mavjudmi va sotuvga tayyormi
    /// </summary>
    public bool IsProductAvailable()
    {
        return Product != null &&
               Product.IsAvailableForSale &&
               Product.StockQuantity >= Quantity;
    }

    /// <summary>
    /// Zaxirada yetarli miqdor bormi
    /// </summary>
    public bool HasSufficientStock()
    {
        return Product != null && Product.StockQuantity >= Quantity;
    }

    /// <summary>
    /// Narx o'zgarganmi
    /// </summary>
    public bool IsPriceChanged()
    {
        return Product != null && Product.Price != UnitPrice;
    }

    /// <summary>
    /// Savat item'ini buyurtmaga o'tkazish uchun tekshirish
    /// </summary>
    public Result ValidateForCheckout()
    {
        var errors = new List<string>();

        if (Product == null)
        {
            errors.Add("Mahsulot topilmadi");
            return Result.Failure(errors);
        }

        if (!Product.IsAvailableForSale)
        {
            errors.Add($"Mahsulot '{Product.Name}' hozir sotuvda yo'q");
        }

        if (!HasSufficientStock())
        {
            errors.Add($"Mahsulot '{Product.Name}' uchun zaxirada yetarli miqdor yo'q. Mavjud: {Product.StockQuantity}, Kerak: {Quantity}");
        }

        if (IsExpired())
        {
            errors.Add($"Savat item'i eskirgan. Iltimos, qayta qo'shing");
        }

        return errors.Any() ? Result.Failure(errors) : Result.Success();
    }
}