using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Domain.Enums;
using ZiyoMarket.Service.DTOs.Content;
using ZiyoMarket.Service.Interfaces;

namespace ZiyoMarket.Api.Controllers.Content;

/// <summary>
/// Banner CRUD Controller
/// Banner'lar uchun alohida CRUD operatsiyalari
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BannerController : BaseController
{
    private readonly IContentService _contentService;

    public BannerController(IContentService contentService)
    {
        _contentService = contentService;
    }

    /// <summary>
    /// Barcha banner'larni olish
    /// </summary>
    /// <returns>Banner'lar ro'yxati</returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllBanners()
    {
        var result = await _contentService.GetContentByTypeAsync(ContentType.Banner);

        if (!result.IsSuccess)
            return HandleResult(result);

        return SuccessResponse(result.Data, "Banner'lar muvaffaqiyatli olindi");
    }

    /// <summary>
    /// Faol banner'larni olish (nashr qilingan va amal qilayotganlar)
    /// </summary>
    /// <returns>Faol banner'lar ro'yxati</returns>
    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActiveBanners()
    {
        var result = await _contentService.GetContentByTypeAsync(ContentType.Banner);

        if (!result.IsSuccess)
            return HandleResult(result);

        // Faqat faol va nashr qilingan banner'lar
        var activeBanners = result.Data?
            .Where(b => b.IsActive)
            .OrderBy(b => b.SortOrder)
            .ToList();

        return SuccessResponse(activeBanners, "Faol banner'lar muvaffaqiyatli olindi");
    }

    /// <summary>
    /// ID bo'yicha banner olish
    /// </summary>
    /// <param name="id">Banner ID</param>
    /// <returns>Banner ma'lumotlari</returns>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBannerById(int id)
    {
        var result = await _contentService.GetContentByIdAsync(id);

        if (!result.IsSuccess)
            return HandleResult(result);

        // Tekshirish: Bu haqiqatan banner'mi?
        if (result.Data?.Type != "Banner")
            return ErrorResponse("Bu kontent banner emas");

        // Ko'rilish sonini oshirish
        await _contentService.IncrementViewCountAsync(id);

        return SuccessResponse(result.Data, "Banner muvaffaqiyatli olindi");
    }

    /// <summary>
    /// Yangi banner yaratish
    /// </summary>
    /// <param name="request">Banner ma'lumotlari</param>
    /// <returns>Yaratilgan banner</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateBanner([FromBody] SaveContentDto request)
    {
        // Banner turini majburiy o'rnatish
        request.Type = ContentType.Banner;

        var createdBy = GetCurrentUserId();
        var result = await _contentService.CreateContentAsync(request, createdBy);

        if (!result.IsSuccess)
            return HandleResult(result);

        return SuccessResponse(result.Data, "Banner muvaffaqiyatli yaratildi");
    }

    /// <summary>
    /// Banner'ni yangilash
    /// </summary>
    /// <param name="id">Banner ID</param>
    /// <param name="request">Yangi ma'lumotlar</param>
    /// <returns>Yangilangan banner</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateBanner(int id, [FromBody] SaveContentDto request)
    {
        // Banner turini majburiy o'rnatish
        request.Type = ContentType.Banner;

        var updatedBy = GetCurrentUserId();
        var result = await _contentService.UpdateContentAsync(id, request, updatedBy);

        if (!result.IsSuccess)
            return HandleResult(result);

        return SuccessResponse(result.Data, "Banner muvaffaqiyatli yangilandi");
    }

    /// <summary>
    /// Banner'ni o'chirish (soft delete)
    /// </summary>
    /// <param name="id">Banner ID</param>
    /// <returns>O'chirish natijasi</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteBanner(int id)
    {
        var deletedBy = GetCurrentUserId();
        var result = await _contentService.DeleteContentAsync(id, deletedBy);

        if (!result.IsSuccess)
            return ErrorResponse(result.Message);

        return SuccessResponse("Banner muvaffaqiyatli o'chirildi");
    }

    /// <summary>
    /// Banner'ni nashr qilish
    /// </summary>
    /// <param name="id">Banner ID</param>
    /// <returns>Nashr qilish natijasi</returns>
    [HttpPost("{id}/publish")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PublishBanner(int id)
    {
        var publishedBy = GetCurrentUserId();
        var result = await _contentService.PublishContentAsync(id, publishedBy);

        if (!result.IsSuccess)
            return ErrorResponse(result.Message);

        return SuccessResponse("Banner muvaffaqiyatli nashr qilindi");
    }

    /// <summary>
    /// Banner'ni nashrdan olib tashlash
    /// </summary>
    /// <param name="id">Banner ID</param>
    /// <returns>Nashrdan olib tashlash natijasi</returns>
    [HttpPost("{id}/unpublish")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UnpublishBanner(int id)
    {
        var unpublishedBy = GetCurrentUserId();
        var result = await _contentService.UnpublishContentAsync(id, unpublishedBy);

        if (!result.IsSuccess)
            return ErrorResponse(result.Message);

        return SuccessResponse("Banner muvaffaqiyatli nashrdan olindi");
    }

    /// <summary>
    /// Banner bosilganda (click) chaqiriladi
    /// </summary>
    /// <param name="id">Banner ID</param>
    /// <returns>Click qo'shilgan natija</returns>
    [HttpPost("{id}/click")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterBannerClick(int id)
    {
        try
        {
            // Click sonini oshirish
            await _contentService.IncrementClickCountAsync(id);

            return SuccessResponse("Banner click muvaffaqiyatli qayd qilindi");
        }
        catch (Exception ex)
        {
            return ErrorResponse($"Click qayd qilishda xatolik: {ex.Message}");
        }
    }

    /// <summary>
    /// Banner'lar tartibini yangilash
    /// </summary>
    /// <param name="id">Banner ID</param>
    /// <param name="newOrder">Yangi tartib raqami</param>
    /// <returns>Yangilash natijasi</returns>
    [HttpPatch("{id}/order")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateBannerOrder(int id, [FromQuery] int newOrder)
    {
        try
        {
            var banner = await _contentService.GetContentByIdAsync(id);

            if (!banner.IsSuccess)
                return HandleResult(banner);

            if (banner.Data?.Type != "Banner")
                return ErrorResponse("Bu kontent banner emas");

            var updateDto = new SaveContentDto
            {
                Type = ContentType.Banner,
                Title = banner.Data.Title,
                Description = banner.Data.Description,
                ImageUrl = banner.Data.ImageUrl,
                ExternalUrl = banner.Data.ExternalUrl,
                SortOrder = newOrder,
                IsPublished = banner.Data.IsPublished,
                TargetAudience = banner.Data.TargetAudience,
                StartDate = banner.Data.StartDate,
                EndDate = banner.Data.EndDate
            };

            var updatedBy = GetCurrentUserId();
            var result = await _contentService.UpdateContentAsync(id, updateDto, updatedBy);

            if (!result.IsSuccess)
                return ErrorResponse(result.Message);

            return SuccessResponse(result.Data, "Banner tartibi muvaffaqiyatli yangilandi");
        }
        catch (Exception ex)
        {
            return ErrorResponse($"Tartibni yangilashda xatolik: {ex.Message}");
        }
    }

    /// <summary>
    /// Test ma'lumotlar yaratish (faqat development)
    /// </summary>
    /// <param name="count">Nechta banner yaratish</param>
    /// <returns>Yaratilgan banner'lar</returns>
    [HttpPost("seed")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SeedBanners([FromQuery] int count = 5)
    {
        try
        {
            var createdBy = GetCurrentUserId();
            var banners = new List<ContentDetailDto>();

            for (int i = 1; i <= count; i++)
            {
                var bannerDto = new SaveContentDto
                {
                    Type = ContentType.Banner,
                    Title = $"Test Banner {i}",
                    Description = $"Bu {i}-chi test banner",
                    ImageUrl = $"https://via.placeholder.com/800x400?text=Banner+{i}",
                    ExternalUrl = i % 2 == 0 ? "https://example.com" : null,
                    IsPublished = i <= 3, // Birinchi 3 ta nashr qilingan
                    SortOrder = i,
                    TargetAudience = i % 3 == 0 ? "Customers" : "All",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(30)
                };

                var result = await _contentService.CreateContentAsync(bannerDto, createdBy);
                if (result.IsSuccess && result.Data != null)
                {
                    banners.Add(result.Data);
                }
            }

            return SuccessResponse(banners, $"{count} ta test banner muvaffaqiyatli yaratildi");
        }
        catch (Exception ex)
        {
            return ErrorResponse($"Test banner'lar yaratishda xatolik: {ex.Message}");
        }
    }
}
