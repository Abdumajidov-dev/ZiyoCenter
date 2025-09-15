using ZiyoMarket.Domain.Common;
using ZiyoMarket.Domain.Entities.Notifications;
using ZiyoMarket.Domain.Entities.Orders;

namespace ZiyoMarket.Domain.Entities.Users;

/// <summary>
/// Sotuvchi entity'si
/// </summary>
public class Seller : BaseAuditableEntity
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
    /// Rol (Seller, Manager)
    /// </summary>
    public string Role { get; set; } = "Seller";

    /// <summary>
    /// Faolmi
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation Properties

    /// <summary>
    /// Sotuvchi tomonidan yaratilgan buyurtmalar
    /// </summary>
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    /// <summary>
    /// Sotuvchi tomonidan berilgan chegirmalar
    /// </summary>
    public virtual ICollection<OrderDiscount> OrderDiscounts { get; set; } = new List<OrderDiscount>();

    /// <summary>
    /// Sotuvchi xabarlari
    /// </summary>
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    // Business Methods

    /// <summary>
    /// To'liq ismi
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Manager rolimi?
    /// </summary>
    public bool IsManager => Role.Equals("Manager", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Sotuvchi faollashtirish
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Sotuvchi faolsizlashtirish
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
        var allowedRoles = new[] { "Seller", "Manager" };

        if (!allowedRoles.Contains(newRole))
            throw new ArgumentException($"Noto'g'ri rol: {newRole}");

        Role = newRole;
        MarkAsUpdated();
    }

    /// <summary>
    /// Chegirma berish huquqi bormi?
    /// </summary>
    public bool CanApplyDiscount(decimal discountAmount, decimal orderTotal)
    {
        if (!IsActive)
            return false;

        // Manager cheksiz chegirma bera oladi
        if (IsManager)
            return true;

        // Oddiy sotuvchi maksimal 20% chegirma bera oladi
        var maxDiscountPercentage = orderTotal * 0.20m;
        return discountAmount <= maxDiscountPercentage;
    }
}