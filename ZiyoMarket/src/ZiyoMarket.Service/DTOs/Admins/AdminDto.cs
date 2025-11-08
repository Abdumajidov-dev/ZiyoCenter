using ZiyoMarket.Service.DTOs.Common;

namespace ZiyoMarket.Service.DTOs.Admins;

/// <summary>
/// Adminlar ro'yxati uchun DTO
/// </summary>
public class AdminListDto
{
    /// <summary>
    /// Admin ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Ism
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Familiya
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// To'liq ism
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Foydalanuvchi nomi
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Email
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Telefon
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Rol (Admin yoki SuperAdmin)
    /// </summary>
    public string Role { get; set; } = "Admin";

    /// <summary>
    /// SuperAdmin rolimi
    /// </summary>
    public bool IsSuperAdmin => Role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Faolmi
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Oxirgi login vaqti
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Yaratilgan sana
    /// </summary>
    public string CreatedAt { get; set; } = string.Empty;
}

// ===================================================================
// DETAIL DTO - Batafsil ma'lumot uchun
// ===================================================================

/// <summary>
/// Admin batafsil ma'lumoti uchun DTO
/// </summary>
public class AdminDetailDto
{
    /// <summary>
    /// Admin ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Ism
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Familiya
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// To'liq ism
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Foydalanuvchi nomi
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Email
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Telefon
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Rol (Admin yoki SuperAdmin)
    /// </summary>
    public string Role { get; set; } = "Admin";

    /// <summary>
    /// SuperAdmin rolimi
    /// </summary>
    public bool IsSuperAdmin => Role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Ruxsatlar
    /// </summary>
    public string? Permissions { get; set; }

    /// <summary>
    /// Faolmi
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Oxirgi login vaqti
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

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
// CREATE DTO - Yangi admin yaratish uchun
// ===================================================================

/// <summary>
/// Yangi admin yaratish uchun DTO
/// </summary>
public class CreateAdminDto
{
    /// <summary>
    /// Ism (majburiy)
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Familiya (majburiy)
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Foydalanuvchi nomi (majburiy, unique)
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Email (majburiy, unique)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Telefon raqami (majburiy)
    /// Format: +998XXXXXXXXX
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Parol (majburiy)
    /// Minimum 6 ta belgi
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Rol (ixtiyoriy, default: "Admin")
    /// Mumkin qiymatlar: "Admin", "SuperAdmin"
    /// </summary>
    public string? Role { get; set; } = "Admin";

    /// <summary>
    /// Ruxsatlar (ixtiyoriy)
    /// </summary>
    public string? Permissions { get; set; }
}

// ===================================================================
// UPDATE DTO - Adminni yangilash uchun
// ===================================================================

/// <summary>
/// Adminni yangilash uchun DTO
/// </summary>
public class UpdateAdminDto
{
    /// <summary>
    /// Ism (majburiy)
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Familiya (majburiy)
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Foydalanuvchi nomi (majburiy, unique)
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Email (majburiy, unique)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Telefon raqami (majburiy)
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Rol (majburiy)
    /// Mumkin qiymatlar: "Admin", "SuperAdmin"
    /// </summary>
    public string Role { get; set; } = "Admin";

    /// <summary>
    /// Ruxsatlar (ixtiyoriy)
    /// </summary>
    public string? Permissions { get; set; }
}

// ===================================================================
// FILTER REQUEST DTO - Qidiruv va filtrlash uchun
// ===================================================================

/// <summary>
/// Adminlarni qidiruv va filtrlash uchun DTO
/// </summary>
public class AdminFilterRequest : PaginationRequest
{
    /// <summary>
    /// Qidiruv matni (ism, familya, username, email)
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Faolmi (true/false/null)
    /// null = barcha adminlar
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Rol filtri
    /// Mumkin qiymatlar: "Admin", "SuperAdmin", null
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
}
