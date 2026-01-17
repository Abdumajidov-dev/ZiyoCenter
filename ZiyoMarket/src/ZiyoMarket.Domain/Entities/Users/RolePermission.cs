using ZiyoMarket.Domain.Common;

namespace ZiyoMarket.Domain.Entities.Users;

/// <summary>
/// RolePermission - Role va Permission orasidagi many-to-many bog'lanish
/// </summary>
public class RolePermission : BaseAuditableEntity
{
    /// <summary>
    /// Role ID
    /// </summary>
    public int RoleId { get; set; }

    /// <summary>
    /// Permission ID
    /// </summary>
    public int PermissionId { get; set; }

    // Navigation Properties

    /// <summary>
    /// Role
    /// </summary>
    public virtual Role Role { get; set; } = null!;

    /// <summary>
    /// Permission
    /// </summary>
    public virtual Permission Permission { get; set; } = null!;
}
