using ZiyoMarket.Service.DTOs.Products;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Interfaces;

/// <summary>
/// Category management service interface
/// </summary>
public interface ICategoryService
{
    // CRUD Operations
    Task<Result<CategoryDto>> GetCategoryByIdAsync(int categoryId);
    Task<Result<List<CategoryDto>>> GetAllCategoriesAsync();
    Task<Result<List<CategoryDto>>> GetRootCategoriesAsync();
    Task<Result<List<CategoryDto>>> GetSubCategoriesAsync(int parentId);
    Task<Result<CategoryDto>> CreateCategoryAsync(SaveCategoryDto request, int createdBy);
    Task<Result<CategoryDto>> UpdateCategoryAsync(int id, SaveCategoryDto request, int updatedBy);
    Task<Result> DeleteCategoryAsync(int categoryId, int deletedBy);

    // Hierarchy Operations
    Task<Result<List<CategoryDto>>> GetCategoryTreeAsync();
    Task<Result<string>> GetCategoryPathAsync(int categoryId);
    Task<Result<List<CategoryDto>>> GetCategoryWithChildrenAsync(int categoryId);

    // Search & Filter
    Task<Result<List<CategoryDto>>> SearchCategoriesAsync(string searchTerm);
    Task<Result<List<CategoryDto>>> GetActiveCategoriesAsync();

    // Product count
    Task<Result<CategoryWithProductCountDto>> GetCategoryWithProductCountAsync(int categoryId);
    Task<Result<List<CategoryWithProductCountDto>>> GetAllCategoriesWithProductCountAsync();

    // Admin Operations
    Task<Result> ReorderCategoriesAsync(List<ReorderCategoryDto> categories, int updatedBy);
    Task<Result> ToggleCategoryStatusAsync(int categoryId, int updatedBy);

    // Bulk Operations
    Task<Result> DeleteAllCategoriesAsync(int deletedBy, DateTime? startDate = null, DateTime? endDate = null);
    Task<Result<List<CategoryDto>>> SeedMockCategoriesAsync(int createdBy, int count = 10);
}