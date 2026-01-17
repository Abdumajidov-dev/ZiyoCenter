using ZiyoMarket.Data.UnitOfWorks;
using ZiyoMarket.Domain.Entities.Users;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Services;

/// <summary>
/// Role va Permission management service
/// </summary>
public class RoleManagementService
{
    private readonly IUnitOfWork _unitOfWork;

    public RoleManagementService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Foydalanuvchiga role biriktirish (Admin uchun)
    /// </summary>
    public async Task<Result> AssignRoleToUserAsync(int userId, string roleName, int assignedByAdminId)
    {
        try
        {
            // Foydalanuvchini topish
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return Result.NotFound("Foydalanuvchi topilmadi");

            // Roleni topish
            var role = await _unitOfWork.Roles.SelectAsync(r => r.Name == roleName && r.IsActive);
            if (role == null)
                return Result.NotFound($"'{roleName}' role topilmadi yoki faol emas");

            // Allaqachon biriktirgan yoki yo'qligini tekshirish
            var existingUserRole = await _unitOfWork.UserRoles.SelectAsync(ur =>
                ur.UserId == userId && ur.RoleId == role.Id);

            if (existingUserRole != null)
                return Result.BadRequest($"Foydalanuvchi allaqachon '{roleName}' rolega ega");

            // Yangi UserRole yaratish
            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = role.Id,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = assignedByAdminId
            };

            await _unitOfWork.UserRoles.InsertAsync(userRole);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success($"'{roleName}' role muvaffaqiyatli biriktirildi");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Role biriktirish xatolik: {ex.Message}");
        }
    }

    /// <summary>
    /// Foydalanuvchidan roleni olib tashlash
    /// </summary>
    public async Task<Result> RemoveRoleFromUserAsync(int userId, string roleName)
    {
        try
        {
            // Roleni topish
            var role = await _unitOfWork.Roles.SelectAsync(r => r.Name == roleName);
            if (role == null)
                return Result.NotFound($"'{roleName}' role topilmadi");

            // UserRole'ni topish
            var userRole = await _unitOfWork.UserRoles.SelectAsync(ur =>
                ur.UserId == userId && ur.RoleId == role.Id);

            if (userRole == null)
                return Result.NotFound($"Foydalanuvchi '{roleName}' rolega ega emas");

            // O'chirish
            await _unitOfWork.UserRoles.DeleteAsync(userRole.Id);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success($"'{roleName}' role olib tashlandi");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Role olib tashlash xatolik: {ex.Message}");
        }
    }

    /// <summary>
    /// Foydalanuvchining barcha rollarini olish
    /// </summary>
    public async Task<Result<List<string>>> GetUserRolesAsync(int userId)
    {
        try
        {
            var user = await _unitOfWork.Users.SelectAsync(
                u => u.Id == userId,
                includes: new[] { "UserRoles.Role" }
            );

            if (user == null)
                return Result<List<string>>.NotFound("Foydalanuvchi topilmadi");

            var roles = user.UserRoles
                .Where(ur => ur.Role.IsActive)
                .Select(ur => ur.Role.Name)
                .ToList();

            return Result<List<string>>.Success(roles);
        }
        catch (Exception ex)
        {
            return Result<List<string>>.InternalError($"Rollarni olishda xatolik: {ex.Message}");
        }
    }

    /// <summary>
    /// Foydalanuvchining barcha permissionlarini olish
    /// </summary>
    public async Task<Result<List<string>>> GetUserPermissionsAsync(int userId)
    {
        try
        {
            var user = await _unitOfWork.Users.SelectAsync(
                u => u.Id == userId,
                includes: new[] { "UserRoles.Role.RolePermissions.Permission" }
            );

            if (user == null)
                return Result<List<string>>.NotFound("Foydalanuvchi topilmadi");

            var permissions = user.GetPermissions().ToList();

            return Result<List<string>>.Success(permissions);
        }
        catch (Exception ex)
        {
            return Result<List<string>>.InternalError($"Permissionlarni olishda xatolik: {ex.Message}");
        }
    }

    /// <summary>
    /// Barcha rollarni olish
    /// </summary>
    public async Task<Result<List<RoleDto>>> GetAllRolesAsync()
    {
        try
        {
            var roles = await _unitOfWork.Roles.FindAsync(r => r.IsActive);

            var roleDtos = roles.Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                IsSystemRole = r.IsSystemRole
            }).ToList();

            return Result<List<RoleDto>>.Success(roleDtos);
        }
        catch (Exception ex)
        {
            return Result<List<RoleDto>>.InternalError($"Rollarni olishda xatolik: {ex.Message}");
        }
    }

    /// <summary>
    /// Barcha permissionlarni olish
    /// </summary>
    public async Task<Result<List<PermissionDto>>> GetAllPermissionsAsync()
    {
        try
        {
            var permissions = await _unitOfWork.Permissions.FindAsync(p => p.IsActive);

            var permissionDtos = permissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Group = p.Group
            }).ToList();

            return Result<List<PermissionDto>>.Success(permissionDtos);
        }
        catch (Exception ex)
        {
            return Result<List<PermissionDto>>.InternalError($"Permissionlarni olishda xatolik: {ex.Message}");
        }
    }
}

// DTOs
public class RoleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
}

public class PermissionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Group { get; set; } = string.Empty;
}
