using System;
using System.Collections.Generic;
using ZiyoMarket.Service.Extensions;

namespace ZiyoMarket.Service.DTOs.Cashback;

/// <summary>
/// Cashback transaction DTO
/// </summary>
public class CashbackTransactionDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int? OrderId { get; set; }
    public string Type { get; set; } = string.Empty; // Earned, Used, Expired, Refunded
    public decimal Amount { get; set; }
    public string FormattedAmount => $"{Amount:N0} so'm";
    public decimal RemainingAmount { get; set; }
    public string FormattedRemainingAmount => $"{RemainingAmount:N0} so'm";
    public DateTime EarnedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public string? Description { get; set; }
    public bool IsExpired => ExpiresAt < DateTime.UtcNow && RemainingAmount > 0;
    public bool IsAvailable => RemainingAmount > 0 && ExpiresAt > DateTime.UtcNow;
    public int DaysUntilExpiry => (ExpiresAt - DateTime.UtcNow).Days;
}

/// <summary>
/// Cashback summary DTO
/// </summary>
public class CashbackSummaryDto
{
    public int CustomerId { get; set; }
    public decimal TotalBalance { get; set; }
    public string FormattedTotalBalance => $"{TotalBalance:N0} so'm";
    public decimal AvailableBalance { get; set; }
    public string FormattedAvailableBalance => $"{AvailableBalance:N0} so'm";
    public decimal ExpiredBalance { get; set; }
    public decimal UsedBalance { get; set; }
    public decimal ExpiringInNext7Days { get; set; }
    public int TotalTransactions { get; set; }
    public List<CashbackTransactionDto> RecentTransactions { get; set; } = new();
}

/// <summary>
/// Earn cashback DTO
/// </summary>
public class EarnCashbackDto
{
    public int CustomerId { get; set; }
    public int OrderId { get; set; }
    public decimal OrderAmount { get; set; }
    public decimal CashbackPercentage { get; set; } = 2.0m;
    public decimal CashbackAmount => (OrderAmount * CashbackPercentage / 100).RoundToTwoDecimals();
}

/// <summary>
/// Use cashback DTO
/// </summary>
public class UseCashbackDto
{
    public int CustomerId { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
}

/// <summary>
/// Cashback expiry notification DTO
/// </summary>
public class CashbackExpiryNotificationDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal ExpiringAmount { get; set; }
    public string FormattedExpiringAmount => $"{ExpiringAmount:N0} so'm";
    public DateTime ExpiryDate { get; set; }
    public int DaysUntilExpiry => (ExpiryDate - DateTime.UtcNow).Days;
}
