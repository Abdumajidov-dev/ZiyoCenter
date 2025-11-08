using ZiyoMarket.Service.DTOs.Admins;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Interfaces;

public interface IAdminService
{
    // CRUD
    Task<Result<AdminDetailDto>> GetAdminByIdAsync(int adminId);
    Task<Result<PaginationResponse<AdminListDto>>> GetAdminsAsync(AdminFilterRequest request);
    Task<Result<AdminDetailDto>> CreateAdminAsync(CreateAdminDto request, int createdBy);
    Task<Result<AdminDetailDto>> UpdateAdminAsync(int id, UpdateAdminDto request, int updatedBy);
    Task<Result> DeleteAdminAsync(int adminId, int deletedBy);

    // Status
    Task<Result> ToggleAdminStatusAsync(int adminId, int updatedBy);
    Task<Result> ChangeAdminRoleAsync(int adminId, string newRole, int updatedBy);

    // Search
    Task<Result<List<AdminListDto>>> SearchAdminsAsync(string searchTerm);
}
