using ZiyoMarket.Domain.Common;

namespace ZiyoMarket.Domain.Entities.Users;

/// <summary>
/// UserRole - User va Role orasidagi many-to-many bog'lanish
/// </summary>
public class UserRole : BaseAuditableEntity
{
    /// <summary>
    /// Foydalanuvchi ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Role ID
    /// </summary>
    public int RoleId { get; set; }

    /// <summary>
    /// Role berilgan sana
    /// </summary>
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Kim tomonidan berilgan (Admin UserId)
    /// </summary>
    public int? AssignedBy { get; set; }

    // Navigation Properties

    /// <summary>
    /// Foydalanuvchi
    /// </summary>
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Role
    /// </summary>
    public virtual Role Role { get; set; } = null!;
}
