using ZiyoMarket.Domain.Common;

namespace ZiyoMarket.Domain.Entities.Users;

/// <summary>
/// Role entity - foydalanuvchi rollari
/// </summary>
public class Role : BaseAuditableEntity
{
    /// <summary>
    /// Role nomi (unique) - Customer, Seller, Manager, Admin, SuperAdmin
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Role tavsifi
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Faolmi
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// System role (o'chirib bo'lmaydigan)
    /// </summary>
    public bool IsSystemRole { get; set; } = false;

    // Navigation Properties

    /// <summary>
    /// Bu rolega ega foydalanuvchilar
    /// </summary>
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    /// <summary>
    /// Bu rolening permissionlari
    /// </summary>
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    // Business Methods

    /// <summary>
    /// Roleni faollashtirish
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Roleni faolsizlashtirish
    /// </summary>
    public void Deactivate()
    {
        if (IsSystemRole)
            throw new InvalidOperationException("System roleni faolsizlashtirib bo'lmaydi");

        IsActive = false;
        MarkAsUpdated();
    }
}
