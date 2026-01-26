using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Service.DTOs.Products;
using ZiyoMarket.Service.Interfaces;

namespace ZiyoMarket.Api.Controllers.Products;

/// <summary>
/// Ichki kategoriyalar uchun controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SubcategoryController : BaseController
{
    private readonly ICategoryService _categoryService;

    public SubcategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Barcha ichki kategoriyalarni olish
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllSubcategories()
    {
        var result = await _categoryService.GetAllCategoriesAsync();

        // Faqat ichki kategoriyalarni filtrlash (ParentId != null)
        var subcategories = result.Data?.Where(c => c.ParentId != null).ToList();

        return Ok(new { success = true, data = subcategories });
    }

    /// <summary>
    /// ID bo'yicha ichki kategoriyani olish
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSubcategoryById(int id)
    {
        var result = await _categoryService.GetCategoryByIdAsync(id);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        // Ichki kategoriya ekanligini tekshirish
        if (result.Data?.ParentId == null)
            return BadRequest(new { success = false, message = "Bu asosiy kategoriya, ichki kategoriya emas" });

        return Ok(new { success = true, data = result.Data });
    }

    /// <summary>
    /// Muayyan asosiy kategoriyaga tegishli ichki kategoriyalarni olish
    /// </summary>
    [HttpGet("by-parent/{parentId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSubcategoriesByParent(int parentId)
    {
        var result = await _categoryService.GetSubCategoriesAsync(parentId);
        return Ok(new { success = true, data = result.Data });
    }

    /// <summary>
    /// Yangi ichki kategoriya yaratish
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateSubcategory([FromBody] SaveSubcategoryDto request)
    {
        if (!request.ParentId.HasValue)
            return BadRequest(new { success = false, message = "Ichki kategoriya uchun ParentId majburiy" });

        // Asosiy kategoriya mavjudligini tekshirish
        var parentResult = await _categoryService.GetCategoryByIdAsync(request.ParentId.Value);
        if (!parentResult.IsSuccess)
            return BadRequest(new { success = false, message = "Asosiy kategoriya topilmadi" });

        if (parentResult.Data?.ParentId != null)
            return BadRequest(new { success = false, message = "Faqat asosiy kategoriyaga ichki kategoriya qo'shish mumkin (nested subcategory yaratib bo'lmaydi)" });

        var categoryDto = new SaveCategoryDto
        {
            Name = request.Name,
            Description = request.Description,
            ParentId = request.ParentId,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive
        };

        var createdBy = GetCurrentUserId();
        var result = await _categoryService.CreateCategoryAsync(categoryDto, createdBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return StatusCode(201, new { success = true, message = result.Message, data = result.Data });
    }

    /// <summary>
    /// Ichki kategoriyani tahrirlash
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateSubcategory(int id, [FromBody] SaveSubcategoryDto request)
    {
        // Mavjud kategoriyani tekshirish
        var existingResult = await _categoryService.GetCategoryByIdAsync(id);
        if (!existingResult.IsSuccess)
            return StatusCode(existingResult.StatusCode, new { message = existingResult.Message });

        if (existingResult.Data?.ParentId == null)
            return BadRequest(new { success = false, message = "Bu asosiy kategoriya, ichki kategoriya emas" });

        if (!request.ParentId.HasValue)
            return BadRequest(new { success = false, message = "Ichki kategoriya uchun ParentId majburiy" });

        // Yangi parent kategoriyani tekshirish
        var parentResult = await _categoryService.GetCategoryByIdAsync(request.ParentId.Value);
        if (!parentResult.IsSuccess)
            return BadRequest(new { success = false, message = "Asosiy kategoriya topilmadi" });

        if (parentResult.Data?.ParentId != null)
            return BadRequest(new { success = false, message = "Faqat asosiy kategoriyaga ichki kategoriya bog'lash mumkin" });

        var categoryDto = new SaveCategoryDto
        {
            Name = request.Name,
            Description = request.Description,
            ParentId = request.ParentId,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive
        };

        var updatedBy = GetCurrentUserId();
        var result = await _categoryService.UpdateCategoryAsync(id, categoryDto, updatedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message, data = result.Data });
    }

    /// <summary>
    /// Ichki kategoriyani o'chirish
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteSubcategory(int id)
    {
        // Ichki kategoriya ekanligini tekshirish
        var existingResult = await _categoryService.GetCategoryByIdAsync(id);
        if (!existingResult.IsSuccess)
            return StatusCode(existingResult.StatusCode, new { message = existingResult.Message });

        if (existingResult.Data?.ParentId == null)
            return BadRequest(new { success = false, message = "Bu asosiy kategoriya, ichki kategoriya emas. /api/category/{id} orqali o'chiring" });

        var deletedBy = GetCurrentUserId();
        var result = await _categoryService.DeleteCategoryAsync(id, deletedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    /// <summary>
    /// Ichki kategoriya statusini o'zgartirish (Active/Inactive)
    /// </summary>
    [HttpPost("{id}/toggle-status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        // Ichki kategoriya ekanligini tekshirish
        var existingResult = await _categoryService.GetCategoryByIdAsync(id);
        if (!existingResult.IsSuccess)
            return StatusCode(existingResult.StatusCode, new { message = existingResult.Message });

        if (existingResult.Data?.ParentId == null)
            return BadRequest(new { success = false, message = "Bu asosiy kategoriya, ichki kategoriya emas" });

        var updatedBy = GetCurrentUserId();
        var result = await _categoryService.ToggleCategoryStatusAsync(id, updatedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }
}

/// <summary>
/// Ichki kategoriya yaratish/tahrirlash uchun DTO
/// </summary>
public class SaveSubcategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ParentId { get; set; } // Ichki kategoriya uchun majburiy
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}
