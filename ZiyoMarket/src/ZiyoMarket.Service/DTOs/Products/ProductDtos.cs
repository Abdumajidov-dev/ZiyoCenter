using System;
using System.ComponentModel.DataAnnotations;
using ZiyoMarket.Service.DTOs.Common;

namespace ZiyoMarket.Service.DTOs.Products;

/// <summary>
/// Product list DTO
/// </summary>
public class ProductListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string QRCode { get; set; } = string.Empty;
    public List<int> CategoryIds { get; set; } = new();
    public List<string> CategoryNames { get; set; } = new();

    public decimal Price { get; set; }
    public string FormattedPrice => $"{Price:N0} so'm";
    public int StockQuantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<string> ImageUrls { get; set; } = new();
    public bool IsAvailable { get; set; }
    public bool IsLowStock { get; set; }
    public int LikesCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
}

/// <summary>
/// Product detail DTO
/// </summary>
public class ProductDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string QRCode { get; set; } = string.Empty;
    public List<int> CategoryIds { get; set; } = new();
    public List<string> CategoryNames { get; set; } = new();
    public List<string> CategoryPaths { get; set; } = new();

    public decimal Price { get; set; }
    public string FormattedPrice => $"{Price:N0} so'm";
    public int StockQuantity { get; set; }
    public int MinStockLevel { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<string> ImageUrls { get; set; } = new();
    public decimal? Weight { get; set; }
    public string? Dimensions { get; set; }
    public string? SKU { get; set; }
    public string? Barcode { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsLowStock { get; set; }
    public bool IsOutOfStock { get; set; }
    public int LikesCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Create product DTO
/// </summary>
public class CreateProductDto
{
    [Required(ErrorMessage = "Product name is required")]
    [MaxLength(300)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [Required(ErrorMessage = "QR code is required")]
    [MaxLength(100)]
    public string QRCode { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "At least one category is required")]
    [MinLength(1, ErrorMessage = "At least one category is required")]
    public List<int> CategoryIds { get; set; } = new();

    
    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, 999999999.99, ErrorMessage = "Price must be between 0.01 and 999999999.99")]
    public decimal Price { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
    public int StockQuantity { get; set; } = 0;
    
    [Range(0, int.MaxValue, ErrorMessage = "Min stock level cannot be negative")]
    public int MinStockLevel { get; set; } = 5;
    
    public List<string> ImageUrls { get; set; } = new();
    
    [Range(0, double.MaxValue, ErrorMessage = "Weight cannot be negative")]
    public decimal? Weight { get; set; }
    
    [MaxLength(100)]
    public string? Dimensions { get; set; }
    
    [MaxLength(100)]
    public string? SKU { get; set; }
    
    [MaxLength(100)]
    public string? Barcode { get; set; }
}

/// <summary>
/// Update product DTO
/// </summary>
public class UpdateProductDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Product name is required")]
    [MaxLength(300)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [Required(ErrorMessage = "At least one category is required")]
    [MinLength(1, ErrorMessage = "At least one category is required")]
    public List<int> CategoryIds { get; set; } = new();

    
    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, 999999999.99, ErrorMessage = "Price must be between 0.01 and 999999999.99")]
    public decimal Price { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Min stock level cannot be negative")]
    public int MinStockLevel { get; set; }
    
    public List<string> ImageUrls { get; set; } = new();
    
    [Range(0, double.MaxValue, ErrorMessage = "Weight cannot be negative")]
    public decimal? Weight { get; set; }
    
    [MaxLength(100)]
    public string? Dimensions { get; set; }
}

/// <summary>
/// Update stock DTO
/// </summary>
public class UpdateStockDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
    
    [Required]
    public string Operation { get; set; } = "add"; // add, remove, set
    
    [MaxLength(300)]
    public string? Reason { get; set; }
}

/// <summary>
/// Low stock product DTO
/// </summary>
public class LowStockProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public int MinStockLevel { get; set; }
    public int NeededQuantity => MinStockLevel - StockQuantity;
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Product filter request
/// </summary>
public class ProductFilterRequest : BaseFilterRequest
{
    public List<int>? CategoryIds { get; set; }

    public string? Status { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStock { get; set; }
    public bool? LowStock { get; set; }
}

/// <summary>
/// Category DTO
/// </summary>
public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ParentId { get; set; }
    public string? ParentName { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public int ProductCount { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CategoryDto> Children { get; set; } = new();
}

/// <summary>
/// Save category DTO
/// </summary>
public class SaveCategoryDto
{
    [Required(ErrorMessage = "Category name is required")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public int? ParentId { get; set; }
    
    public int SortOrder { get; set; } = 0;
    
    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? ImageUrl { get; set; }
}
/// <summary>
/// Kategoriya va unga tegishli mahsulotlar soni haqida ma�lumot DTO
/// </summary>
public class CategoryWithProductCountDto
{
    /// <summary>
    /// Kategoriya ID raqami
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Kategoriya nomi
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Kategoriyaning ota kategoriyasi ID raqami
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// Ota kategoriya nomi
    /// </summary>
    public string? ParentName { get; set; }

    /// <summary>
    /// Kategoriyadagi faol mahsulotlar soni
    /// </summary>
    public int ActiveProductCount { get; set; }

    /// <summary>
    /// Kategoriyadagi umumiy mahsulotlar soni
    /// </summary>
    public int TotalProductCount { get; set; }

    /// <summary>
    /// Kategoriya faolmi yoki yo�qmi
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Oxirgi yangilangan sana
    /// </summary>
    public string? UpdatedAt { get; set; }
}
/// <summary>
/// Kategoriyalar tartibini (sort order) o�zgartirish uchun DTO
/// </summary>
public class ReorderCategoryDto
{
    /// <summary>
    /// Kategoriya ID
    /// </summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int Id { get; set; }

    /// <summary>
    /// Yangi tartib raqami (sort order)
    /// </summary>
    [Required]
    [Range(0, int.MaxValue)]
    public int SortOrder { get; set; }

    /// <summary>
    /// Ota kategoriya ID (agar mavjud bo�lsa)
    /// </summary>
    public int? ParentId { get; set; }
}