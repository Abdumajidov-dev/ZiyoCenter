using Microsoft.EntityFrameworkCore;
using ZiyoMarket.Data.UnitOfWorks;
using ZiyoMarket.Domain.Entities.Users;
using ZiyoMarket.Service.DTOs.Permissions;
using ZiyoMarket.Service.Interfaces;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Services;

/// <summary>
/// Permission management service implementation
/// </summary>
public class PermissionManagementService : IPermissionManagementService
{
    private readonly IUnitOfWork _unitOfWork;

    public PermissionManagementService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<PermissionResponseDto>>> GetAllPermissionsAsync()
    {
        try
        {
            var permissions = await _unitOfWork.Permissions.FindAsync(p => p.IsActive);

            var permissionDtos = permissions.Select(p => new PermissionResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                DisplayName = p.Description ?? p.Name, // DisplayName uchun Description ishlatamiz
                Description = p.Description ?? "",
                Module = p.Group, // Backend'da "Group", Frontend'da "Module"
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            }).ToList();

            return Result<List<PermissionResponseDto>>.Success(permissionDtos);
        }
        catch (Exception ex)
        {
            return Result<List<PermissionResponseDto>>.InternalError($"Permissionlarni olishda xatolik: {ex.Message}");
        }
    }

    public async Task<Result<List<PermissionResponseDto>>> GetPermissionsByModuleAsync(string module)
    {
        try
        {
            // Frontend "Products" yuboradi, backend "Product" kutadi
            var normalizedModule = NormalizeModuleName(module);

            var permissions = await _unitOfWork.Permissions.FindAsync(p =>
                p.IsActive && p.Group == normalizedModule);

            var permissionDtos = permissions.Select(p => new PermissionResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                DisplayName = p.Description ?? p.Name,
                Description = p.Description ?? "",
                Module = p.Group,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            }).ToList();

            return Result<List<PermissionResponseDto>>.Success(permissionDtos);
        }
        catch (Exception ex)
        {
            return Result<List<PermissionResponseDto>>.InternalError($"Module bo'yicha permissionlarni olishda xatolik: {ex.Message}");
        }
    }

    public async Task<Result<List<PermissionResponseDto>>> GetAdminPermissionsAsync(int adminId)
    {
        try
        {
            // Admin entityni tekshirish
            var admin = await _unitOfWork.Admins.GetByIdAsync(adminId);
            if (admin == null)
                return Result<List<PermissionResponseDto>>.NotFound("Admin topilmadi");

            // Agar admin eski tizimda "Permissions" stringga ega bo'lsa
            if (!string.IsNullOrEmpty(admin.Permissions))
            {
                var permissionNames = admin.Permissions.Split(',', StringSplitOptions.RemoveEmptyEntries);
                var permissions = await _unitOfWork.Permissions.FindAsync(p =>
                    p.IsActive && permissionNames.Contains(p.Name));

                var permissionDtos = permissions.Select(p => new PermissionResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    DisplayName = p.Description ?? p.Name,
                    Description = p.Description ?? "",
                    Module = p.Group,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt
                }).ToList();

                return Result<List<PermissionResponseDto>>.Success(permissionDtos);
            }

            // Yangi User-based permission system (agar implement qilingan bo'lsa)
            // TODO: User table'dan permission olish logikasini qo'shish
            return Result<List<PermissionResponseDto>>.Success(new List<PermissionResponseDto>());
        }
        catch (Exception ex)
        {
            return Result<List<PermissionResponseDto>>.InternalError($"Admin permissionlarini olishda xatolik: {ex.Message}");
        }
    }

    public async Task<Result> AssignPermissionsToAdminAsync(int adminId, List<int> permissionIds, int assignedBy)
    {
        try
        {
            var admin = await _unitOfWork.Admins.GetByIdAsync(adminId);
            if (admin == null)
                return Result.NotFound("Admin topilmadi");

            // Permission'larni ID bo'yicha olish
            var permissions = await _unitOfWork.Permissions.FindAsync(p =>
                permissionIds.Contains(p.Id) && p.IsActive);

            if (permissions.ToList().Count != permissionIds.Count)
                return Result.BadRequest("Ba'zi permissionlar topilmadi yoki faol emas");

            // Permission nomlarini vergul bilan ajratilgan string qilib saqlash (eski tizim)
            var permissionNames = permissions.Select(p => p.Name).ToList();
            admin.Permissions = string.Join(",", permissionNames);
            admin.UpdatedBy = assignedBy;
            admin.MarkAsUpdated();

            await _unitOfWork.Admins.Update(admin, adminId);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Permissionlar muvaffaqiyatli biriktirildi");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Permissionlarni biriktirishda xatolik: {ex.Message}");
        }
    }

    public async Task<Result<PermissionResponseDto>> CreatePermissionAsync(CreatePermissionDto request, int createdBy)
    {
        try
        {
            // Mavjudligini tekshirish
            var existing = await _unitOfWork.Permissions.SelectAsync(p => p.Name == request.Name);
            if (existing != null)
                return Result<PermissionResponseDto>.BadRequest($"'{request.Name}' nomli permission allaqachon mavjud");

            var permission = new Permission
            {
                Name = request.Name,
                Description = request.DisplayName, // DisplayName ni Description ga saqlaymiz
                Group = NormalizeModuleName(request.Module),
                IsActive = true,
                CreatedBy = createdBy
            };

            await _unitOfWork.Permissions.InsertAsync(permission);
            await _unitOfWork.SaveChangesAsync();

            var dto = new PermissionResponseDto
            {
                Id = permission.Id,
                Name = permission.Name,
                DisplayName = permission.Description ?? permission.Name,
                Description = permission.Description ?? "",
                Module = permission.Group,
                IsActive = permission.IsActive,
                CreatedAt = permission.CreatedAt
            };

            return Result<PermissionResponseDto>.Success(dto, "Permission muvaffaqiyatli yaratildi");
        }
        catch (Exception ex)
        {
            return Result<PermissionResponseDto>.InternalError($"Permission yaratishda xatolik: {ex.Message}");
        }
    }

    public async Task<Result<PermissionResponseDto>> UpdatePermissionAsync(int id, UpdatePermissionDto request, int updatedBy)
    {
        try
        {
            var permission = await _unitOfWork.Permissions.GetByIdAsync(id);
            if (permission == null)
                return Result<PermissionResponseDto>.NotFound("Permission topilmadi");

            // Agar nom o'zgarsa, unique ekanligini tekshirish
            if (permission.Name != request.Name)
            {
                var existing = await _unitOfWork.Permissions.SelectAsync(p => p.Name == request.Name);
                if (existing != null)
                    return Result<PermissionResponseDto>.BadRequest($"'{request.Name}' nomli permission allaqachon mavjud");
            }

            permission.Name = request.Name;
            permission.Description = request.DisplayName;
            permission.Group = NormalizeModuleName(request.Module);
            permission.IsActive = request.IsActive;
            permission.UpdatedBy = updatedBy;
            permission.MarkAsUpdated();

            await _unitOfWork.Permissions.Update(permission, id);
            await _unitOfWork.SaveChangesAsync();

            var dto = new PermissionResponseDto
            {
                Id = permission.Id,
                Name = permission.Name,
                DisplayName = permission.Description ?? permission.Name,
                Description = permission.Description ?? "",
                Module = permission.Group,
                IsActive = permission.IsActive,
                CreatedAt = permission.CreatedAt
            };

            return Result<PermissionResponseDto>.Success(dto, "Permission muvaffaqiyatli yangilandi");
        }
        catch (Exception ex)
        {
            return Result<PermissionResponseDto>.InternalError($"Permission yangilashda xatolik: {ex.Message}");
        }
    }

    public async Task<Result> DeletePermissionAsync(int id, int deletedBy)
    {
        try
        {
            var permission = await _unitOfWork.Permissions.GetByIdAsync(id);
            if (permission == null)
                return Result.NotFound("Permission topilmadi");

            // Soft delete
            permission.IsActive = false;
            permission.UpdatedBy = deletedBy;
            permission.MarkAsUpdated();

            await _unitOfWork.Permissions.Update(permission, id);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Permission muvaffaqiyatli o'chirildi");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Permission o'chirishda xatolik: {ex.Message}");
        }
    }

    /// <summary>
    /// Module nomini normalizatsiya qilish (Frontend ko'plik, Backend birlik)
    /// </summary>
    private string NormalizeModuleName(string module)
    {
        return module switch
        {
            "Products" => "Product",
            "Orders" => "Order",
            "Customers" => "Customer",
            "Sellers" => "Seller",
            "Categories" => "Category",
            "Admins" => "Admin",
            "Users" => "User",
            "Reports" => "Report",
            "Notifications" => "Notification",
            _ => module
        };
    }
}
