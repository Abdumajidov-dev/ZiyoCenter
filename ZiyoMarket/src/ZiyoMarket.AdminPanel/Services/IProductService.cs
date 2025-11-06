using ZiyoMarket.AdminPanel.Models;

namespace ZiyoMarket.AdminPanel.Services;

public interface IProductService
{
    Task<ApiResponse<PaginatedResult<Product>>> GetProductsAsync(int pageNumber = 1, int pageSize = 20, string? search = null);
    Task<ApiResponse<Product>> GetProductByIdAsync(int id);
    Task<ApiResponse<Product>> CreateProductAsync(CreateProductDto product);
    Task<ApiResponse<Product>> UpdateProductAsync(UpdateProductDto product);
    Task<ApiResponse<bool>> DeleteProductAsync(int id);
    Task<ApiResponse<List<Category>>> GetCategoriesAsync();
}
