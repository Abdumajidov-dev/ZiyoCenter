using ZiyoMarket.Service.DTOs.Sellers;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Interfaces;

public interface ISellerService
{
    // CRUD
    Task<Result<SellerDetailDto>> GetSellerByIdAsync(int sellerId);
    Task<Result<PaginationResponse<SellerListDto>>> GetSellersAsync(SellerFilterRequest request);
    Task<Result<SellerDetailDto>> CreateSellerAsync(CreateSellerDto request, int createdBy);
    Task<Result<SellerDetailDto>> UpdateSellerAsync(int id, UpdateSellerDto request, int updatedBy);
    Task<Result> DeleteSellerAsync(int sellerId, int deletedBy);

    // Performance
    Task<Result<SellerPerformanceDto>> GetSellerPerformanceAsync(int sellerId, DateTime? startDate = null, DateTime? endDate = null);
    Task<Result<List<TopSellerDto>>> GetTopSellersAsync(int count = 10);

    // Status
    Task<Result> ToggleSellerStatusAsync(int sellerId, int updatedBy);
    Task<Result> ChangeSellerRoleAsync(int sellerId, string newRole, int updatedBy);

    // Search
    Task<Result<List<SellerListDto>>> SearchSellersAsync(string searchTerm);

    // Bulk operations
    Task<Result> DeleteAllSellersAsync(int deletedBy, DateTime? startDate = null, DateTime? endDate = null);
    Task<Result<List<SellerDetailDto>>> SeedMockSellersAsync(int createdBy, int count = 10);
}
