using System.Collections.Generic;
using System.Threading.Tasks;
using ZiyoMarket.Service.DTOs.Products;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Interfaces
{
    public interface IProductService
    {
        // ============ CRUD Operations ============

        Task<Result<ProductDetailDto>> GetProductByIdAsync(int productId, int? customerId = null);

        Task<Result<ProductDetailDto>> GetProductByQRCodeAsync(string qrCode, int? customerId = null);

        Task<Result<PaginationResponse<ProductListDto>>> GetProductsAsync(
            ProductFilterRequest request, int? customerId = null);

        Task<Result<ProductDetailDto>> CreateProductAsync(CreateProductDto request, int createdBy);

        Task<Result<ProductDetailDto>> UpdateProductAsync(UpdateProductDto request, int updatedBy);

        Task<Result> DeleteProductAsync(int productId, int deletedBy);

        // ============ Stock Management ============

        Task<Result> UpdateStockAsync(UpdateStockDto request, int updatedBy);

        Task<Result> AddStockAsync(int productId, int quantity, string? reason, int updatedBy);

        Task<Result> RemoveStockAsync(int productId, int quantity, string? reason, int updatedBy);

        Task<Result<List<LowStockProductDto>>> GetLowStockProductsAsync();

        // ============ Search and Filter ============

        Task<Result<List<ProductListDto>>> SearchProductsAsync(
            string searchTerm, int? categoryId = null, int? customerId = null);

        Task<Result<List<ProductListDto>>> GetProductsByCategoryAsync(
            int categoryId, int? customerId = null);

        Task<Result<List<ProductListDto>>> GetFeaturedProductsAsync(int count = 10);

        Task<Result<List<ProductListDto>>> GetNewArrivalsAsync(int count = 10);

        // ============ Product Likes ============

        Task<Result> ToggleLikeAsync(int productId, int customerId);

        Task<Result<List<ProductListDto>>> GetLikedProductsAsync(int customerId);
    }
}
