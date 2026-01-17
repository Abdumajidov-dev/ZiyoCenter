using ZiyoMarket.Domain.Common;

namespace ZiyoMarket.Domain.Entities.Users;

/// <summary>
/// Permission entity - tizim ruxsatlari
/// </summary>
public class Permission : BaseAuditableEntity
{
    /// <summary>
    /// Permission nomi (unique) - ViewProducts, CreateProduct, ManageOrders, etc.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Permission tavsifi
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Guruh nomi (Product, Order, User, Report, Settings, etc.)
    /// </summary>
    public string Group { get; set; } = string.Empty;

    /// <summary>
    /// Faolmi
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation Properties

    /// <summary>
    /// Bu permissionga ega rollar
    /// </summary>
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    // Business Methods

    /// <summary>
    /// Permissionni faollashtirish
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Permissionni faolsizlashtirish
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }
}
