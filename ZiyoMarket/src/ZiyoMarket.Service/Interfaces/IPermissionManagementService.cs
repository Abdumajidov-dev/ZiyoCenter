using ZiyoMarket.Service.DTOs.Permissions;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Interfaces;

/// <summary>
/// Permission management service - Admin panel uchun
/// </summary>
public interface IPermissionManagementService
{
    /// <summary>
    /// Barcha permissionlarni olish
    /// </summary>
    Task<Result<List<PermissionResponseDto>>> GetAllPermissionsAsync();

    /// <summary>
    /// Modul bo'yicha permissionlarni olish
    /// </summary>
    Task<Result<List<PermissionResponseDto>>> GetPermissionsByModuleAsync(string module);

    /// <summary>
    /// Admin'ning permissionlarini olish
    /// </summary>
    Task<Result<List<PermissionResponseDto>>> GetAdminPermissionsAsync(int adminId);

    /// <summary>
    /// Admin'ga permissionlarni biriktirish
    /// </summary>
    Task<Result> AssignPermissionsToAdminAsync(int adminId, List<int> permissionIds, int assignedBy);

    /// <summary>
    /// Yangi permission yaratish (SuperAdmin only)
    /// </summary>
    Task<Result<PermissionResponseDto>> CreatePermissionAsync(CreatePermissionDto request, int createdBy);

    /// <summary>
    /// Permission yangilash (SuperAdmin only)
    /// </summary>
    Task<Result<PermissionResponseDto>> UpdatePermissionAsync(int id, UpdatePermissionDto request, int updatedBy);

    /// <summary>
    /// Permission o'chirish (SuperAdmin only)
    /// </summary>
    Task<Result> DeletePermissionAsync(int id, int deletedBy);
}
