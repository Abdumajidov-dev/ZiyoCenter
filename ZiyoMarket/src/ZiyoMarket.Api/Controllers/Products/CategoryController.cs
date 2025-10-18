using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Service.DTOs.Products;
using ZiyoMarket.Service.Interfaces;

namespace ZiyoMarket.Api.Controllers.Products;


[ApiController]
[Route("api/[controller]")]
public class CategoryController : BaseController
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategoryById(int id)
    {
        var result = await _categoryService.GetCategoryByIdAsync(id);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllCategories()
    {
        var result = await _categoryService.GetAllCategoriesAsync();
        return Ok(new { success = true, data = result.Data });
    }

    [HttpGet("root")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRootCategories()
    {
        var result = await _categoryService.GetRootCategoriesAsync();
        return Ok(new { success = true, data = result.Data });
    }

    [HttpGet("{parentId}/subcategories")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSubCategories(int parentId)
    {
        var result = await _categoryService.GetSubCategoriesAsync(parentId);
        return Ok(new { success = true, data = result.Data });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCategory([FromBody] SaveCategoryDto request)
    {
        var createdBy = GetCurrentUserId();
        var result = await _categoryService.CreateCategoryAsync(request, createdBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return StatusCode(201, new { success = true, message = result.Message, data = result.Data });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] SaveCategoryDto request)
    {
        var updatedBy = GetCurrentUserId();
        var result = await _categoryService.UpdateCategoryAsync(id, request, updatedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message, data = result.Data });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var deletedBy = GetCurrentUserId();
        var result = await _categoryService.DeleteCategoryAsync(id, deletedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpGet("tree")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategoryTree()
    {
        var result = await _categoryService.GetCategoryTreeAsync();
        return Ok(new { success = true, data = result.Data });
    }

    [HttpPost("reorder")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ReorderCategories([FromBody] List<ReorderCategoryDto> categories)
    {
        var updatedBy = GetCurrentUserId();
        var result = await _categoryService.ReorderCategoriesAsync(categories, updatedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpPost("{id}/toggle-status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var updatedBy = GetCurrentUserId();
        var result = await _categoryService.ToggleCategoryStatusAsync(id, updatedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpDelete("bulk")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAllCategories([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var deletedBy = GetCurrentUserId();
        var result = await _categoryService.DeleteAllCategoriesAsync(deletedBy, startDate, endDate);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpPost("seed")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SeedMockCategories([FromQuery] int count = 10)
    {
        var createdBy = GetCurrentUserId();
        var result = await _categoryService.SeedMockCategoriesAsync(createdBy, count);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message, data = result.Data });
    }
}

