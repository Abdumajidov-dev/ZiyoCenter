using ZiyoMarket.Domain.Common;
using ZiyoMarket.Domain.Entities.Notifications;
using ZiyoMarket.Domain.Entities.Support;

namespace ZiyoMarket.Domain.Entities.Users;

/// <summary>
/// Administrator entity'si
/// </summary>
public class Admin : BaseAuditableEntity
{
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
    /// Foydalanuvchi nomi (unique)
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Telefon raqami
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Parol hash'i
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Rol (Admin, SuperAdmin)
    /// </summary>
    public string Role { get; set; } = "Admin";

    /// <summary>
    /// Ruxsatlar (vergul bilan ajratilgan)
    /// </summary>
    public string? Permissions { get; set; }

    /// <summary>
    /// Faolmi
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Oxirgi login vaqti
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    // Navigation Properties

    /// <summary>
    /// Admin javob bergan support chat'lar
    /// </summary>
    public virtual ICollection<SupportChat> SupportChats { get; set; } = new List<SupportChat>();

    /// <summary>
    /// Admin xabarlari
    /// </summary>
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    // Business Methods

    /// <summary>
    /// SuperAdmin rolimi?
    /// </summary>
    public bool IsSuperAdmin => Role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Admin faollashtirish
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Admin faolsizlashtirish
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    /// <summary>
    /// Rolni o'zgartirish
    /// </summary>
    public void ChangeRole(string newRole)
    {
        var allowedRoles = new[] { "Admin", "SuperAdmin" };

        if (!allowedRoles.Contains(newRole))
            throw new ArgumentException($"Noto'g'ri rol: {newRole}");

        Role = newRole;
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
    /// Ma'lum amallarni bajarish huquqi bormi?
    /// </summary>
    public bool HasPermission(string action)
    {
        if (!IsActive)
            return false;

        // SuperAdmin barcha amallarni bajara oladi
        if (IsSuperAdmin)
            return true;

        // Oddiy Admin'ning cheklangan huquqlari
        var allowedActions = new[]
        {
            "ViewProducts", "CreateProduct", "UpdateProduct",
            "ViewOrders", "UpdateOrderStatus",
            "ViewCustomers", "ViewReports",
            "ViewNotifications", "RespondToSupport"
        };

        return allowedActions.Contains(action);
    }

    /// <summary>
    /// Boshqa admin'larni boshqarish huquqi bormi?
    /// </summary>
    public bool CanManageAdmins()
    {
        return IsActive && IsSuperAdmin;
    }

    /// <summary>
    /// Tizim sozlamalarini o'zgartirish huquqi bormi?
    /// </summary>
    public bool CanManageSystemSettings()
    {
        return IsActive && IsSuperAdmin;
    }
}