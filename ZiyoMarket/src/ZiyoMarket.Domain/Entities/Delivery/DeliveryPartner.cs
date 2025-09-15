using ZiyoMarket.Domain.Common;
using ZiyoMarket.Domain.Enums;

namespace ZiyoMarket.Domain.Entities.Delivery;

/// <summary>
/// Yetkazib berish hamkori entity'si
/// </summary>
public class DeliveryPartner : BaseAuditableEntity
{
    /// <summary>
    /// Hamkor nomi
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Telefon raqami
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Email manzili
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Yetkazib berish turi
    /// </summary>
    public string DeliveryType { get; set; } = string.Empty; // "Postal", "Courier", "Express"

    /// <summary>
    /// Yetkazib berish narxi
    /// </summary>
    public decimal PricePerDelivery { get; set; }

    /// <summary>
    /// Taxminiy yetkazib berish vaqti (kun)
    /// </summary>
    public int EstimatedDays { get; set; } = 1;

    /// <summary>
    /// Maksimal og'irlik (kg)
    /// </summary>
    public decimal? MaxWeight { get; set; }

    /// <summary>
    /// Xizmat ko'rsatadigan hududlar
    /// </summary>
    public string? ServiceAreas { get; set; } // JSON array of areas

    /// <summary>
    /// Faolmi
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Tartiblash uchun
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// API URL (agar tracking API bor bo'lsa)
    /// </summary>
    public string? ApiUrl { get; set; }

    /// <summary>
    /// API kaliti
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Izohlar
    /// </summary>
    public string? Notes { get; set; }

    // Navigation Properties

    /// <summary>
    /// Bu hamkor orqali yetkazilgan buyurtmalar
    /// </summary>
    public virtual ICollection<OrderDelivery> OrderDeliveries { get; set; } = new List<OrderDelivery>();

    // Business Methods

    /// <summary>
    /// Express yetkazib berish hamkorimi
    /// </summary>
    public bool IsExpressDelivery => DeliveryType.Contains("Express", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Pochta hamkorimi
    /// </summary>
    public bool IsPostalService => DeliveryType.Contains("Postal", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Hamkorni faollashtirish
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Hamkorni faolsizlashtirish
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
            throw new ArgumentException("Hamkor nomi bo'sh bo'la olmaydi");

        Name = newName.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Narxni yangilash
    /// </summary>
    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new ArgumentException("Narx manfiy bo'la olmaydi");

        PricePerDelivery = newPrice;
        MarkAsUpdated();
    }

    /// <summary>
    /// Yetkazib berish vaqtini yangilash
    /// </summary>
    public void UpdateEstimatedDays(int days)
    {
        if (days < 1)
            throw new ArgumentException("Yetkazib berish vaqti kamida 1 kun bo'lishi kerak");

        EstimatedDays = days;
        MarkAsUpdated();
    }

    /// <summary>
    /// Telefon raqamini yangilash
    /// </summary>
    public void UpdatePhone(string? phone)
    {
        Phone = phone?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Email manzilini yangilash
    /// </summary>
    public void UpdateEmail(string? email)
    {
        Email = email?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Maksimal og'irlikni o'rnatish
    /// </summary>
    public void SetMaxWeight(decimal? maxWeight)
    {
        if (maxWeight.HasValue && maxWeight <= 0)
            throw new ArgumentException("Maksimal og'irlik musbat bo'lishi kerak");

        MaxWeight = maxWeight;
        MarkAsUpdated();
    }

    /// <summary>
    /// Xizmat hududlarini yangilash
    /// </summary>
    public void UpdateServiceAreas(string? serviceAreas)
    {
        ServiceAreas = serviceAreas?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// API ma'lumotlarini yangilash
    /// </summary>
    public void UpdateApiInfo(string? apiUrl, string? apiKey)
    {
        ApiUrl = apiUrl?.Trim();
        ApiKey = apiKey?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Og'irlik uchun mos keladimi
    /// </summary>
    public bool CanHandleWeight(decimal weight)
    {
        return !MaxWeight.HasValue || weight <= MaxWeight.Value;
    }

    /// <summary>
    /// Hududga xizmat ko'rsatadimi
    /// </summary>
    public bool ServesArea(string area)
    {
        if (string.IsNullOrEmpty(ServiceAreas))
            return true; // Agar hududlar ko'rsatilmagan bo'lsa, hamma joyga xizmat

        // Simple check - real implementation JSON parse qilishi kerak
        return ServiceAreas.Contains(area, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Yetkazib berish uchun mavjudmi
    /// </summary>
    public bool IsAvailableForDelivery(decimal? weight = null, string? area = null)
    {
        if (!IsActive)
            return false;

        if (weight.HasValue && !CanHandleWeight(weight.Value))
            return false;

        if (!string.IsNullOrEmpty(area) && !ServesArea(area))
            return false;

        return true;
    }

    /// <summary>
    /// Jami yetkazilgan buyurtmalar soni
    /// </summary>
    public int GetTotalDeliveries()
    {
        return OrderDeliveries.Count(od => od.DeliveryStatus == DeliveryStatus.Delivered);
    }

    /// <summary>
    /// Muvaffaqiyatli yetkazib berish foizi
    /// </summary>
    public decimal GetSuccessRate()
    {
        var totalOrders = OrderDeliveries.Count();
        if (totalOrders == 0) return 100;

        var successfulOrders = OrderDeliveries.Count(od => od.DeliveryStatus == DeliveryStatus.Delivered);
        return (decimal)successfulOrders / totalOrders * 100;
    }

    /// <summary>
    /// O'rtacha yetkazib berish vaqti
    /// </summary>
    public double GetAverageDeliveryDays()
    {
        var deliveredOrders = OrderDeliveries
            .Where(od => od.DeliveryStatus == DeliveryStatus.Delivered &&
                        od.DeliveredAt.HasValue)
            .ToList();

        if (!deliveredOrders.Any()) return EstimatedDays;

        var totalDays = deliveredOrders
            .Sum(od => od.DeliveredAt!.Value.Subtract(od.AssignedAt).TotalDays);

        return totalDays / deliveredOrders.Count;
    }

    /// <summary>
    /// Hamkor o'chirilishi mumkinmi
    /// </summary>
    public bool CanBeDeleted()
    {
        return !OrderDeliveries.Any();
    }

    /// <summary>
    /// Hamkor ma'lumotlarini to'liq formatda
    /// </summary>
    public string GetFullInfo()
    {
        var info = $"{Name} - {DeliveryType}";
        info += $" ({PricePerDelivery:C}, {EstimatedDays} kun)";

        if (!IsActive)
            info += " [NOFAOL]";

        return info;
    }

    /// <summary>
    /// Hamkor validatsiya
    /// </summary>
    public Result Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Hamkor nomi bo'sh bo'la olmaydi");

        if (string.IsNullOrWhiteSpace(DeliveryType))
            errors.Add("Yetkazib berish turi ko'rsatilishi kerak");

        if (PricePerDelivery < 0)
            errors.Add("Narx manfiy bo'la olmaydi");

        if (EstimatedDays < 1)
            errors.Add("Yetkazib berish vaqti kamida 1 kun bo'lishi kerak");

        if (MaxWeight.HasValue && MaxWeight <= 0)
            errors.Add("Maksimal og'irlik musbat bo'lishi kerak");

        return errors.Any() ? Result.Failure(errors) : Result.Success();
    }
}