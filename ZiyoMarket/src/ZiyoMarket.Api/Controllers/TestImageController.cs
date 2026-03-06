using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Data.UnitOfWorks;
using ZiyoMarket.Domain.Entities.Products;
using ZiyoMarket.Service.Enums;
using ZiyoMarket.Service.Interfaces;

namespace ZiyoMarket.Api.Controllers;

/// <summary>
/// Test controller to verify image upload integration with Products and Categories
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TestImageController : BaseController
{
    private readonly IFileUploadService _fileUploadService;
    private readonly IUnitOfWork _unitOfWork;

    public TestImageController(
        IFileUploadService fileUploadService,
        IUnitOfWork unitOfWork)
    {
        _fileUploadService = fileUploadService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Test: Create a category with image upload
    /// </summary>
    [HttpPost("create-category-with-image")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult> CreateCategoryWithImage(
        string name,
        string? description,
        int? parentId,
        IFormFile? imageFile)
    {
        try
        {
            // Create category
            var category = new Category
            {
                Name = name,
                Description = description,
                ParentId = parentId,
                IsActive = true,
                DisplayOrder = 0
            };

            // Upload image if provided
            if (imageFile != null)
            {
                var uploadResult = await _fileUploadService.UploadImageAsync(imageFile, ImageCategory.Category);
                category.ImageUrl = uploadResult.FilePath; // Save relative path
            }

            // Save to database
            await _unitOfWork.Categories.InsertAsync(category);
            await _unitOfWork.SaveChangesAsync();

            // Generate full image URL for response
            var imageFullUrl = category.ImageUrl != null
                ? _fileUploadService.GetFileUrl(category.ImageUrl)
                : null;

            return Ok(new
            {
                message = "Category created successfully",
                category = new
                {
                    id = category.Id,
                    name = category.Name,
                    description = category.Description,
                    parent_id = category.ParentId,
                    image_url = category.ImageUrl, // Relative path
                    image = imageFullUrl, // Full URL
                    created_at = category.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Test: Create a product with image upload
    /// </summary>
    [HttpPost("create-product-with-image")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult> CreateProductWithImage(
        string name,
        string? description,
        string qrCode,
        decimal price,
        int stockQuantity,
        List<int> categoryIds,
        IFormFile? imageFile)
    {
        try
        {
            // Create product
            var product = new Product
            {
                Name = name,
                Description = description,
                QrCode = qrCode,
                Price = price,
                StockQuantity = stockQuantity,
                IsActive = true,
                MinStockLevel = 5
            };

            // Upload image if provided
            if (imageFile != null)
            {
                var uploadResult = await _fileUploadService.UploadImageAsync(imageFile, ImageCategory.Product);
                product.ImageUrl = uploadResult.FilePath; // Save relative path
            }

            // Save product
            await _unitOfWork.Products.InsertAsync(product);
            await _unitOfWork.SaveChangesAsync();

            // Add categories (many-to-many)
            if (categoryIds != null && categoryIds.Any())
            {
                var productCategories = categoryIds.Select(catId => new ProductCategory
                {
                    ProductId = product.Id,
                    CategoryId = catId
                }).ToList();

                await _unitOfWork.ProductCategories.AddRangeAsync(productCategories);
                await _unitOfWork.SaveChangesAsync();
            }

            // Generate full image URL for response
            var imageFullUrl = product.ImageUrl != null
                ? _fileUploadService.GetFileUrl(product.ImageUrl)
                : null;

            return Ok(new
            {
                message = "Product created successfully",
                product = new
                {
                    id = product.Id,
                    name = product.Name,
                    description = product.Description,
                    qr_code = product.QrCode,
                    price = product.Price,
                    stock_quantity = product.StockQuantity,
                    category_ids = categoryIds,
                    image_url = product.ImageUrl, // Relative path
                    image = imageFullUrl, // Full URL
                    created_at = product.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Test: Update category image
    /// </summary>
    [HttpPut("update-category-image/{categoryId}")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult> UpdateCategoryImage(
        int categoryId,
        IFormFile imageFile)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
            if (category == null)
                return NotFound(new { message = "Category not found" });

            // Delete old image if exists
            if (!string.IsNullOrEmpty(category.ImageUrl))
            {
                await _fileUploadService.DeleteImageAsync(category.ImageUrl);
            }

            // Upload new image
            var uploadResult = await _fileUploadService.UploadImageAsync(imageFile, ImageCategory.Category);
            category.ImageUrl = uploadResult.FilePath;

            await _unitOfWork.Categories.UpdateAsync(category);
            await _unitOfWork.SaveChangesAsync();

            var imageFullUrl = _fileUploadService.GetFileUrl(category.ImageUrl);

            return Ok(new
            {
                message = "Category image updated successfully",
                category = new
                {
                    id = category.Id,
                    name = category.Name,
                    image_url = category.ImageUrl,
                    image = imageFullUrl
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Test: Update product image
    /// </summary>
    [HttpPut("update-product-image/{productId}")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult> UpdateProductImage(
        int productId,
        IFormFile imageFile)
    {
        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            // Delete old image if exists
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                await _fileUploadService.DeleteImageAsync(product.ImageUrl);
            }

            // Upload new image
            var uploadResult = await _fileUploadService.UploadImageAsync(imageFile, ImageCategory.Product);
            product.ImageUrl = uploadResult.FilePath;

            await _unitOfWork.Products.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();

            var imageFullUrl = _fileUploadService.GetFileUrl(product.ImageUrl);

            return Ok(new
            {
                message = "Product image updated successfully",
                product = new
                {
                    id = product.Id,
                    name = product.Name,
                    image_url = product.ImageUrl,
                    image = imageFullUrl
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Test: Get all categories with images
    /// </summary>
    [HttpGet("categories-with-images")]
    public async Task<ActionResult> GetCategoriesWithImages()
    {
        try
        {
            var categories = await _unitOfWork.Categories
                .FindAsync(c => c.DeletedAt == null);

            var result = categories.Select(c => new
            {
                id = c.Id,
                name = c.Name,
                description = c.Description,
                parent_id = c.ParentId,
                image_url = c.ImageUrl, // Relative path
                image = !string.IsNullOrEmpty(c.ImageUrl)
                    ? _fileUploadService.GetFileUrl(c.ImageUrl)
                    : null, // Full URL
                is_active = c.IsActive
            }).ToList();

            return Ok(new
            {
                count = result.Count,
                categories = result
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Test: Get all products with images
    /// </summary>
    [HttpGet("products-with-images")]
    public async Task<ActionResult> GetProductsWithImages()
    {
        try
        {
            var products = await _unitOfWork.Products
                .FindAsync(p => p.DeletedAt == null);

            var result = products.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                description = p.Description,
                qr_code = p.QrCode,
                price = p.Price,
                stock_quantity = p.StockQuantity,
                image_url = p.ImageUrl, // Relative path
                image = !string.IsNullOrEmpty(p.ImageUrl)
                    ? _fileUploadService.GetFileUrl(p.ImageUrl)
                    : null, // Full URL
                is_active = p.IsActive
            }).ToList();

            return Ok(new
            {
                count = result.Count,
                products = result
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
