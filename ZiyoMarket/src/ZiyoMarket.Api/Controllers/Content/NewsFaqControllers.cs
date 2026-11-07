using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Api.Helpers;
using ZiyoMarket.Domain.Common;
using ZiyoMarket.Service.DTOs.Content;
using ZiyoMarket.Service.Interfaces;

namespace ZiyoMarket.Api.Controllers.Content;

/// <summary>
/// Yangiliklar. O'qish hamma uchun (faqat nashr qilinganlar), admin qoralamalarni ham ko'radi.
/// </summary>
[ApiController]
[Route("api/news")]
public class NewsController : BaseController
{
    private readonly INewsService _newsService;

    public NewsController(INewsService newsService)
    {
        _newsService = newsService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var result = await _newsService.GetAllAsync(includeUnpublished: GetCurrentUserType() == "Admin");
        return StatusCode(result.StatusCode, new { success = result.IsSuccess, message = result.Message, data = result.Data });
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _newsService.GetByIdAsync(id, includeUnpublished: GetCurrentUserType() == "Admin");
        return StatusCode(result.StatusCode, new { success = result.IsSuccess, message = result.Message, data = result.Data });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [RequirePermission(AdminPermissions.ContentManage)]
    public async Task<IActionResult> Create([FromBody] SaveNewsDto dto)
    {
        var result = await _newsService.CreateAsync(dto, GetCurrentUserId());
        return StatusCode(result.StatusCode, new { success = result.IsSuccess, message = result.Message, data = result.Data });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [RequirePermission(AdminPermissions.ContentManage)]
    public async Task<IActionResult> Update(int id, [FromBody] SaveNewsDto dto)
    {
        var result = await _newsService.UpdateAsync(id, dto, GetCurrentUserId());
        return StatusCode(result.StatusCode, new { success = result.IsSuccess, message = result.Message, data = result.Data });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [RequirePermission(AdminPermissions.ContentManage)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _newsService.DeleteAsync(id, GetCurrentUserId());
        return StatusCode(result.StatusCode, new { success = result.IsSuccess, message = result.Message, data = result.IsSuccess });
    }
}

/// <summary>
/// Ko'p so'raladigan savollar. O'qish hamma uchun (faqat faollari), admin yashirilganlarni ham ko'radi.
/// </summary>
[ApiController]
[Route("api/faqs")]
public class FaqController : BaseController
{
    private readonly IFaqService _faqService;

    public FaqController(IFaqService faqService)
    {
        _faqService = faqService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var result = await _faqService.GetAllAsync(includeInactive: GetCurrentUserType() == "Admin");
        return StatusCode(result.StatusCode, new { success = result.IsSuccess, message = result.Message, data = result.Data });
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _faqService.GetByIdAsync(id);
        return StatusCode(result.StatusCode, new { success = result.IsSuccess, message = result.Message, data = result.Data });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [RequirePermission(AdminPermissions.ContentManage)]
    public async Task<IActionResult> Create([FromBody] SaveFaqDto dto)
    {
        var result = await _faqService.CreateAsync(dto, GetCurrentUserId());
        return StatusCode(result.StatusCode, new { success = result.IsSuccess, message = result.Message, data = result.Data });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [RequirePermission(AdminPermissions.ContentManage)]
    public async Task<IActionResult> Update(int id, [FromBody] SaveFaqDto dto)
    {
        var result = await _faqService.UpdateAsync(id, dto, GetCurrentUserId());
        return StatusCode(result.StatusCode, new { success = result.IsSuccess, message = result.Message, data = result.Data });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [RequirePermission(AdminPermissions.ContentManage)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _faqService.DeleteAsync(id, GetCurrentUserId());
        return StatusCode(result.StatusCode, new { success = result.IsSuccess, message = result.Message, data = result.IsSuccess });
    }
}
