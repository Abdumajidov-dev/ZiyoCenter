using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Service.DTOs.Products;
using ZiyoMarket.Service.Interfaces;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Api.Controllers.Products;

/// <summary>
/// Product management endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductController : BaseController
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    // ===================== CRUD =====================

    /// <summary>
    /// Get product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <response code="200">Product retrieved successfully</response>
    /// <response code="404">Product not found</response>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById(int id)
    {
        var customerId = IsAuthenticated() ? GetCurrentUserId() : (int?)null;
        var result = await _productService.GetProductByIdAsync(id, customerId);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new
        {
            success = true,
            data = result.Data
        });
    }

    /// <summary>
    /// Get product by QR code
    /// </summary>
    /// <param name="qrCode">Product QR code</param>
    /// <response code="200">Product retrieved successfully</response>
    /// <response code="404">Product not found</response>
    [HttpGet("qr/{qrCode}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductByQRCode(string qrCode)
    {
        var customerId = IsAuthenticated() ? GetCurrentUserId() : (int?)null;
        var result = await _productService.GetProductByQRCodeAsync(qrCode, customerId);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new
        {
            success = true,
            data = result.Data
        });
    }

    /// <summary>
    /// Get paginated list of products with filters
    /// </summary>
    /// <param name="request">Filter and pagination parameters</param>
    /// <response code="200">Products retrieved successfully</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PaginationResponse<ProductListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts([FromQuery] ProductFilterRequest request)
    {
        var customerId = IsAuthenticated() ? GetCurrentUserId() : (int?)null;
        var result = await _productService.GetProductsAsync(request, customerId);

        return Ok(new
        {
            success = true,
            data = result.Data
        });
    }

    /// <summary>
    /// Search products by name, description, or QR code
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="categoryId">Optional category filter</param>
    /// <response code="200">Search results</response>
    [HttpGet("search")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<ProductListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchProducts([FromQuery] string searchTerm, [FromQuery] int? categoryId = null)
    {
        var customerId = IsAuthenticated() ? GetCurrentUserId() : (int?)null;
        var result = await _productService.SearchProductsAsync(searchTerm, categoryId, customerId);

        return Ok(new
        {
            success = true,
            data = result.Data
        });
    }

    /// <summary>
    /// Get products by category
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <response code="200">Products retrieved successfully</response>
    [HttpGet("category/{categoryId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<ProductListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductsByCategory(int categoryId)
    {
        var customerId = IsAuthenticated() ? GetCurrentUserId() : (int?)null;
        var result = await _productService.GetProductsByCategoryAsync(categoryId, customerId);

        return Ok(new
        {
            success = true,
            data = result.Data
        });
    }

    /// <summary>
    /// Create new product (Admin only)
    /// </summary>
    /// <param name="request">Product details</param>
    /// <response code="201">Product created successfully</response>
    /// <response code="409">QR code already exists</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto request)
    {
        var createdBy = GetCurrentUserId();
        var result = await _productService.CreateProductAsync(request, createdBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return StatusCode(StatusCodes.Status201Created, new
        {
            success = true,
            message = result.Message,
            data = result.Data
        });
    }

    /// <summary>
    /// Update existing product (Admin only)
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="request">Updated product details</param>
    /// <response code="200">Product updated successfully</response>
    /// <response code="404">Product not found</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto request)
    {
        request.Id = id; // Ensure ID from route is used
        var updatedBy = GetCurrentUserId();
        var result = await _productService.UpdateProductAsync(request, updatedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new
        {
            success = true,
            message = result.Message,
            data = result.Data
        });
    }

    /// <summary>
    /// Delete product (Admin only)
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <response code="200">Product deleted successfully</response>
    /// <response code="404">Product not found</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var deletedBy = GetCurrentUserId();
        var result = await _productService.DeleteProductAsync(id, deletedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new
        {
            success = true,
            message = result.Message
        });
    }

    // ===================== STOCK MANAGEMENT =====================

    /// <summary>
    /// Update product stock (Admin/Seller)
    /// </summary>
    /// <param name="request">Stock update details</param>
    /// <response code="200">Stock updated successfully</response>
    /// <response code="404">Product not found</response>
    [HttpPost("stock/update")]
    [Authorize(Roles = "Admin,Seller")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStock([FromBody] UpdateStockDto request)
    {
        var updatedBy = GetCurrentUserId();
        var result = await _productService.UpdateStockAsync(request, updatedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new
        {
            success = true,
            message = result.Message
        });
    }

    /// <summary>
    /// Add stock to product (Admin/Seller)
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="quantity">Quantity to add</param>
    /// <param name="reason">Reason for adding stock</param>
    /// <response code="200">Stock added successfully</response>
    [HttpPost("{productId}/stock/add")]
    [Authorize(Roles = "Admin,Seller")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AddStock(int productId, [FromQuery] int quantity, [FromQuery] string? reason = null)
    {
        var updatedBy = GetCurrentUserId();
        var result = await _productService.AddStockAsync(productId, quantity, reason, updatedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new
        {
            success = true,
            message = result.Message
        });
    }

    /// <summary>
    /// Remove stock from product (Admin/Seller)
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="quantity">Quantity to remove</param>
    /// <param name="reason">Reason for removing stock</param>
    /// <response code="200">Stock removed successfully</response>
    [HttpPost("{productId}/stock/remove")]
    [Authorize(Roles = "Admin,Seller")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveStock(int productId, [FromQuery] int quantity, [FromQuery] string? reason = null)
    {
        var updatedBy = GetCurrentUserId();
        var result = await _productService.RemoveStockAsync(productId, quantity, reason, updatedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new
        {
            success = true,
            message = result.Message
        });
    }

    /// <summary>
    /// Get products with low stock (Admin/Seller)
    /// </summary>
    /// <response code="200">Low stock products retrieved</response>
    [HttpGet("stock/low")]
    [Authorize(Roles = "Admin,Seller")]
    [ProducesResponseType(typeof(List<LowStockProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLowStockProducts()
    {
        var result = await _productService.GetLowStockProductsAsync();

        return Ok(new
        {
            success = true,
            data = result.Data
        });
    }

    // ===================== PRODUCT LIKES =====================

    /// <summary>
    /// Toggle like/unlike for product (Customer only)
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <response code="200">Product liked/unliked successfully</response>
    [HttpPost("{productId}/like")]
    [Authorize(Roles = "Customer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ToggleLike(int productId)
    {
        var customerId = GetCurrentUserId();
        var result = await _productService.ToggleLikeAsync(productId, customerId);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new
        {
            success = true,
            message = result.Message
        });
    }

    /// <summary>
    /// Get liked products for current customer
    /// </summary>
    /// <response code="200">Liked products retrieved</response>
    [HttpGet("liked")]
    [Authorize(Roles = "Customer")]
    [ProducesResponseType(typeof(List<ProductListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLikedProducts()
    {
        var customerId = GetCurrentUserId();
        var result = await _productService.GetLikedProductsAsync(customerId);

        return Ok(new
        {
            success = true,
            data = result.Data
        });
    }

    // ===================== SPECIAL LISTS =====================

    /// <summary>
    /// Get featured products (most liked)
    /// </summary>
    /// <param name="count">Number of products to return</param>
    /// <response code="200">Featured products retrieved</response>
    [HttpGet("featured")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<ProductListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFeaturedProducts([FromQuery] int count = 10)
    {
        var result = await _productService.GetFeaturedProductsAsync(count);

        return Ok(new
        {
            success = true,
            data = result.Data
        });
    }

    /// <summary>
    /// Get new arrivals (recently added products)
    /// </summary>
    /// <param name="count">Number of products to return</param>
    /// <response code="200">New arrivals retrieved</response>
    [HttpGet("new-arrivals")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<ProductListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNewArrivals([FromQuery] int count = 10)
    {
        var result = await _productService.GetNewArrivalsAsync(count);

        return Ok(new
        {
            success = true,
            data = result.Data
        });
    }
}
