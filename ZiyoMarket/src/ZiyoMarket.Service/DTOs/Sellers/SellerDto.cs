using ZiyoMarket.Service.DTOs.Common;

namespace ZiyoMarket.Service.DTOs.Sellers;

/// <summary>
/// Sotuvchilar ro'yxati uchun DTO
/// </summary>
public class SellerListDto
{
    /// <summary>
    /// Sotuvchi ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Ism
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Familya
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// To'liq ism
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Telefon raqami
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Email
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Rol (Seller yoki Manager)
    /// </summary>
    public string Role { get; set; } = "Seller";

    /// <summary>
    /// Faolmi
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Manager rolimi
    /// </summary>
    public bool IsManager => Role.Equals("Manager", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Jami buyurtmalar soni
    /// </summary>
    public int TotalOrders { get; set; }

    /// <summary>
    /// Maksimal chegirma foizi
    /// </summary>
    public decimal MaxDiscountPercentage => IsManager ? 100 : 20;

    /// <summary>
    /// Yaratilgan sana
    /// </summary>
    public string CreatedAt { get; set; } = string.Empty;
}

// ===================================================================
// DETAIL DTO - Batafsil ma'lumot uchun
// ===================================================================

/// <summary>
/// Sotuvchi batafsil ma'lumoti uchun DTO
/// </summary>
public class SellerDetailDto
{
    /// <summary>
    /// Sotuvchi ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Ism
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Familya
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// To'liq ism
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Telefon raqami
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Email
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Rol (Seller yoki Manager)
    /// </summary>
    public string Role { get; set; } = "Seller";

    /// <summary>
    /// Faolmi
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Manager rolimi
    /// </summary>
    public bool IsManager => Role.Equals("Manager", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Maksimal chegirma foizi
    /// </summary>
    public decimal MaxDiscountPercentage => IsManager ? 100 : 20;

    /// <summary>
    /// Jami buyurtmalar soni
    /// </summary>
    public int TotalOrders { get; set; }

    /// <summary>
    /// Jami sotuvlar summasi
    /// </summary>
    public decimal TotalSales { get; set; }

    /// <summary>
    /// Yaratilgan sana
    /// </summary>
    public string CreatedAt { get; set; } = string.Empty;

    /// <summary>
    /// Yangilangan sana
    /// </summary>
    public string? UpdatedAt { get; set; }

    /// <summary>
    /// Kim tomonidan yaratilgan
    /// </summary>
    public int? CreatedBy { get; set; }

    /// <summary>
    /// Kim tomonidan yangilangan
    /// </summary>
    public int? UpdatedBy { get; set; }
}

// ===================================================================
// CREATE DTO - Yangi sotuvchi yaratish uchun
// ===================================================================

/// <summary>
/// Yangi sotuvchi yaratish uchun DTO
/// </summary>
public class CreateSellerDto
{
    /// <summary>
    /// Ism (majburiy)
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Familya (majburiy)
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Telefon raqami (majburiy, unique)
    /// Format: +998XXXXXXXXX
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Email (ixtiyoriy)
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Parol (majburiy)
    /// Minimum 6 ta belgi
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Rol (ixtiyoriy, default: "Seller")
    /// Mumkin qiymatlar: "Seller", "Manager"
    /// </summary>
    public string? Role { get; set; } = "Seller";
}

// ===================================================================
// UPDATE DTO - Sotuvchini yangilash uchun
// ===================================================================

/// <summary>
/// Sotuvchini yangilash uchun DTO
/// </summary>
public class UpdateSellerDto
{
    /// <summary>
    /// Ism (majburiy)
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Familya (majburiy)
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Telefon raqami (majburiy, unique)
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Email (ixtiyoriy)
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Rol (majburiy)
    /// Mumkin qiymatlar: "Seller", "Manager"
    /// </summary>
    public string Role { get; set; } = "Seller";
}

// ===================================================================
// PERFORMANCE DTO - Sotuvchi samaradorligi uchun
// ===================================================================

/// <summary>
/// Sotuvchi ishlash ko'rsatkichlari uchun DTO
/// </summary>
public class SellerPerformanceDto
{
    /// <summary>
    /// Sotuvchi ID
    /// </summary>
    public int SellerId { get; set; }

    /// <summary>
    /// Sotuvchi ismi
    /// </summary>
    public string SellerName { get; set; } = string.Empty;

    /// <summary>
    /// Jami buyurtmalar soni
    /// </summary>
    public int TotalOrders { get; set; }

    /// <summary>
    /// Jami sotuvlar summasi
    /// </summary>
    public decimal TotalSales { get; set; }

    /// <summary>
    /// O'rtacha buyurtma summasi
    /// </summary>
    public decimal AverageOrderValue { get; set; }

    /// <summary>
    /// Jami berilgan chegirmalar
    /// </summary>
    public decimal TotalDiscountGiven { get; set; }

    /// <summary>
    /// Chegirma foizi (jami sotuvdan)
    /// </summary>
    public decimal DiscountPercentage => TotalSales > 0 ? (TotalDiscountGiven / TotalSales) * 100 : 0;

    /// <summary>
    /// Sof sotuvlar (chegirmasiz)
    /// </summary>
    public decimal NetSales => TotalSales + TotalDiscountGiven;

    /// <summary>
    /// Ishlash darajasi (string)
    /// </summary>
    public string PerformanceLevel
    {
        get
        {
            if (TotalOrders >= 100) return "Excellent";
            if (TotalOrders >= 50) return "Good";
            if (TotalOrders >= 20) return "Average";
            return "Beginner";
        }
    }

    /// <summary>
    /// Ishlash darajasi (ball, 0-100)
    /// </summary>
    public int PerformanceScore
    {
        get
        {
            var orderScore = Math.Min(TotalOrders, 100);
            var salesScore = Math.Min((int)(TotalSales / 1000000), 100); // 1M so'm = 1 ball
            return (orderScore + salesScore) / 2;
        }
    }
}

// ===================================================================
// TOP SELLER DTO - Eng yaxshi sotuvchilar uchun
// ===================================================================

/// <summary>
/// Eng yaxshi sotuvchilar ro'yxati uchun DTO
/// </summary>
public class TopSellerDto
{
    /// <summary>
    /// Sotuvchi ID
    /// </summary>
    public int SellerId { get; set; }

    /// <summary>
    /// Sotuvchi ismi
    /// </summary>
    public string SellerName { get; set; } = string.Empty;

    /// <summary>
    /// Jami buyurtmalar soni
    /// </summary>
    public int TotalOrders { get; set; }

    /// <summary>
    /// Jami sotuvlar summasi
    /// </summary>
    public decimal TotalSales { get; set; }

    /// <summary>
    /// O'rni (rank)
    /// </summary>
    public int Rank { get; set; }

    /// <summary>
    /// Mukofot (badge)
    /// </summary>
    public string Badge
    {
        get
        {
            if (Rank == 1) return "🥇 Gold";
            if (Rank == 2) return "🥈 Silver";
            if (Rank == 3) return "🥉 Bronze";
            return "⭐ Star";
        }
    }
}

// ===================================================================
// FILTER REQUEST DTO - Qidiruv va filtrlash uchun
// ===================================================================

/// <summary>
/// Sotuvchilarni qidiruv va filtrlash uchun DTO
/// </summary>
public class SellerFilterRequest : PaginationRequest
{
    /// <summary>
    /// Qidiruv matni (ism, familya, telefon)
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Faolmi (true/false/null)
    /// null = barcha sotuvchilar
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Rol filtri
    /// Mumkin qiymatlar: "Seller", "Manager", null
    /// null = barcha rollar
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// Yaratilgan sana (dan)
    /// </summary>
    public DateTime? CreatedAfter { get; set; }

    /// <summary>
    /// Yaratilgan sana (gacha)
    /// </summary>
    public DateTime? CreatedBefore { get; set; }

    /// <summary>
    /// Minimal buyurtmalar soni
    /// </summary>
    public int? MinOrders { get; set; }

    /// <summary>
    /// Minimal sotuvlar summasi
    /// </summary>
    public decimal? MinSales { get; set; }
}

// ===================================================================
// SELLER STATISTICS DTO - Sotuvchi statistikasi uchun
// ===================================================================

/// <summary>
/// Sotuvchi statistikasi uchun DTO
/// </summary>
public class SellerStatisticsDto
{
    /// <summary>
    /// Sotuvchi ID
    /// </summary>
    public int SellerId { get; set; }

    /// <summary>
    /// Sotuvchi ismi
    /// </summary>
    public string SellerName { get; set; } = string.Empty;

    /// <summary>
    /// Bugungi buyurtmalar
    /// </summary>
    public int TodayOrders { get; set; }

    /// <summary>
    /// Bugungi sotuvlar
    /// </summary>
    public decimal TodaySales { get; set; }

    /// <summary>
    /// Shu hafta buyurtmalar
    /// </summary>
    public int WeekOrders { get; set; }

    /// <summary>
    /// Shu hafta sotuvlar
    /// </summary>
    public decimal WeekSales { get; set; }

    /// <summary>
    /// Shu oy buyurtmalar
    /// </summary>
    public int MonthOrders { get; set; }

    /// <summary>
    /// Shu oy sotuvlar
    /// </summary>
    public decimal MonthSales { get; set; }

    /// <summary>
    /// Jami buyurtmalar
    /// </summary>
    public int TotalOrders { get; set; }

    /// <summary>
    /// Jami sotuvlar
    /// </summary>
    public decimal TotalSales { get; set; }

    /// <summary>
    /// O'rtacha kunlik sotuvlar
    /// </summary>
    public decimal AverageDailySales { get; set; }

    /// <summary>
    /// Oxirgi buyurtma sanasi
    /// </summary>
    public DateTime? LastOrderDate { get; set; }
}

// ===================================================================
// SELLER SUMMARY DTO - Qisqacha ma'lumot
// ===================================================================

/// <summary>
/// Sotuvchi qisqacha ma'lumoti uchun DTO
/// </summary>
public class SellerSummaryDto
{
    /// <summary>
    /// Sotuvchi ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// To'liq ism
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Telefon
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Rol
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Faolmi
    /// </summary>
    public bool IsActive { get; set; }
}

// ===================================================================
// CHANGE PASSWORD DTO - Parolni o'zgartirish uchun
// ===================================================================

/// <summary>
/// Sotuvchi parolini o'zgartirish uchun DTO
/// </summary>
public class ChangeSellerPasswordDto
{
    /// <summary>
    /// Hozirgi parol
    /// </summary>
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// Yangi parol
    /// Minimum 6 ta belgi
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Yangi parolni tasdiqlash
    /// </summary>
    public string ConfirmPassword { get; set; } = string.Empty;
}

// ===================================================================
// CHANGE ROLE DTO - Rolni o'zgartirish uchun
// ===================================================================

/// <summary>
/// User rolini o'zgartirish uchun DTO
/// </summary>
public class ChangeRoleRequest
{
    /// <summary>
    /// Yangi rol
    /// Seller uchun: "Seller" yoki "Manager"
    /// Admin uchun: "Admin" yoki "SuperAdmin"
    /// </summary>
    public string NewRole { get; set; } = string.Empty;
}

// ===================================================================
// DISCOUNT PERMISSION DTO - Chegirma berish huquqi
// ===================================================================

/// <summary>
/// Sotuvchining chegirma berish huquqini tekshirish uchun DTO
/// </summary>
public class DiscountPermissionDto
{
    /// <summary>
    /// Sotuvchi ID
    /// </summary>
    public int SellerId { get; set; }

    /// <summary>
    /// Buyurtma summasi
    /// </summary>
    public decimal OrderTotal { get; set; }

    /// <summary>
    /// Berilayotgan chegirma
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Huquq bormi
    /// </summary>
    public bool CanApplyDiscount { get; set; }

    /// <summary>
    /// Maksimal chegirma summasi
    /// </summary>
    public decimal MaxAllowedDiscount { get; set; }

    /// <summary>
    /// Sabab (agar ruxsat berilmasa)
    /// </summary>
    public string? Reason { get; set; }
}

// ===================================================================
// SELLER ACTIVITY DTO - Faollik statistikasi
// ===================================================================

/// <summary>
/// Sotuvchi faollik statistikasi uchun DTO
/// </summary>
public class SellerActivityDto
{
    /// <summary>
    /// Sotuvchi ID
    /// </summary>
    public int SellerId { get; set; }

    /// <summary>
    /// Sotuvchi ismi
    /// </summary>
    public string SellerName { get; set; } = string.Empty;

    /// <summary>
    /// Oxirgi faollik sanasi
    /// </summary>
    public DateTime? LastActivityDate { get; set; }

    /// <summary>
    /// Bugun yaratgan buyurtmalar
    /// </summary>
    public int TodayOrdersCreated { get; set; }

    /// <summary>
    /// Hozirgi onlayn statusmi
    /// </summary>
    public bool IsOnline { get; set; }

    /// <summary>
    /// Jami ishlagan kunlar
    /// </summary>
    public int TotalWorkingDays { get; set; }

    /// <summary>
    /// Faollik foizi (0-100)
    /// </summary>
    public decimal ActivityPercentage { get; set; }
}
