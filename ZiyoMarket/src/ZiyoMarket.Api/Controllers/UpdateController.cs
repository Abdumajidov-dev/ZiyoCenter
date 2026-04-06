using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Api.Common;
using ZiyoMarket.Service.DTOs.Update;
using ZiyoMarket.Service.Interfaces;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Api.Controllers;

/// <summary>
/// Auto-update system API endpoints
/// </summary>
[ApiController]
[Route("api/update")]
public class UpdateController : BaseController
{
    private readonly IUpdateService _updateService;

    public UpdateController(IUpdateService updateService)
    {
        _updateService = updateService;
    }

    /// <summary>
    /// Check if new version is available
    /// </summary>
    /// <param name="request">Current client version info</param>
    /// <returns>Update information</returns>
    [HttpPost("check")]
    [ProducesResponseType(typeof(ApiResponse<UpdateInfoDto>), 200)]
    public async Task<IActionResult> CheckForUpdates([FromBody] CheckUpdateRequest request)
    {
        var result = await _updateService.CheckForUpdatesAsync(request);
        return HandleResult(result);
    }

    /// <summary>
    /// Get latest version for channel
    /// </summary>
    /// <param name="channel">Release channel (stable, beta, dev)</param>
    /// <param name="platform">Platform (windows, linux, macos)</param>
    /// <returns>Latest version info</returns>
    [HttpGet("latest")]
    [ProducesResponseType(typeof(ApiResponse<AppVersionDto>), 200)]
    public async Task<IActionResult> GetLatestVersion(
        [FromQuery] string channel = "stable",
        [FromQuery] string platform = "windows")
    {
        var result = await _updateService.GetLatestVersionAsync(channel, platform);
        return HandleResult(result);
    }

    /// <summary>
    /// Get all releases
    /// </summary>
    /// <param name="channel">Optional channel filter</param>
    /// <param name="platform">Optional platform filter</param>
    /// <returns>List of releases</returns>
    [HttpGet("releases")]
    [ProducesResponseType(typeof(ApiResponse<List<AppVersionDto>>), 200)]
    public async Task<IActionResult> GetAllReleases(
        [FromQuery] string? channel = null,
        [FromQuery] string? platform = null)
    {
        var result = await _updateService.GetAllReleasesAsync(channel, platform);
        return HandleResult(result);
    }

    /// <summary>
    /// Get version by ID
    /// </summary>
    /// <param name="id">Version ID</param>
    /// <returns>Version details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<AppVersionDto>), 200)]
    public async Task<IActionResult> GetVersionById(int id)
    {
        var result = await _updateService.GetVersionByIdAsync(id);
        return HandleResult(result);
    }

    /// <summary>
    /// Download update file
    /// </summary>
    /// <param name="id">Version ID</param>
    /// <returns>File download</returns>
    [HttpGet("download/{id}")]
    public async Task<IActionResult> DownloadUpdate(int id)
    {
        // Get file path
        var fileResult = await _updateService.GetDownloadFilePathAsync(id);
        if (!fileResult.IsSuccess)
            return NotFound(ErrorResponse(fileResult.Message));

        var filePath = fileResult.Data!;

        // Record download analytics
        var clientVersion = Request.Headers["X-Client-Version"].ToString();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers["User-Agent"].ToString();

        await _updateService.RecordDownloadAsync(id, clientVersion, ipAddress, userAgent);

        // Return file
        var fileName = Path.GetFileName(filePath);
        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
        return File(fileBytes, "application/octet-stream", fileName);
    }

    /// <summary>
    /// Download update by version number
    /// </summary>
    /// <param name="version">Version number (e.g., 1.2.0)</param>
    /// <returns>File download</returns>
    [HttpGet("download/version/{version}")]
    public async Task<IActionResult> DownloadUpdateByVersion(string version)
    {
        // Find version by version number
        var versionResult = await _updateService.GetAllReleasesAsync();
        if (!versionResult.IsSuccess || versionResult.Data == null)
            return NotFound(ErrorResponse("Version not found"));

        var versionDto = versionResult.Data!.FirstOrDefault(v => v.VersionNumber == version);
        if (versionDto == null)
            return NotFound(ErrorResponse("Version not found"));

        return await DownloadUpdate(versionDto.Id);
    }

    // ========== ADMIN ENDPOINTS ==========

    /// <summary>
    /// Create new version (Admin only)
    /// </summary>
    /// <param name="request">Version creation request</param>
    /// <returns>Created version</returns>
    [HttpPost("versions")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(ApiResponse<AppVersionDto>), 201)]
    public async Task<IActionResult> CreateVersion([FromForm] CreateAppVersionRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _updateService.CreateVersionAsync(request, userId);

        if (result.IsSuccess && result.Data != null)
            return Created($"/api/update/{result.Data!.Id}", SuccessResponse(result.Data, result.Message));

        return HandleResult(result);
    }

    /// <summary>
    /// Update version information (Admin only)
    /// </summary>
    /// <param name="id">Version ID</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated version</returns>
    [HttpPut("versions/{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(ApiResponse<AppVersionDto>), 200)]
    public async Task<IActionResult> UpdateVersion(int id, [FromForm] CreateAppVersionRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _updateService.UpdateVersionAsync(id, request, userId);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete version (Admin only)
    /// </summary>
    /// <param name="id">Version ID</param>
    /// <returns>Success response</returns>
    [HttpDelete("versions/{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> DeleteVersion(int id)
    {
        var userId = GetCurrentUserId();
        var result = await _updateService.DeleteVersionAsync(id, userId);

        if (result.IsSuccess)
            return Ok(SuccessResponse(result.Message));

        return BadRequest(ErrorResponse(result.Message));
    }

    /// <summary>
    /// Get download statistics (Admin only)
    /// </summary>
    /// <returns>Download statistics</returns>
    [HttpGet("statistics")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(ApiResponse<UpdateStatisticsDto>), 200)]
    public async Task<IActionResult> GetStatistics()
    {
        var result = await _updateService.GetStatisticsAsync();
        return HandleResult(result);
    }
}
