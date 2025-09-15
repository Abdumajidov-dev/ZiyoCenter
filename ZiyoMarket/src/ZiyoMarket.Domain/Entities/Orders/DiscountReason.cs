using ZiyoMarket.Domain.Common;

namespace ZiyoMarket.Domain.Entities.Orders;

/// <summary>
/// Chegirma sabablari entity'si
/// </summary>
public class DiscountReason : BaseAuditableEntity
{
    /// <summary>
    /// Chegirma sababi nomi
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tavsif
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Faolmi
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Maksimal chegirma foizi (agar cheklangan bo'lsa)
    /// </summary>
    public decimal? MaxDiscountPercentage { get; set; }

    /// <summary>
    /// Maksimal chegirma summasi (agar cheklangan bo'lsa)
    /// </summary>
    public decimal? MaxDiscountAmount { get; set; }

    /// <summary>
    /// Faqat sotuvchilar ishlatishi mumkinmi
    /// </summary>
    public bool IsSellerOnly { get; set; } = false;

    /// <summary>
    /// Tartiblash uchun
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    // Navigation Properties

    /// <summary>
    /// Bu sabab bilan berilgan chegirmalar
    /// </summary>
    public virtual ICollection<OrderDiscount> OrderDiscounts { get; set; } = new List<OrderDiscount>();

    // Business Methods

    /// <summary>
    /// Chegirma sababi faollashtirish
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Chegirma sababi faolsizlashtirish
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    /// <summary>
    /// Nomini o'zgartirish
    /// </summary>
    public void ChangeName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Chegirma sababi nomi bo'sh bo'la olmaydi");

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
    /// Maksimal chegirma foizini o'rnatish
    /// </summary>
    public void SetMaxDiscountPercentage(decimal? percentage)
    {
        if (percentage.HasValue && (percentage < 0 || percentage > 100))
            throw new ArgumentException("Chegirma foizi 0 va 100 orasida bo'lishi kerak");

        MaxDiscountPercentage = percentage;
        MarkAsUpdated();
    }

    /// <summary>
    /// Maksimal chegirma summasini o'rnatish
    /// </summary>
    public void SetMaxDiscountAmount(decimal? amount)
    {
        if (amount.HasValue && amount < 0)
            throw new ArgumentException("Maksimal chegirma summasi manfiy bo'la olmaydi");

        MaxDiscountAmount = amount;
        MarkAsUpdated();
    }

    /// <summary>
    /// Sotuvchi huquqini o'rnatish
    /// </summary>
    public void SetSellerOnly(bool isSellerOnly)
    {
        IsSellerOnly = isSellerOnly;
        MarkAsUpdated();
    }

    /// <summary>
    /// Tartiblash raqamini o'rnatish
    /// </summary>
    public void SetDisplayOrder(int order)
    {
        DisplayOrder = order;
        MarkAsUpdated();
    }

    /// <summary>
    /// Chegirma summasi ruxsat etiladiganmi
    /// </summary>
    public bool IsDiscountAllowed(decimal discountAmount, decimal orderTotal)
    {
        if (!IsActive)
            return false;

        // Maksimal summa tekshiruvi
        if (MaxDiscountAmount.HasValue && discountAmount > MaxDiscountAmount.Value)
            return false;

        // Maksimal foiz tekshiruvi
        if (MaxDiscountPercentage.HasValue && orderTotal > 0)
        {
            var discountPercentage = (discountAmount / orderTotal) * 100;
            if (discountPercentage > MaxDiscountPercentage.Value)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Foydalanuvchi uchun mavjudmi (sotuvchi yoki mijoz)
    /// </summary>
    public bool IsAvailableForUser(bool isSellerUser)
    {
        if (!IsActive)
            return false;

        if (IsSellerOnly && !isSellerUser)
            return false;

        return true;
    }

    /// <summary>
    /// Maksimal ruxsat etilgan chegirma summasini hisoblash
    /// </summary>
    public decimal GetMaxAllowedDiscount(decimal orderTotal)
    {
        var maxByAmount = MaxDiscountAmount ?? decimal.MaxValue;
        var maxByPercentage = decimal.MaxValue;

        if (MaxDiscountPercentage.HasValue && orderTotal > 0)
        {
            maxByPercentage = orderTotal * (MaxDiscountPercentage.Value / 100);
        }

        return Math.Min(maxByAmount, maxByPercentage);
    }

    /// <summary>
    /// Bu sabab bilan jami berilgan chegirmalar summasi
    /// </summary>
    public decimal GetTotalDiscountsGiven()
    {
        return OrderDiscounts
            .Where(od => !od.IsDeleted)
            .Sum(od => od.DiscountAmount);
    }

    /// <summary>
    /// Bu sabab qancha marta ishlatilgan
    /// </summary>
    public int GetUsageCount()
    {
        return OrderDiscounts.Count(od => !od.IsDeleted);
    }

    /// <summary>
    /// Oxirgi marta qachon ishlatilgan
    /// </summary>
    public DateTime? GetLastUsedDate()
    {
        return OrderDiscounts
            .Where(od => !od.IsDeleted)
            .Max(od => (DateTime?)od.AppliedAt);
    }

    /// <summary>
    /// Chegirma sababi o'chirilishi mumkinmi
    /// </summary>
    public bool CanBeDeleted()
    {
        return !OrderDiscounts.Any(od => !od.IsDeleted);
    }

    /// <summary>
    /// Chegirma sababi validatsiya
    /// </summary>
    public Result Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Chegirma sababi nomi bo'sh bo'la olmaydi");

        if (MaxDiscountPercentage.HasValue &&
            (MaxDiscountPercentage < 0 || MaxDiscountPercentage > 100))
            errors.Add("Maksimal chegirma foizi 0 va 100 orasida bo'lishi kerak");

        if (MaxDiscountAmount.HasValue && MaxDiscountAmount < 0)
            errors.Add("Maksimal chegirma summasi manfiy bo'la olmaydi");

        return errors.Any() ? Result.Failure(errors) : Result.Success();
    }

    /// <summary>
    /// Chegirma sababi ma'lumotlarini to'liq formatda
    /// </summary>
    public string GetFullInfo()
    {
        var info = Name;

        var constraints = new List<string>();

        if (MaxDiscountPercentage.HasValue)
            constraints.Add($"Maks {MaxDiscountPercentage}%");

        if (MaxDiscountAmount.HasValue)
            constraints.Add($"Maks {MaxDiscountAmount:C}");

        if (IsSellerOnly)
            constraints.Add("Faqat sotuvchilar");

        if (constraints.Any())
            info += $" ({string.Join(", ", constraints)})";

        return info;
    }
}