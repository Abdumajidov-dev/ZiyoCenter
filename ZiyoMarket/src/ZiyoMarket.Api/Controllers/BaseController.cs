using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ZiyoMarket.Api.Common;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Get current user ID from JWT token
    /// </summary>
    protected int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    /// <summary>
    /// Get current user type from JWT token
    /// </summary>
    protected string GetCurrentUserType()
    {
        return User.FindFirst("UserType")?.Value ?? "";
    }

    /// <summary>
    /// Check if current user is authenticated
    /// </summary>
    protected bool IsAuthenticated()
    {
        return User.Identity?.IsAuthenticated ?? false;
    }

    /// <summary>
    /// Return success response with data
    /// </summary>
    protected IActionResult SuccessResponse<T>(T data, string message = "Success")
    {
        return Ok(ApiResponse<T>.Success(data, message));
    }

    /// <summary>
    /// Return success response without data
    /// </summary>
    protected IActionResult SuccessResponse(string message = "Success")
    {
        return Ok(ApiResponse.Success(message));
    }

    /// <summary>
    /// Return error response
    /// </summary>
    protected IActionResult ErrorResponse(string message)
    {
        return BadRequest(ApiResponse.Failure(message));
    }

    /// <summary>
    /// Return error response with data
    /// </summary>
    protected IActionResult ErrorResponse<T>(string message, T? data = default)
    {
        return BadRequest(ApiResponse<T>.Failure(message, data));
    }

    /// <summary>
    /// Handle service result and return appropriate response
    /// </summary>
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(ApiResponse<T>.Success(result.Data, result.Message));
        }
        return BadRequest(ApiResponse<T>.Failure(result.Message));
    }
}
