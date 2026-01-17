using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Service.Interfaces;

namespace ZiyoMarket.Api.Controllers;

/// <summary>
/// Permission management controller - Admin panel uchun
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class PermissionController : BaseController
{
    private readonly IPermissionManagementService _permissionService;

    public PermissionController(IPermissionManagementService permissionService)
    {
        _permissionService = permissionService;
    }

    /// <summary>
    /// Barcha permissionlarni olish
    /// GET /api/permission
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllPermissions()
    {
        var result = await _permissionService.GetAllPermissionsAsync();

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { success = false, message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    /// <summary>
    /// Modul bo'yicha permissionlarni olish
    /// GET /api/permission/module/{module}
    /// </summary>
    [HttpGet("module/{module}")]
    public async Task<IActionResult> GetPermissionsByModule(string module)
    {
        var result = await _permissionService.GetPermissionsByModuleAsync(module);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { success = false, message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    /// <summary>
    /// Yangi permission yaratish (SuperAdmin only)
    /// POST /api/permission
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionRequest request)
    {
        var createdBy = GetCurrentUserId();
        var dto = new ZiyoMarket.Service.DTOs.Permissions.CreatePermissionDto
        {
            Name = request.Name,
            DisplayName = request.DisplayName,
            Description = request.Description,
            Module = request.Module
        };
        var result = await _permissionService.CreatePermissionAsync(dto, createdBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { success = false, message = result.Message });

        return StatusCode(201, new { success = true, message = result.Message, data = result.Data });
    }

    /// <summary>
    /// Permission yangilash (SuperAdmin only)
    /// PUT /api/permission/{id}
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdatePermission(int id, [FromBody] UpdatePermissionRequest request)
    {
        var updatedBy = GetCurrentUserId();
        var dto = new ZiyoMarket.Service.DTOs.Permissions.UpdatePermissionDto
        {
            Name = request.Name,
            DisplayName = request.DisplayName,
            Description = request.Description,
            Module = request.Module,
            IsActive = request.IsActive
        };
        var result = await _permissionService.UpdatePermissionAsync(id, dto, updatedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { success = false, message = result.Message });

        return Ok(new { success = true, message = result.Message, data = result.Data });
    }

    /// <summary>
    /// Permission o'chirish (SuperAdmin only)
    /// DELETE /api/permission/{id}
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeletePermission(int id)
    {
        var deletedBy = GetCurrentUserId();
        var result = await _permissionService.DeletePermissionAsync(id, deletedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { success = false, message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }
}

// Request DTOs
public class CreatePermissionRequest
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
}

public class UpdatePermissionRequest
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
