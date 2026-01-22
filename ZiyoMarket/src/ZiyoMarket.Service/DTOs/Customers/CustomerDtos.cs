using System;
using System.ComponentModel.DataAnnotations;
using ZiyoMarket.Service.DTOs.Common;

namespace ZiyoMarket.Service.DTOs.Customers;

/// <summary>
/// Customer list DTO
/// </summary>
public class CustomerListDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Phone { get; set; } = string.Empty;
    public decimal CashbackBalance { get; set; }
    public string FormattedCashbackBalance => $"{CashbackBalance:N0} so'm";
    public int OrdersCount { get; set; }
    public decimal TotalSpent { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalOrders{get;set;}
}
/// <summary>
/// Yangi mijoz yaratish uchun DTO
/// </summary>
public class CreateCustomerDto
{
    /// <summary>
    /// Ism
    /// </summary>
    [Required(ErrorMessage = "Ism majburiy")]
    [MaxLength(100, ErrorMessage = "Ism uzunligi 100 belgidan oshmasligi kerak")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Familiya
    /// </summary>
    [Required(ErrorMessage = "Familiya majburiy")]
    [MaxLength(100, ErrorMessage = "Familiya uzunligi 100 belgidan oshmasligi kerak")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Telefon raqami (unique)
    /// </summary>
    [Required(ErrorMessage = "Telefon raqami majburiy")]
    [Phone(ErrorMessage = "Telefon raqami noto�g�ri formatda kiritilgan")]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Parol (hash emas, foydalanuvchi tomonidan kiritiladigan)
    /// </summary>
    [Required(ErrorMessage = "Parol majburiy")]
    [MinLength(6, ErrorMessage = "Parol uzunligi kamida 6 ta belgi bo�lishi kerak")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Manzil (ixtiyoriy)
    /// </summary>
    [MaxLength(500, ErrorMessage = "Manzil 500 belgidan oshmasligi kerak")]
    public string? Address { get; set; }

    /// <summary>
    /// FCM token (push notification uchun, ixtiyoriy)
    /// </summary>
    public string? FcmToken { get; set; }

    /// <summary>
    /// Boshlang�ich cashback (default: 0)
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Cashback manfiy bo�lishi mumkin emas")]
    public decimal CashbackBalance { get; set; } = 0;

    /// <summary>
    /// Mijoz faolmi yoki yo�qmi (default: true)
    /// </summary>
    public bool IsActive { get; set; } = true;
} 
///<summary>
/// Customer detail DTO
/// </summary>
public class CustomerDetailDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public decimal CashbackBalance { get; set; }
    public string FormattedCashbackBalance => $"{CashbackBalance:N0} so'm";
    public bool IsActive { get; set; }
    
    // Statistics
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal TotalCashbackEarned { get; set; }
    public decimal TotalCashbackUsed { get; set; }
    public DateTime? LastOrderDate { get; set; }
    
    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Update customer DTO
/// </summary>
public class UpdateCustomerDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int Id { get; set; }

    [Required(ErrorMessage = "First name is required")]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Address { get; set; }
}

/// <summary>
/// Customer filter request
/// </summary>
public class CustomerFilterRequest : BaseFilterRequest
{
    public bool? IsActive { get; set; }
    public decimal? MinCashbackBalance { get; set; }
    public decimal? MaxCashbackBalance { get; set; }
    public int? MinOrders { get; set; }
    public int? MaxOrders { get; set; }
}

/// <summary>
/// Customer statistics DTO
/// </summary>
public class CustomerStatisticsDto
{
    public int TotalCustomers { get; set; }
    public int ActiveCustomers { get; set; }
    public int NewCustomersThisMonth { get; set; }
    public decimal AverageCashbackBalance { get; set; }
    public decimal TotalCashbackBalance { get; set; }
}
public class CustomerPersonalStatsDto // bitta mijoz uchun
{
    public int CustomerId { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal TotalCashback { get; set; }
    public decimal AverageOrderValue { get; set; }
    public DateTime? LastOrderDate { get; set; }
}
public class CustomerProfileDto
{
    public int Id { get; set; }

    // Shaxsiy ma�lumotlar
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }

    // Cashback ma�lumotlari
    public decimal CashbackBalance { get; set; }
    public string FormattedCashbackBalance => $"{CashbackBalance:N0} so'm";

    // Faollik holati
    public bool IsActive { get; set; }

    // Statistik ma�lumotlar
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
/// <summary>
/// Eng ko�p xarid qilgan mijozlar ro�yxati uchun DTO
/// </summary>
public class TopCustomerDto
{
    /// <summary>
    /// Mijoz ID raqami
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Mijoz to�liq ismi (FirstName + LastName)
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Mijozning jami buyurtmalari soni
    /// </summary>
    public int TotalOrders { get; set; }

    /// <summary>
    /// Mijoz tomonidan sarflangan jami summa
    /// </summary>
    public decimal TotalSpent { get; set; }

    /// <summary>
    /// Formatlangan summa (masalan: 150 000 so'm)
    /// </summary>
    public string FormattedTotalSpent => $"{TotalSpent:N0} so'm";
}