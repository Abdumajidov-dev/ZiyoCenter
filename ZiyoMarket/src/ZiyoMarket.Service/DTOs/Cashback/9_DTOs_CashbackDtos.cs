namespace ZiyoMarket.Service.DTOs.Cashback;

// ===================================================================
// CASHBACK TRANSACTION DTO
// ===================================================================

/// <summary>
/// Cashback transaction ma'lumoti uchun DTO
/// </summary>
public class CashbackTransactionDto
{
    /// <summary>
    /// Transaction ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Mijoz ID
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Buyurtma ID (ixtiyoriy)
    /// </summary>
    public int? OrderId { get; set; }

    /// <summary>
    /// Transaction turi: Earned, Used, Expired, Refunded
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Cashback miqdori
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Qolgan (ishlatilmagan) miqdor
    /// </summary>
    public decimal RemainingAmount { get; set; }

    /// <summary>
    /// Yig'ilgan sana (string format)
    /// </summary>
    public string EarnedAt { get; set; } = string.Empty;

    /// <summary>
    /// Amal qilish muddati (string format)
    /// </summary>
    public string ExpiresAt { get; set; } = string.Empty;

    /// <summary>
    /// Ishlatilgan sana (ixtiyoriy, string format)
    /// </summary>
    public string? UsedAt { get; set; }

    /// <summary>
    /// Tavsif
    /// </summary>
    public string? Description { get; set; }

    // Computed Properties

    /// <summary>
    /// Formatlangan miqdor
    /// </summary>
    public string FormattedAmount => $"{Math.Abs(Amount):N0} so'm";

    /// <summary>
    /// Formatlangan qolgan miqdor
    /// </summary>
    public string FormattedRemainingAmount => $"{RemainingAmount:N0} so'm";

    /// <summary>
    /// Muddati o'tganmi?
    /// </summary>
    public bool IsExpired
    {
        get
        {
            if (string.IsNullOrEmpty(ExpiresAt)) return false;
            return DateTime.Parse(ExpiresAt) < DateTime.UtcNow && RemainingAmount > 0;
        }
    }

    /// <summary>
    /// Mavjudmi (ishlatish mumkinmi)?
    /// </summary>
    public bool IsAvailable
    {
        get
        {
            if (string.IsNullOrEmpty(ExpiresAt)) return false;
            return RemainingAmount > 0 && DateTime.Parse(ExpiresAt) > DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Muddati tugashiga necha kun qoldi
    /// </summary>
    public int DaysUntilExpiry
    {
        get
        {
            if (string.IsNullOrEmpty(ExpiresAt)) return 0;
            var days = (DateTime.Parse(ExpiresAt) - DateTime.UtcNow).Days;
            return days > 0 ? days : 0;
        }
    }

    /// <summary>
    /// Cashback rang kodi (UI uchun)
    /// </summary>
    public string ColorCode
    {
        get
        {
            return Type.ToLower() switch
            {
                "earned" => "green",
                "used" => "blue",
                "expired" => "red",
                "refunded" => "orange",
                _ => "gray"
            };
        }
    }

    /// <summary>
    /// Cashback icon (UI uchun)
    /// </summary>
    public string Icon
    {
        get
        {
            return Type.ToLower() switch
            {
                "earned" => "?",
                "used" => "?",
                "expired" => "?",
                "refunded" => "??",
                _ => "??"
            };
        }
    }
}

// ===================================================================
// CASHBACK SUMMARY DTO
// ===================================================================

/// <summary>
/// Cashback umumiy ma'lumoti uchun DTO
/// </summary>
public class CashbackSummaryDto
{
    /// <summary>
    /// Jami balans (barcha cashback)
    /// </summary>
    public decimal TotalBalance { get; set; }

    /// <summary>
    /// Jami yig'ilgan cashback (Earned)
    /// </summary>
    public decimal TotalEarned { get; set; }

    /// <summary>
    /// Jami ishlatilgan cashback (Used)
    /// </summary>
    public decimal TotalUsed { get; set; }

    /// <summary>
    /// Jami muddati o'tgan cashback (Expired)
    /// </summary>
    public decimal TotalExpired { get; set; }

    /// <summary>
    /// 7 kun ichida muddati tugaydigan cashback
    /// </summary>
    public decimal ExpiringIn7Days { get; set; }

    // Computed Properties

    /// <summary>
    /// Formatlangan jami balans
    /// </summary>
    public string FormattedTotalBalance => $"{TotalBalance:N0} so'm";

    /// <summary>
    /// Formatlangan yig'ilgan
    /// </summary>
    public string FormattedTotalEarned => $"{TotalEarned:N0} so'm";

    /// <summary>
    /// Formatlangan ishlatilgan
    /// </summary>
    public string FormattedTotalUsed => $"{TotalUsed:N0} so'm";

    /// <summary>
    /// Formatlangan muddati o'tgan
    /// </summary>
    public string FormattedTotalExpired => $"{TotalExpired:N0} so'm";

    /// <summary>
    /// Formatlangan tugayotgan cashback
    /// </summary>
    public string FormattedExpiringIn7Days => $"{ExpiringIn7Days:N0} so'm";

    /// <summary>
    /// Foiz (ishlatilgan/yig'ilgan)
    /// </summary>
    public decimal UsagePercentage => TotalEarned > 0 ? (TotalUsed / TotalEarned) * 100 : 0;

    /// <summary>
    /// Muddati o'tish foizi
    /// </summary>
    public decimal ExpiryPercentage => TotalEarned > 0 ? (TotalExpired / TotalEarned) * 100 : 0;

    /// <summary>
    /// Samaradorlik darajasi
    /// </summary>
    public string EfficiencyLevel
    {
        get
        {
            if (ExpiryPercentage > 30) return "? Poor - Too much expired";
            if (UsagePercentage > 70) return "? Excellent - Active user";
            if (UsagePercentage > 40) return "?? Good";
            return "?? Average";
        }
    }
}

// ===================================================================
// EARN CASHBACK DTO
// ===================================================================

/// <summary>
/// Cashback yig'ish uchun DTO
/// </summary>
public class EarnCashbackDto
{
    /// <summary>
    /// Mijoz ID
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Buyurtma ID
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Buyurtma summasi
    /// </summary>
    public decimal OrderAmount { get; set; }

    /// <summary>
    /// Cashback foizi (default: 2%)
    /// </summary>
    public decimal CashbackPercentage { get; set; } = 2.0m;

    /// <summary>
    /// Hisoblanadigan cashback miqdori
    /// </summary>
    public decimal CashbackAmount => Math.Round(OrderAmount * CashbackPercentage / 100, 2);

    /// <summary>
    /// Amal qilish muddati (default: 30 kun)
    /// </summary>
    public int ExpiryDays { get; set; } = 30;

    /// <summary>
    /// Formatlangan cashback
    /// </summary>
    public string FormattedCashbackAmount => $"{CashbackAmount:N0} so'm";
}

// ===================================================================
// USE CASHBACK DTO
// ===================================================================

/// <summary>
/// Cashback ishlatish uchun DTO
/// </summary>
public class UseCashbackDto
{
    /// <summary>
    /// Mijoz ID
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Buyurtma ID
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Ishlatilayotgan miqdor
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Formatlangan miqdor
    /// </summary>
    public string FormattedAmount => $"{Amount:N0} so'm";
}

// ===================================================================
// CASHBACK EXPIRY NOTIFICATION DTO
// ===================================================================

/// <summary>
/// Cashback muddati tugayotganligi haqida xabar uchun DTO
/// </summary>
public class CashbackExpiryNotificationDto
{
    /// <summary>
    /// Mijoz ID
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Mijoz ismi
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Muddati tugayotgan cashback miqdori
    /// </summary>
    public decimal ExpiringAmount { get; set; }

    /// <summary>
    /// Muddati tugash sanasi
    /// </summary>
    public DateTime ExpiryDate { get; set; }

    /// <summary>
    /// Formatlangan miqdor
    /// </summary>
    public string FormattedExpiringAmount => $"{ExpiringAmount:N0} so'm";

    /// <summary>
    /// Muddati tugashiga qolgan kunlar
    /// </summary>
    public int DaysUntilExpiry => (ExpiryDate - DateTime.UtcNow).Days;

    /// <summary>
    /// Xabar matni (notification uchun)
    /// </summary>
    public string NotificationMessage =>
        $"?? Sizning {FormattedExpiringAmount} cashback'ingiz {DaysUntilExpiry} kundan keyin muddati tugaydi. Iltimos, tezroq foydalaning!";

    /// <summary>
    /// Urgentlik darajasi
    /// </summary>
    public string UrgencyLevel
    {
        get
        {
            if (DaysUntilExpiry <= 1) return "?? Critical";
            if (DaysUntilExpiry <= 3) return "?? High";
            if (DaysUntilExpiry <= 7) return "?? Medium";
            return "?? Low";
        }
    }
}

// ===================================================================
// CASHBACK HISTORY FILTER DTO
// ===================================================================

/// <summary>
/// Cashback tarixini filtrlash uchun DTO
/// </summary>
public class CashbackHistoryFilterDto
{
    /// <summary>
    /// Mijoz ID
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Transaction turi filtri
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Boshlanish sanasi
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Tugash sanasi
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Sahifa raqami
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Sahifa hajmi
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Skip
    /// </summary>
    public int Skip => (PageNumber - 1) * PageSize;
}

// ===================================================================
// CASHBACK STATISTICS DTO
// ===================================================================

/// <summary>
/// Cashback statistikasi uchun DTO
/// </summary>
public class CashbackStatisticsDto
{
    /// <summary>
    /// Jami mijozlar soni
    /// </summary>
    public int TotalCustomers { get; set; }

    /// <summary>
    /// Cashback'i bor mijozlar
    /// </summary>
    public int CustomersWithCashback { get; set; }

    /// <summary>
    /// Jami yig'ilgan cashback (tizim bo'yicha)
    /// </summary>
    public decimal TotalEarnedSystemWide { get; set; }

    /// <summary>
    /// Jami ishlatilgan cashback
    /// </summary>
    public decimal TotalUsedSystemWide { get; set; }

    /// <summary>
    /// Jami muddati o'tgan cashback
    /// </summary>
    public decimal TotalExpiredSystemWide { get; set; }

    /// <summary>
    /// Hozirgi jami balans (tizim bo'yicha)
    /// </summary>
    public decimal CurrentTotalBalance { get; set; }

    /// <summary>
    /// O'rtacha mijoz balansasi
    /// </summary>
    public decimal AverageCustomerBalance => TotalCustomers > 0 ? CurrentTotalBalance / TotalCustomers : 0;

    /// <summary>
    /// Foydalanish foizi
    /// </summary>
    public decimal UsagePercentage => TotalEarnedSystemWide > 0 ? (TotalUsedSystemWide / TotalEarnedSystemWide) * 100 : 0;

    /// <summary>
    /// Muddati o'tish foizi
    /// </summary>
    public decimal ExpiryPercentage => TotalEarnedSystemWide > 0 ? (TotalExpiredSystemWide / TotalEarnedSystemWide) * 100 : 0;
}

// ===================================================================
// CASHBACK SETTINGS DTO
// ===================================================================

/// <summary>
/// Cashback tizim sozlamalari uchun DTO
/// </summary>
public class CashbackSettingsDto
{
    /// <summary>
    /// Cashback foizi (default: 2%)
    /// </summary>
    public decimal CashbackPercentage { get; set; } = 2.0m;

    /// <summary>
    /// Amal qilish muddati (kunlarda, default: 30)
    /// </summary>
    public int ExpiryDays { get; set; } = 30;

    /// <summary>
    /// Minimal buyurtma summasi (cashback olish uchun)
    /// </summary>
    public decimal MinimumOrderAmount { get; set; } = 0;

    /// <summary>
    /// Maksimal cashback miqdori (bir buyurtma uchun)
    /// </summary>
    public decimal? MaximumCashbackPerOrder { get; set; }

    /// <summary>
    /// Cashback ishlatish minimal summasi
    /// </summary>
    public decimal MinimumUsageAmount { get; set; } = 1000;

    /// <summary>
    /// Cashback faolmi?
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}

// ===================================================================
// CASHBACK VALIDATION RESULT DTO
// ===================================================================

/// <summary>
/// Cashback ishlatish validatsiyasi natijasi uchun DTO
/// </summary>
public class CashbackValidationResultDto
{
    /// <summary>
    /// Validatsiya o'tganmi?
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Mavjud balans
    /// </summary>
    public decimal AvailableBalance { get; set; }

    /// <summary>
    /// So'ralgan miqdor
    /// </summary>
    public decimal RequestedAmount { get; set; }

    /// <summary>
    /// Xatolik xabari
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Ishlatish mumkin bo'lgan maksimal miqdor
    /// </summary>
    public decimal MaxUsableAmount { get; set; }

    /// <summary>
    /// FIFO tartibida ishlatilishi kerak bo'lgan transaction'lar
    /// </summary>
    public List<CashbackTransactionDto> TransactionsToUse { get; set; } = new();
}