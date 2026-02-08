using System.ComponentModel.DataAnnotations;

namespace ZiyoMarket.Service.DTOs.Products;

/// <summary>
/// Category list DTO
/// </summary>
public class CategoryListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ParentId { get; set; }
    public string? ParentName { get; set; }
    public string? ImageUrl { get; set; }
    public string? Image { get; set; } // Full URL for display
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public int ProductCount { get; set; }
    public int ChildrenCount { get; set; }
    public bool IsRootCategory { get; set; }
    public string FullPath { get; set; } = string.Empty;
}

/// <summary>
/// Category detail DTO
/// </summary>
public class CategoryDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ParentId { get; set; }
    public string? ParentName { get; set; }
    public string? ImageUrl { get; set; }
    public string? Image { get; set; } // Full URL for display
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public string FullPath { get; set; } = string.Empty;
    public List<CategoryListDto> Children { get; set; } = new();
    public int ProductCount { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string? UpdatedAt { get; set; }
}

/// <summary>
/// Create category DTO
/// </summary>
public class CreateCategoryDto
{
    [Required(ErrorMessage = "Category name is required")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int? ParentId { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Update category DTO
/// </summary>
public class UpdateCategoryDto
{
    [Required(ErrorMessage = "Category ID is required")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Category name is required")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int? ParentId { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Category with image upload DTO
/// </summary>
public class CategoryWithImageDto
{
    public int? Id { get; set; } // Null for create, value for update
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ParentId { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    // Image file will be sent separately as IFormFile
}
