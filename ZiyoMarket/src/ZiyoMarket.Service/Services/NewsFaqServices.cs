using Microsoft.EntityFrameworkCore;
using ZiyoMarket.Data.UnitOfWorks;
using ZiyoMarket.Domain.Entities.Content;
using ZiyoMarket.Domain.Enums;
using ZiyoMarket.Service.DTOs.Content;
using ZiyoMarket.Service.Interfaces;
using ZiyoMarket.Service.Results;
using ContentEntity = ZiyoMarket.Domain.Entities.Content.Content;

namespace ZiyoMarket.Service.Services;

/// <summary>
/// Yangiliklar alohida jadval emas — Content (ContentType.Article): Description = matn,
/// ContentUrl = rasm, IsActive = nashr qilingan, ValidFrom = nashr sanasi.
/// </summary>
public class NewsService : INewsService
{
    private readonly IUnitOfWork _unitOfWork;

    public NewsService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<NewsDto>>> GetAllAsync(bool includeUnpublished)
    {
        try
        {
            var news = await _unitOfWork.Contents
                .SelectAll(c => c.ContentType == ContentType.Article && c.DeletedAt == null &&
                                (includeUnpublished || c.IsActive))
                .OrderByDescending(c => c.ValidFrom)
                .ToListAsync();

            return Result<List<NewsDto>>.Success(news.Select(ToDto).ToList());
        }
        catch (Exception ex)
        {
            return Result<List<NewsDto>>.InternalError(ex.Message);
        }
    }

    public async Task<Result<NewsDto>> GetByIdAsync(int id, bool includeUnpublished)
    {
        var news = await FindAsync(id);
        if (news == null || (!includeUnpublished && !news.IsActive))
            return Result<NewsDto>.NotFound("Yangilik topilmadi");

        return Result<NewsDto>.Success(ToDto(news));
    }

    public async Task<Result<NewsDto>> CreateAsync(SaveNewsDto dto, int createdBy)
    {
        try
        {
            var news = new ContentEntity { ContentType = ContentType.Article, CreatedBy = createdBy };
            Apply(news, dto);

            await _unitOfWork.Contents.InsertAsync(news);
            await _unitOfWork.SaveChangesAsync();

            return Result<NewsDto>.Success(ToDto(news), "Yangilik yaratildi", 201);
        }
        catch (Exception ex)
        {
            return Result<NewsDto>.InternalError(ex.Message);
        }
    }

    public async Task<Result<NewsDto>> UpdateAsync(int id, SaveNewsDto dto, int updatedBy)
    {
        try
        {
            var news = await FindAsync(id);
            if (news == null)
                return Result<NewsDto>.NotFound("Yangilik topilmadi");

            Apply(news, dto);
            news.UpdatedBy = updatedBy;
            news.MarkAsUpdated();

            await _unitOfWork.Contents.Update(news, id);
            await _unitOfWork.SaveChangesAsync();

            return Result<NewsDto>.Success(ToDto(news), "Yangilik yangilandi");
        }
        catch (Exception ex)
        {
            return Result<NewsDto>.InternalError(ex.Message);
        }
    }

    public async Task<Result> DeleteAsync(int id, int deletedBy)
    {
        var news = await FindAsync(id);
        if (news == null)
            return Result.NotFound("Yangilik topilmadi");

        news.DeletedBy = deletedBy;
        _unitOfWork.Contents.SoftDelete(news);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success("Yangilik o'chirildi");
    }

    private Task<ContentEntity?> FindAsync(int id)
        => _unitOfWork.Contents.SelectAsync(c => c.Id == id && c.ContentType == ContentType.Article && c.DeletedAt == null);

    private static void Apply(ContentEntity news, SaveNewsDto dto)
    {
        news.Title = dto.Title;
        news.Description = dto.Content;
        news.ContentUrl = dto.ImageUrl;
        news.IsActive = dto.IsActive;
        // Admin panel yangi yangilikda DateTime.MinValue yuboradi — bu "sana berilmagan" degani
        if (dto.PublishDate is { Year: > 2000 } publishDate)
            news.ValidFrom = publishDate;
    }

    private static NewsDto ToDto(ContentEntity c) => new()
    {
        Id = c.Id,
        Title = c.Title,
        Content = c.Description ?? string.Empty,
        ImageUrl = c.ContentUrl,
        PublishDate = c.ValidFrom,
        IsActive = c.IsActive,
        ViewsCount = c.ViewCount
    };
}

public class FaqService : IFaqService
{
    private readonly IUnitOfWork _unitOfWork;

    public FaqService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<FaqDto>>> GetAllAsync(bool includeInactive)
    {
        try
        {
            var faqs = await _unitOfWork.Faqs
                .SelectAll(f => f.DeletedAt == null && (includeInactive || f.IsActive))
                .OrderBy(f => f.Category).ThenBy(f => f.SortOrder).ThenBy(f => f.Id)
                .ToListAsync();

            return Result<List<FaqDto>>.Success(faqs.Select(ToDto).ToList());
        }
        catch (Exception ex)
        {
            return Result<List<FaqDto>>.InternalError(ex.Message);
        }
    }

    public async Task<Result<FaqDto>> GetByIdAsync(int id)
    {
        var faq = await FindAsync(id);
        return faq == null
            ? Result<FaqDto>.NotFound("FAQ topilmadi")
            : Result<FaqDto>.Success(ToDto(faq));
    }

    public async Task<Result<FaqDto>> CreateAsync(SaveFaqDto dto, int createdBy)
    {
        try
        {
            var faq = new Faq { CreatedBy = createdBy };
            Apply(faq, dto);

            await _unitOfWork.Faqs.InsertAsync(faq);
            await _unitOfWork.SaveChangesAsync();

            return Result<FaqDto>.Success(ToDto(faq), "FAQ yaratildi", 201);
        }
        catch (Exception ex)
        {
            return Result<FaqDto>.InternalError(ex.Message);
        }
    }

    public async Task<Result<FaqDto>> UpdateAsync(int id, SaveFaqDto dto, int updatedBy)
    {
        try
        {
            var faq = await FindAsync(id);
            if (faq == null)
                return Result<FaqDto>.NotFound("FAQ topilmadi");

            Apply(faq, dto);
            faq.UpdatedBy = updatedBy;
            faq.MarkAsUpdated();

            await _unitOfWork.Faqs.Update(faq, id);
            await _unitOfWork.SaveChangesAsync();

            return Result<FaqDto>.Success(ToDto(faq), "FAQ yangilandi");
        }
        catch (Exception ex)
        {
            return Result<FaqDto>.InternalError(ex.Message);
        }
    }

    public async Task<Result> DeleteAsync(int id, int deletedBy)
    {
        var faq = await FindAsync(id);
        if (faq == null)
            return Result.NotFound("FAQ topilmadi");

        faq.DeletedBy = deletedBy;
        _unitOfWork.Faqs.SoftDelete(faq);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success("FAQ o'chirildi");
    }

    private Task<Faq?> FindAsync(int id)
        => _unitOfWork.Faqs.SelectAsync(f => f.Id == id && f.DeletedAt == null);

    private static void Apply(Faq faq, SaveFaqDto dto)
    {
        faq.Question = dto.Question;
        faq.Answer = dto.Answer;
        faq.SortOrder = dto.SortOrder;
        faq.IsActive = dto.IsActive;
        faq.Category = string.IsNullOrWhiteSpace(dto.Category) ? "General" : dto.Category;
    }

    private static FaqDto ToDto(Faq f) => new()
    {
        Id = f.Id,
        Question = f.Question,
        Answer = f.Answer,
        SortOrder = f.SortOrder,
        IsActive = f.IsActive,
        Category = f.Category
    };
}
