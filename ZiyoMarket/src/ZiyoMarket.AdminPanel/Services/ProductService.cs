using ZiyoMarket.AdminPanel.Models;

namespace ZiyoMarket.AdminPanel.Services;

public class ProductService : IProductService
{
    private readonly IApiService _apiService;

    public ProductService(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<ApiResponse<PaginatedResult<Product>>> GetProductsAsync(int pageNumber = 1, int pageSize = 20, string? search = null)
    {
        var endpoint = $"product?pageNumber={pageNumber}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
        {
            endpoint += $"&search={Uri.EscapeDataString(search)}";
        }

        return await _apiService.GetAsync<PaginatedResult<Product>>(endpoint);
    }

    public async Task<ApiResponse<Product>> GetProductByIdAsync(int id)
    {
        return await _apiService.GetAsync<Product>($"product/{id}");
    }

    public async Task<ApiResponse<Product>> CreateProductAsync(CreateProductDto product)
    {
        return await _apiService.PostAsync<Product>("product", product);
    }

    public async Task<ApiResponse<Product>> UpdateProductAsync(UpdateProductDto product)
    {
        return await _apiService.PutAsync<Product>($"product/{product.Id}", product);
    }

    public async Task<ApiResponse<bool>> DeleteProductAsync(int id)
    {
        return await _apiService.DeleteAsync<bool>($"product/{id}");
    }

    public async Task<ApiResponse<List<Category>>> GetCategoriesAsync()
    {
        return await _apiService.GetAsync<List<Category>>("category");
    }
}
