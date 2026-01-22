using ZiyoMarket.Domain.Common;
using ZiyoMarket.Domain.Entities.Notifications;
using ZiyoMarket.Domain.Entities.Orders;
using ZiyoMarket.Domain.Entities.Products;
using ZiyoMarket.Domain.Entities.Support;

namespace ZiyoMarket.Domain.Entities.Users;

/// <summary>
/// Unified User entity - barcha foydalanuvchilar uchun
/// </summary>
public class User : BaseAuditableEntity
{
    /// <summary>
    /// Ism
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Familya
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Telefon raqami (unique)
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Parol hash'i
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Username (Admin uchun)
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Manzil
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// FCM token (push notification uchun)
    /// </summary>
    public string? FcmToken { get; set; }

    /// <summary>
    /// Cashback balansi
    /// </summary>
    public decimal CashbackBalance { get; set; } = 0;

    /// <summary>
    /// Faolmi
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Telefon tasdiqlangan
    /// </summary>
    public bool IsPhoneVerified { get; set; } = false;

    /// <summary>
    /// Oxirgi login vaqti
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    // Navigation Properties

    /// <summary>
    /// Foydalanuvchining rollari (many-to-many)
    /// </summary>
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    /// <summary>
    /// Foydalanuvchining buyurtmalari
    /// </summary>
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    /// <summary>
    /// Foydalanuvchining savat item'lari
    /// </summary>
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    /// <summary>
    /// Foydalanuvchining yoqtirgan mahsulotlari
    /// </summary>
    public virtual ICollection<ProductLike> ProductLikes { get; set; } = new List<ProductLike>();

    /// <summary>
    /// Foydalanuvchining cashback tranzaksiyalari
    /// </summary>
    public virtual ICollection<CashbackTransaction> CashbackTransactions { get; set; } = new List<CashbackTransaction>();

    /// <summary>
    /// Foydalanuvchining support chat'lari
    /// </summary>
    public virtual ICollection<SupportChat> SupportChats { get; set; } = new List<SupportChat>();

    /// <summary>
    /// Foydalanuvchining xabarlari
    /// </summary>
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    /// <summary>
    /// Foydalanuvchi tomonidan berilgan chegirmalar
    /// </summary>
    public virtual ICollection<OrderDiscount> OrderDiscounts { get; set; } = new List<OrderDiscount>();

    /// <summary>
    /// Foydalanuvchining qurilma tokenlari
    /// </summary>
    public virtual ICollection<DeviceToken> DeviceTokens { get; set; } = new List<DeviceToken>();

    // Business Methods

    /// <summary>
    /// To'liq ismi
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Cashback qo'shish
    /// </summary>
    public void AddCashback(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Cashback summasi musbat bo'lishi kerak");

        CashbackBalance += amount;
        MarkAsUpdated();
    }

    /// <summary>
    /// Cashback ishlatish
    /// </summary>
    public void UseCashback(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Ishlatilayotgan summa musbat bo'lishi kerak");

        if (amount > CashbackBalance)
            throw new InvalidOperationException("Cashback balansi yetarli emas");

        CashbackBalance -= amount;
        MarkAsUpdated();
    }

    /// <summary>
    /// Foydalanuvchini faollashtirish
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Foydalanuvchini faolsizlashtirish
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    /// <summary>
    /// FCM token yangilash
    /// </summary>
    public void UpdateFcmToken(string? fcmToken)
    {
        FcmToken = fcmToken;
        MarkAsUpdated();
    }

    /// <summary>
    /// Login vaqtini yangilash
    /// </summary>
    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    /// <summary>
    /// Telefon tasdiqlash
    /// </summary>
    public void VerifyPhone()
    {
        IsPhoneVerified = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Ma'lum bir permission bormi?
    /// </summary>
    public bool HasPermission(string permissionName)
    {
        if (!IsActive)
            return false;

        return UserRoles
            .Where(ur => ur.Role.IsActive)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Any(rp => rp.Permission.Name == permissionName && rp.Permission.IsActive);
    }

    /// <summary>
    /// Ma'lum bir role bormi?
    /// </summary>
    public bool HasRole(string roleName)
    {
        if (!IsActive)
            return false;

        return UserRoles
            .Any(ur => ur.Role.Name == roleName && ur.Role.IsActive);
    }

    /// <summary>
    /// Barcha permissionlarni olish
    /// </summary>
    public IEnumerable<string> GetPermissions()
    {
        if (!IsActive)
            return Enumerable.Empty<string>();

        return UserRoles
            .Where(ur => ur.Role.IsActive)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Where(rp => rp.Permission.IsActive)
            .Select(rp => rp.Permission.Name)
            .Distinct();
    }

    /// <summary>
    /// Barcha role nomlarini olish
    /// </summary>
    public IEnumerable<string> GetRoleNames()
    {
        if (!IsActive)
            return Enumerable.Empty<string>();

        return UserRoles
            .Where(ur => ur.Role.IsActive)
            .Select(ur => ur.Role.Name)
            .Distinct();
    }
}
