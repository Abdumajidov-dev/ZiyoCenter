using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Service.DTOs.Content;
using ZiyoMarket.Service.Interfaces;

namespace ZiyoMarket.Api.Controllers.Content;

[ApiController]
[Route("api/[controller]")]
public class ContentController : BaseController
{
    private readonly IContentService _contentService;

    public ContentController(IContentService contentService)
    {
        _contentService = contentService;
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetContentById(int id)
    {
        var result = await _contentService.GetContentByIdAsync(id);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        // Increment view count
        await _contentService.IncrementViewCountAsync(id);

        return Ok(new { success = true, data = result.Data });
    }

    //[HttpGet]
    //[Authorize(Roles = "Admin")]
    //public async Task<IActionResult> GetAllContent()
    //{
    //    var result = await _contentService.GetAllContentAsync();
    //    return Ok(new { success = true, data = result.Data });
    //}

    [HttpGet("published")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublishedContent()
    {
        var result = await _contentService.GetPublishedContentAsync();
        return Ok(new { success = true, data = result.Data });
    }

    [HttpGet("type/{contentType}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetContentByType(string contentType)
    {
        var result = await _contentService.GetContentByTypeAsync(contentType);
        return Ok(new { success = true, data = result.Data });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateContent([FromBody] SaveContentDto request)
    {
        var createdBy = GetCurrentUserId();
        var result = await _contentService.CreateContentAsync(request, createdBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return StatusCode(201, new { success = true, message = result.Message, data = result.Data });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateContent(int id, [FromBody] SaveContentDto request)
    {
        var updatedBy = GetCurrentUserId();
        var result = await _contentService.UpdateContentAsync(id, request, updatedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message, data = result.Data });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteContent(int id)
    {
        var deletedBy = GetCurrentUserId();
        var result = await _contentService.DeleteContentAsync(id, deletedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpPost("{id}/publish")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PublishContent(int id)
    {
        var publishedBy = GetCurrentUserId();
        var result = await _contentService.PublishContentAsync(id, publishedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpPost("{id}/unpublish")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UnpublishContent(int id)
    {
        var unpublishedBy = GetCurrentUserId();
        var result = await _contentService.UnpublishContentAsync(id, unpublishedBy);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpDelete("bulk")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAllContent([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var deletedBy = GetCurrentUserId();
        var result = await _contentService.DeleteAllContentAsync(deletedBy, startDate, endDate);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpPost("seed")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SeedMockContent([FromQuery] int count = 10)
    {
        var createdBy = GetCurrentUserId();
        var result = await _contentService.SeedMockContentAsync(createdBy, count);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message, data = result.Data });
    }
}
