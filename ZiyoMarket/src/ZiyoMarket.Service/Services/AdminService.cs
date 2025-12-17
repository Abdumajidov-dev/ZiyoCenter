using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ZiyoMarket.Data.UnitOfWorks;
using ZiyoMarket.Domain.Entities.Users;
using ZiyoMarket.Service.DTOs.Admins;
using ZiyoMarket.Service.Interfaces;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Services;

public class AdminService : IAdminService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AdminService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<AdminDetailDto>> GetAdminByIdAsync(int adminId)
    {
        try
        {
            var admin = await _unitOfWork.Admins
                .SelectAsync(a => a.Id == adminId && a.DeletedAt == null);

            if (admin == null)
                return Result<AdminDetailDto>.NotFound("Admin topilmadi");

            var dto = _mapper.Map<AdminDetailDto>(admin);
            return Result<AdminDetailDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<AdminDetailDto>.InternalError($"Xatolik: {ex.Message}");
        }
    }

    public async Task<Result<PaginationResponse<AdminListDto>>> GetAdminsAsync(AdminFilterRequest request)
    {
        try
        {
            var query = _unitOfWork.Admins.Table.Where(a => a.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                query = query.Where(a =>
                    a.FirstName.ToLower().Contains(term) ||
                    a.LastName.ToLower().Contains(term) ||
                    a.Username.ToLower().Contains(term));
            }

            if (request.IsActive.HasValue)
                query = query.Where(a => a.IsActive == request.IsActive.Value);

            if (!string.IsNullOrWhiteSpace(request.Role))
                query = query.Where(a => a.Role == request.Role);

            query = query.OrderByDescending(a => a.CreatedAt);

            var total = await query.CountAsync();
            var admins = await query
                .Skip(request.Skip)
                .Take(request.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<List<AdminListDto>>(admins);

            return Result<PaginationResponse<AdminListDto>>.Success(
                new PaginationResponse<AdminListDto>(dtos, total, request.PageNumber, request.PageSize));
        }
        catch (Exception ex)
        {
            return Result<PaginationResponse<AdminListDto>>.InternalError($"Xatolik: {ex.Message}");
        }
    }

    public async Task<Result<AdminDetailDto>> CreateAdminAsync(CreateAdminDto request, int createdBy)
    {
        try
        {
            var existingUsername = await _unitOfWork.Admins
                .AnyAsync(a => a.Username == request.Username && a.DeletedAt == null);

            if (existingUsername)
                return Result<AdminDetailDto>.BadRequest("Bu username allaqachon mavjud");

            var admin = new Admin
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Username = request.Username,
                Phone = request.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role ?? "Admin",
                Permissions = request.Permissions,
                IsActive = true,
                CreatedBy = createdBy
            };

            await _unitOfWork.Admins.InsertAsync(admin);
            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<AdminDetailDto>(admin);
            return Result<AdminDetailDto>.Success(dto, "Admin muvaffaqiyatli yaratildi");
        }
        catch (Exception ex)
        {
            return Result<AdminDetailDto>.InternalError($"Xatolik: {ex.Message}");
        }
    }

    public async Task<Result<AdminDetailDto>> UpdateAdminAsync(int id, UpdateAdminDto request, int updatedBy)
    {
        try
        {
            var admin = await _unitOfWork.Admins.GetByIdAsync(id);
            if (admin == null)
                return Result<AdminDetailDto>.NotFound("Admin topilmadi");

            var existingUsername = await _unitOfWork.Admins
                .AnyAsync(a => a.Username == request.Username && a.Id != id && a.DeletedAt == null);

            if (existingUsername)
                return Result<AdminDetailDto>.BadRequest("Bu username allaqachon mavjud");

            admin.FirstName = request.FirstName;
            admin.LastName = request.LastName;
            admin.Username = request.Username;
            admin.Phone = request.Phone;
            admin.Role = request.Role;
            admin.Permissions = request.Permissions;
            admin.UpdatedBy = updatedBy;
            admin.MarkAsUpdated();

            await _unitOfWork.Admins.Update(admin, id);
            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<AdminDetailDto>(admin);
            return Result<AdminDetailDto>.Success(dto, "Admin muvaffaqiyatli yangilandi");
        }
        catch (Exception ex)
        {
            return Result<AdminDetailDto>.InternalError($"Xatolik: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAdminAsync(int adminId, int deletedBy)
    {
        try
        {
            var admin = await _unitOfWork.Admins.GetByIdAsync(adminId);
            if (admin == null)
                return Result.NotFound("Admin topilmadi");

            admin.DeletedBy = deletedBy;
            admin.Delete();

            await _unitOfWork.Admins.Update(admin, adminId);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Admin muvaffaqiyatli o'chirildi");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Xatolik: {ex.Message}");
        }
    }

    public async Task<Result> ToggleAdminStatusAsync(int adminId, int updatedBy)
    {
        try
        {
            var admin = await _unitOfWork.Admins.GetByIdAsync(adminId);
            if (admin == null)
                return Result.NotFound("Admin topilmadi");

            admin.IsActive = !admin.IsActive;
            admin.UpdatedBy = updatedBy;
            admin.MarkAsUpdated();

            await _unitOfWork.Admins.Update(admin, adminId);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success($"Admin {(admin.IsActive ? "faollashtirildi" : "faolsizlantirildi")}");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Xatolik: {ex.Message}");
        }
    }

    public async Task<Result> ChangeAdminRoleAsync(int adminId, string newRole, int updatedBy)
    {
        try
        {
            var admin = await _unitOfWork.Admins.GetByIdAsync(adminId);
            if (admin == null)
                return Result.NotFound("Admin topilmadi");

            // Validate role using entity method
            try
            {
                admin.ChangeRole(newRole);
            }
            catch (ArgumentException ex)
            {
                return Result.BadRequest(ex.Message);
            }

            admin.UpdatedBy = updatedBy;
            admin.MarkAsUpdated();

            await _unitOfWork.Admins.Update(admin, adminId);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success($"Admin roli '{newRole}' ga o'zgartirildi");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Xatolik: {ex.Message}");
        }
    }

    public async Task<Result<List<AdminListDto>>> SearchAdminsAsync(string searchTerm)
    {
        try
        {
            var term = searchTerm.ToLower();
            var admins = await _unitOfWork.Admins.Table
                .Where(a => a.DeletedAt == null &&
                    (a.FirstName.ToLower().Contains(term) ||
                     a.LastName.ToLower().Contains(term) ||
                     a.Username.ToLower().Contains(term)))
                .Take(20)
                .ToListAsync();

            var dtos = _mapper.Map<List<AdminListDto>>(admins);
            return Result<List<AdminListDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<AdminListDto>>.InternalError($"Xatolik: {ex.Message}");
        }
    }
}
