using ZiyoMarket.Domain.Common;

namespace ZiyoMarket.Service.DTOs.Admins;

/// <summary>
/// Admin panel kutadigan ruxsat shakli (Models/DTOs/PermissionDto).
/// </summary>
public class PermissionDto
{
    public int Id { get; set; }

    /// <summary>
    /// Ruxsat kodi, masalan "orders.manage"
    /// </summary>
    public string Name { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public static PermissionDto From(AdminPermissions.Definition p) => new()
    {
        Id = p.Id,
        Name = p.Code,
        DisplayName = p.DisplayName,
        Description = p.Description,
        Module = p.Module,
        IsActive = true,
        CreatedAt = DateTime.MinValue
    };
}

public class AssignPermissionsRequest
{
    public int AdminId { get; set; }
    public List<int> PermissionIds { get; set; } = new();
}
