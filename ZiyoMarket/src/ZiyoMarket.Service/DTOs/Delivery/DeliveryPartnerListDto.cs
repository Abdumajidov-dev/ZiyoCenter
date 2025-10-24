using System.ComponentModel.DataAnnotations;
using ZiyoMarket.Domain.Enums;

namespace ZiyoMarket.Service.DTOs.Delivery;

// ==============================
// 🔹 DELIVERY PARTNER DTO'LAR
// ==============================

/// <summary>
/// Yetkazib berish hamkori ro‘yxat DTO
/// </summary>
public class DeliveryPartnerListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DeliveryType { get; set; } = string.Empty;
    public decimal PricePerDelivery { get; set; }
    public int EstimatedDays { get; set; }
    public bool IsActive { get; set; }
    public int TotalDeliveries { get; set; }
    public decimal SuccessRate { get; set; }
}
public class DeliveryPartnerCreateDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    [Required, MaxLength(50)]
    public string DeliveryType { get; set; } = "Courier";
    [Range(0, double.MaxValue)]
    public decimal PricePerDelivery { get; set; }
    [Range(1, int.MaxValue)]
    public int EstimatedDays { get; set; } = 1;
    [Range(0.1, double.MaxValue)]
    public decimal? MaxWeight { get; set; }
    [Phone]
    public string? Phone { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
    public string? ServiceAreas { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;
    public string? ApiUrl { get; set; }
    public string? ApiKey { get; set; }
    [MaxLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// Yetkazib berish hamkori batafsil DTO
/// </summary>
public class DeliveryPartnerDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string DeliveryType { get; set; } = string.Empty;
    public decimal PricePerDelivery { get; set; }
    public int EstimatedDays { get; set; }
    public decimal? MaxWeight { get; set; }
    public string? ServiceAreas { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public string? ApiUrl { get; set; }
    public string? ApiKey { get; set; }
    public string? Notes { get; set; }
    public int TotalDeliveries { get; set; }
    public decimal SuccessRate { get; set; }
    public double AverageDeliveryDays { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
public class DeliveryPartnerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DeliveryType { get; set; } = string.Empty;
    public decimal PricePerDelivery { get; set; }
    public int EstimatedDays { get; set; }
    public bool IsActive { get; set; }
    public int TotalDeliveries { get; set; }
    public decimal SuccessRate { get; set; }
}


public class DeliveryPartnerUpdateDto : DeliveryPartnerCreateDto
{
    [Required]
    public int Id { get; set; }
}

public class DeliveryPartnerStatsDto
{
    public int PartnerId { get; set; }
    public string PartnerName { get; set; } = string.Empty;
    public int TotalDeliveries { get; set; }
    public int SuccessfulDeliveries { get; set; }
    public int FailedDeliveries { get; set; }
    public int InProgressDeliveries { get; set; }
    public decimal SuccessRate { get; set; }
    public double AverageDeliveryDays { get; set; }
}

public class OrderDeliveryDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int DeliveryPartnerId { get; set; }
    public string DeliveryPartnerName { get; set; } = string.Empty;
    public DeliveryStatus DeliveryStatus { get; set; }
    public decimal DeliveryFee { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public bool IsDelayed { get; set; }
}

public class OrderDeliveryCreateDto
{
    [Required, Range(1, int.MaxValue)]
    public int OrderId { get; set; }

    [Required, Range(1, int.MaxValue)]
    public int DeliveryPartnerId { get; set; }

    [Required, MaxLength(500)]
    public string DeliveryAddress { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal DeliveryFee { get; set; }

    public string? ReceiverName { get; set; }
    public string? ReceiverPhone { get; set; }
    public string? Notes { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
}
