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
    public string? Email { get; set; }
    public decimal CashbackBalance { get; set; }
    public string FormattedCashbackBalance => $"{CashbackBalance:N0} so'm";
    public int OrdersCount { get; set; }
    public decimal TotalSpent { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Customer detail DTO
/// </summary>
public class CustomerDetailDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
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
    
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? Email { get; set; }
    
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
