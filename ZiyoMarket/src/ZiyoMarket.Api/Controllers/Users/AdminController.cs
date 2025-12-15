using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Service.DTOs.Admins;
using ZiyoMarket.Service.DTOs.Sellers;
using ZiyoMarket.Service.Interfaces;

namespace ZiyoMarket.Api.Controllers.Users;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin")]
public class AdminController : BaseController
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAdminById(int id)
    {
        var result = await _adminService.GetAdminByIdAsync(id);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    [HttpGet]
    public async Task<IActionResult> GetAdmins([FromQuery] AdminFilterRequest request)
    {
        var result = await _adminService.GetAdminsAsync(request);
        return Ok(new { success = true, data = result.Data });
    }

    [HttpPost]
    public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminDto request)
    {
        var createdBy = GetCurrentUserId();
        var result = await _adminService.CreateAdminAsync(request, createdBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return StatusCode(201, new { success = true, message = result.Message, data = result.Data });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAdmin(int id, [FromBody] UpdateAdminDto request)
    {
        var updatedBy = GetCurrentUserId();
        var result = await _adminService.UpdateAdminAsync(id, request, updatedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message, data = result.Data });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAdmin(int id)
    {
        var deletedBy = GetCurrentUserId();
        var result = await _adminService.DeleteAdminAsync(id, deletedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpPost("{id}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var updatedBy = GetCurrentUserId();
        var result = await _adminService.ToggleAdminStatusAsync(id, updatedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpPut("{id}/change-role")]
    public async Task<IActionResult> ChangeRole(int id, [FromBody] ChangeRoleRequest request)
    {
        var updatedBy = GetCurrentUserId();
        var result = await _adminService.ChangeAdminRoleAsync(id, request.NewRole, updatedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchAdmins([FromQuery] string searchTerm)
    {
        var result = await _adminService.SearchAdminsAsync(searchTerm);
        return Ok(new { success = true, data = result.Data });
    }
}
