using ZiyoMarket.Domain.Common;
using ZiyoMarket.Domain.Entities.Users;

namespace ZiyoMarket.Domain.Entities.Orders;

/// <summary>
/// Buyurtma chegirmasi entity'si
/// </summary>
public class OrderDiscount : BaseEntity
{
    /// <summary>
    /// Buyurtma ID
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Chegirma sababi ID
    /// </summary>
    public int DiscountReasonId { get; set; }

    /// <summary>
    /// Chegirma summasi
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Qaysi sotuvchi bergan (agar offline bo'lsa)
    /// </summary>
    public int? AppliedBySellerId { get; set; }

    /// <summary>
    /// Izohlar
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Qo'llangan sana
    /// </summary>
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties

    /// <summary>
    /// Buyurtma
    /// </summary>
    public virtual Order Order { get; set; } = null!;

    /// <summary>
    /// Chegirma sababi
    /// </summary>
    public virtual DiscountReason DiscountReason { get; set; } = null!;

    /// <summary>
    /// Chegirmani bergan sotuvchi
    /// </summary>
    public virtual Seller? AppliedBySeller { get; set; }

    // Business Methods

    /// <summary>
    /// Chegirma foizi (buyurtma jami narxiga nisbatan)
    /// </summary>
    public decimal GetDiscountPercentage()
    {
        if (Order == null || Order.TotalPrice <= 0)
            return 0;

        return (DiscountAmount / Order.TotalPrice) * 100;
    }

    /// <summary>
    /// Chegirmani kim bergan
    /// </summary>
    public string GetAppliedByInfo()
    {
        if (AppliedBySeller != null)
            return $"Sotuvchi: {AppliedBySeller.FullName}";

        return "Tizim tomonidan";
    }

    /// <summary>
    /// Chegirma summasi yaroqlimi
    /// </summary>
    public bool IsValidAmount()
    {
        return DiscountAmount > 0 &&
               Order != null &&
               DiscountAmount <= Order.TotalPrice;
    }

    /// <summary>
    /// Chegirmani bekor qilish
    /// </summary>
    public void Cancel(string reason)
    {
        Notes = $"Bekor qilindi: {reason}";
        Delete();
    }

    /// <summary>
    /// Chegirma summani yangilash
    /// </summary>
    public void UpdateAmount(decimal newAmount)
    {
        if (newAmount < 0)
            throw new ArgumentException("Chegirma summasi manfiy bo'la olmaydi");

        if (Order != null && newAmount > Order.TotalPrice)
            throw new ArgumentException("Chegirma summasi buyurtma narxidan ko'p bo'la olmaydi");

        DiscountAmount = newAmount;
        MarkAsUpdated();
    }

    /// <summary>
    /// Izohni yangilash
    /// </summary>
    public void UpdateNotes(string? notes)
    {
        Notes = notes?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Chegirma ma'lumotlarini to'liq formatda
    /// </summary>
    public string GetFullDescription()
    {
        var description = $"{DiscountReason?.Name ?? "Unknown"}: {DiscountAmount:C}";

        if (!string.IsNullOrEmpty(Notes))
            description += $" ({Notes})";

        return description;
    }

    /// <summary>
    /// Chegirma validatsiya qilish
    /// </summary>
    public Result Validate()
    {
        var errors = new List<string>();

        if (DiscountAmount <= 0)
            errors.Add("Chegirma summasi musbat bo'lishi kerak");

        if (Order == null)
            errors.Add("Buyurtma topilmadi");
        else if (DiscountAmount > Order.TotalPrice)
            errors.Add("Chegirma summasi buyurtma narxidan ko'p bo'la olmaydi");

        if (DiscountReason == null)
            errors.Add("Chegirma sababi ko'rsatilishi kerak");

        return errors.Any() ? Result.Failure(errors) : Result.Success();
    }

    /// <summary>
    /// Chegirma audit ma'lumotlari
    /// </summary>
    public string GetAuditInfo()
    {
        var info = $"Sana: {AppliedAt:dd.MM.yyyy HH:mm}";

        if (AppliedBySeller != null)
            info += $", Sotuvchi: {AppliedBySeller.FullName}";

        if (!string.IsNullOrEmpty(Notes))
            info += $", Izoh: {Notes}";

        return info;
    }
}