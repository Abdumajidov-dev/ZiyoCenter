using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ZiyoMarket.Data.UnitOfWorks;
using ZiyoMarket.Domain.Entities.Content;
using ZiyoMarket.Service.DTOs.Content;
using ZiyoMarket.Service.Interfaces;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Services;

public class ContentService : IContentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ContentService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ContentDetailDto>> GetContentByIdAsync(int contentId)
    {
        try
   {
     var content = await _unitOfWork.Contents.GetByIdAsync(contentId);
        if (content == null || content.DeletedAt != null)
     return Result<ContentDetailDto>.NotFound("Content not found");

        var dto = _mapper.Map<ContentDetailDto>(content);
     return Result<ContentDetailDto>.Success(dto);
        }
        catch (Exception ex)
   {
      return Result<ContentDetailDto>.InternalError($"Error: {ex.Message}");
  }
    }

    public async Task<Result<Results.PaginationResponse<ContentListDto>>> GetAllContentAsync(ContentFilterRequest request)
    {
        try
        {
  var query = _unitOfWork.Contents.Table.Where(c => c.DeletedAt == null);

     if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
   var term = request.SearchTerm.ToLower();
            query = query.Where(c =>
            c.Title.ToLower().Contains(term) ||
    c.Description.ToLower().Contains(term) ||
         (c.Tags != null && c.Tags.ToLower().Contains(term)));
}

   if (!string.IsNullOrEmpty(request.Type))
          query = query.Where(c => c.ContentType.ToString() == request.Type);

   if (!string.IsNullOrEmpty(request.Author))
     query = query.Where(c => c.Author == request.Author);

        if (request.IsActive)
    query = query.Where(c => c.IsActive == request.IsActive);

            query = query.OrderByDescending(c => c.CreatedAt);

         var total = await query.CountAsync();
     var contents = await query
 .Skip(request.Skip)
 .Take(request.PageSize)
           .ToListAsync();

var dtos = _mapper.Map<List<ContentListDto>>(contents);

            return Result<Results.PaginationResponse<ContentListDto>>.Success(
            new Results.PaginationResponse<ContentListDto>(dtos, total, request.PageNumber, request.PageSize));
        }
catch (Exception ex)
{
          return Result<Results.PaginationResponse<ContentListDto>>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<ContentDetailDto>> CreateContentAsync(SaveContentDto request, int createdBy)
    {
        try
     {
      var content = _mapper.Map<Content>(request);
            content.CreatedBy = createdBy;

         await _unitOfWork.Contents.InsertAsync(content);
     await _unitOfWork.SaveChangesAsync();

    var dto = _mapper.Map<ContentDetailDto>(content);
            return Result<ContentDetailDto>.Success(dto, "Content created successfully", 201);
        }
    catch (Exception ex)
    {
  return Result<ContentDetailDto>.InternalError($"Error: {ex.Message}");
        }
  }

    public async Task<Result<ContentDetailDto>> UpdateContentAsync(int id, SaveContentDto request, int updatedBy)
    {
        try
        {
            var content = await _unitOfWork.Contents.GetByIdAsync(id);
      if (content == null || content.DeletedAt != null)
            return Result<ContentDetailDto>.NotFound("Content not found");

         _mapper.Map(request, content);
            content.UpdatedBy = updatedBy;
   content.MarkAsUpdated();

            await _unitOfWork.Contents.Update(content, id);
      await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<ContentDetailDto>(content);
        return Result<ContentDetailDto>.Success(dto, "Content updated successfully");
        }
  catch (Exception ex)
        {
         return Result<ContentDetailDto>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> DeleteContentAsync(int contentId, int deletedBy)
    {
 try
    {
            var content = await _unitOfWork.Contents.GetByIdAsync(contentId);
   if (content == null || content.DeletedAt != null)
     return Result.NotFound("Content not found");

   content.DeletedBy = deletedBy;
            content.Delete();

            await _unitOfWork.Contents.Update(content, contentId);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Content deleted successfully");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> PublishContentAsync(int contentId, int publishedBy)
    {
        try
        {
        var content = await _unitOfWork.Contents.GetByIdAsync(contentId);
      if (content == null || content.DeletedAt != null)
    return Result.NotFound("Content not found");

            content.IsActive = true;
            content.UpdatedBy = publishedBy;
            content.MarkAsUpdated();

      await _unitOfWork.Contents.Update(content, contentId);
            await _unitOfWork.SaveChangesAsync();

return Result.Success("Content published successfully");
        }
        catch (Exception ex)
    {
    return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> UnpublishContentAsync(int contentId, int unpublishedBy)
    {
        try
      {
            var content = await _unitOfWork.Contents.GetByIdAsync(contentId);
 if (content == null || content.DeletedAt != null)
      return Result.NotFound("Content not found");

 content.IsActive = false;
   content.UpdatedBy = unpublishedBy;
            content.MarkAsUpdated();

await _unitOfWork.Contents.Update(content, contentId);
  await _unitOfWork.SaveChangesAsync();

            return Result.Success("Content unpublished successfully");
   }
        catch (Exception ex)
    {
            return Result.InternalError($"Error: {ex.Message}");
   }
    }

    public async Task<Result<List<ContentListDto>>> GetPublishedContentAsync(string? type = null)
    {
        try
        {
            var query = _unitOfWork.Contents.Table.Where(c => c.DeletedAt == null && c.IsActive);

            if (!string.IsNullOrEmpty(type))
            query = query.Where(c => c.ContentType.ToString() == type);

 query = query.OrderByDescending(c => c.CreatedAt);

            var contents = await query.ToListAsync();
 var dtos = _mapper.Map<List<ContentListDto>>(contents);

     return Result<List<ContentListDto>>.Success(dtos);
   }
      catch (Exception ex)
        {
        return Result<List<ContentListDto>>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ContentListDto>>> GetScheduledContentAsync()
    {
        try
     {
            var now = DateTime.UtcNow;
      var contents = await _unitOfWork.Contents.Table
          .Where(c => c.DeletedAt == null &&
                  c.ValidFrom > now)
  .OrderBy(c => c.ValidFrom)
          .ToListAsync();

            var dtos = _mapper.Map<List<ContentListDto>>(contents);
          return Result<List<ContentListDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<ContentListDto>>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> UpdateContentOrderAsync(List<UpdateContentOrderDto> updates, int updatedBy)
    {
        try
      {
    foreach (var update in updates)
      {
     var content = await _unitOfWork.Contents.GetByIdAsync(update.ContentId);
      if (content == null || content.DeletedAt != null)
         continue;

         content.DisplayOrder = update.DisplayOrder;
         content.UpdatedBy = updatedBy;
     content.MarkAsUpdated();

         await _unitOfWork.Contents.Update(content, update.ContentId);
  }

       await _unitOfWork.SaveChangesAsync();
      return Result.Success("Content order updated successfully");
        }
        catch (Exception ex)
        {
  return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ContentListDto>>> GetContentByTypeAsync(string type)
    {
     try
        {
        var contents = await _unitOfWork.Contents.Table
    .Where(c => c.DeletedAt == null &&
     c.ContentType.ToString() == type)
         .OrderByDescending(c => c.CreatedAt)
    .ToListAsync();

     var dtos = _mapper.Map<List<ContentListDto>>(contents);
 return Result<List<ContentListDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
    return Result<List<ContentListDto>>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<BannerDto>>> GetActiveBannersAsync()
    {
        try
      {
    var now = DateTime.UtcNow;
  var banners = await _unitOfWork.Contents.Table
   .Where(c => c.DeletedAt == null &&
             c.IsActive &&
      c.ContentType == Domain.Enums.ContentType.Banner &&
             c.ValidFrom <= now &&
        (!c.ValidUntil.HasValue || c.ValidUntil > now))
             .OrderBy(c => c.DisplayOrder)
   .ToListAsync();

  var dtos = _mapper.Map<List<BannerDto>>(banners);
            return Result<List<BannerDto>>.Success(dtos);
        }
 catch (Exception ex)
        {
 return Result<List<BannerDto>>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<VideoDto>>> GetLatestVideosAsync(int count = 10)
 {
      try
        {
  var videos = await _unitOfWork.Contents.Table
            .Where(c => c.DeletedAt == null &&
       c.IsActive &&
     c.ContentType == Domain.Enums.ContentType.Video)
         .OrderByDescending(c => c.CreatedAt)
        .Take(count)
         .ToListAsync();

     var dtos = _mapper.Map<List<VideoDto>>(videos);
      return Result<List<VideoDto>>.Success(dtos);
   }
        catch (Exception ex)
    {
     return Result<List<VideoDto>>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ArticleDto>>> GetLatestArticlesAsync(int count = 10)
    {
    try
        {
            var articles = await _unitOfWork.Contents.Table
                .Where(c => c.DeletedAt == null &&
       c.IsActive &&
    c.ContentType == Domain.Enums.ContentType.Article)
      .OrderByDescending(c => c.CreatedAt)
             .Take(count)
      .ToListAsync();

            var dtos = _mapper.Map<List<ArticleDto>>(articles);
      return Result<List<ArticleDto>>.Success(dtos);
     }
        catch (Exception ex)
      {
return Result<List<ArticleDto>>.InternalError($"Error: {ex.Message}");
        }
    }

 public async Task<Result<List<AnnouncementDto>>> GetActiveAnnouncementsAsync()
    {
        try
        {
            var now = DateTime.UtcNow;
          var announcements = await _unitOfWork.Contents.Table
     .Where(c => c.DeletedAt == null &&
     c.IsActive &&
             c.ContentType == Domain.Enums.ContentType.Announcement &&
    c.ValidFrom <= now &&
   (!c.ValidUntil.HasValue || c.ValidUntil > now))
     .OrderByDescending(c => c.CreatedAt)
      .ToListAsync();

     var dtos = _mapper.Map<List<AnnouncementDto>>(announcements);
         return Result<List<AnnouncementDto>>.Success(dtos);
      }
        catch (Exception ex)
        {
            return Result<List<AnnouncementDto>>.InternalError($"Error: {ex.Message}");
        }
    }

 public async Task<Result<ContentStatsDto>> GetContentStatisticsAsync()
    {
        try
{
            var stats = new ContentStatsDto
 {
          TotalContent = await _unitOfWork.Contents.CountAsync(c => c.DeletedAt == null),
                ActiveContent = await _unitOfWork.Contents.CountAsync(c => c.DeletedAt == null && c.IsActive),
    TotalViews = await _unitOfWork.Contents.Table.Where(c => c.DeletedAt == null).SumAsync(c => c.ViewCount),
    TotalClicks = await _unitOfWork.Contents.Table.Where(c => c.DeletedAt == null).SumAsync(c => c.ClickCount),
        ContentByType = await _unitOfWork.Contents.Table
 .Where(c => c.DeletedAt == null)
             .GroupBy(c => c.ContentType)
        .Select(g => new ContentTypeStatsDto
             {
    Type = g.Key.ToString(),
Count = g.Count(),
    Views = g.Sum(c => c.ViewCount),
          Clicks = g.Sum(c => c.ClickCount)
    })
       .ToListAsync()
        };

          return Result<ContentStatsDto>.Success(stats);
      }
        catch (Exception ex)
   {
   return Result<ContentStatsDto>.InternalError($"Error: {ex.Message}");
        }
 }

    public async Task<Result<List<TopContentDto>>> GetTopContentAsync(
        DateTime? startDate = null, DateTime? endDate = null, int count = 10)
    {
        try
        {
var query = _unitOfWork.Contents.Table.Where(c => c.DeletedAt == null);

          if (startDate.HasValue)
      query = query.Where(c => DateTime.Parse(c.CreatedAt) >= startDate.Value);

 if (endDate.HasValue)
                query = query.Where(c => DateTime.Parse(c.CreatedAt) <= endDate.Value);

       var topContent = await query
          .OrderByDescending(c => c.ViewCount)
                .Take(count)
                .Select(c => new TopContentDto
        {
       ContentId = c.Id,
    Title = c.Title,
      Type = c.ContentType.ToString(),
         ViewCount = c.ViewCount,
                    ClickCount = c.ClickCount,
           CreatedAt = DateTime.Parse(c.CreatedAt)
         })
   .ToListAsync();

            return Result<List<TopContentDto>>.Success(topContent);
        }
        catch (Exception ex)
      {
            return Result<List<TopContentDto>>.InternalError($"Error: {ex.Message}");
     }
    }

    public async Task<Result<ContentPerformanceDto>> GetContentPerformanceAsync(int contentId)
    {
        try
        {
    var content = await _unitOfWork.Contents.GetByIdAsync(contentId);
      if (content == null || content.DeletedAt != null)
         return Result<ContentPerformanceDto>.NotFound("Content not found");

  var performance = new ContentPerformanceDto
  {
     ContentId = contentId,
                Title = content.Title,
     Type = content.ContentType.ToString(),
         ViewCount = content.ViewCount,
        ClickCount = content.ClickCount,
          ClickThroughRate = content.ViewCount > 0 
       ? (double)content.ClickCount / content.ViewCount * 100 
      : 0
            };

      return Result<ContentPerformanceDto>.Success(performance);
      }
        catch (Exception ex)
        {
      return Result<ContentPerformanceDto>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> IncrementViewCountAsync(int contentId)
    {
        try
        {
         var content = await _unitOfWork.Contents.GetByIdAsync(contentId);
    if (content == null || content.DeletedAt != null)
  return Result.NotFound("Content not found");

    content.ViewCount++;
      content.MarkAsUpdated();

 await _unitOfWork.Contents.Update(content, contentId);
          await _unitOfWork.SaveChangesAsync();

     return Result.Success("View count incremented");
        }
        catch (Exception ex)
        {
     return Result.InternalError($"Error: {ex.Message}");
    }
    }

    public async Task<Result> IncrementClickCountAsync(int contentId)
    {
  try
        {
      var content = await _unitOfWork.Contents.GetByIdAsync(contentId);
   if (content == null || content.DeletedAt != null)
   return Result.NotFound("Content not found");

    content.ClickCount++;
      content.MarkAsUpdated();

 await _unitOfWork.Contents.Update(content, contentId);
         await _unitOfWork.SaveChangesAsync();

            return Result.Success("Click count incremented");
        }
        catch (Exception ex)
        {
    return Result.InternalError($"Error: {ex.Message}");
   }
    }

    public async Task<Result<List<ContentViewStatsDto>>> GetViewStatisticsAsync(
        DateTime startDate, DateTime endDate)
    {
        try
        {
   var contents = await _unitOfWork.Contents.Table
       .Where(c => c.DeletedAt == null &&
       DateTime.Parse(c.CreatedAt) >= startDate &&
            DateTime.Parse(c.CreatedAt) <= endDate)
                .Select(c => new ContentViewStatsDto
     {
    ContentId = c.Id,
          Title = c.Title,
        Type = c.ContentType.ToString(),
         ViewCount = c.ViewCount,
   ClickCount = c.ClickCount,
       CreatedAt = DateTime.Parse(c.CreatedAt)
    })
      .OrderByDescending(c => c.ViewCount)
.ToListAsync();

         return Result<List<ContentViewStatsDto>>.Success(contents);
        }
        catch (Exception ex)
        {
          return Result<List<ContentViewStatsDto>>.InternalError($"Error: {ex.Message}");
     }
    }

    public async Task<Result<List<ContentListDto>>> GetExpiredContentAsync()
    {
        try
   {
    var now = DateTime.UtcNow;
   var contents = await _unitOfWork.Contents.Table
            .Where(c => c.DeletedAt == null &&
  c.ValidUntil.HasValue &&
      c.ValidUntil <= now)
      .OrderByDescending(c => c.ValidUntil)
.ToListAsync();

            var dtos = _mapper.Map<List<ContentListDto>>(contents);
 return Result<List<ContentListDto>>.Success(dtos);
   }
        catch (Exception ex)
        {
            return Result<List<ContentListDto>>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ContentListDto>>> GetDraftContentAsync()
    {
        try
        {
   var contents = await _unitOfWork.Contents.Table
       .Where(c => c.DeletedAt == null && !c.IsActive)
                .OrderByDescending(c => c.CreatedAt)
    .ToListAsync();

        var dtos = _mapper.Map<List<ContentListDto>>(contents);
         return Result<List<ContentListDto>>.Success(dtos);
        }
  catch (Exception ex)
{
        return Result<List<ContentListDto>>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> ArchiveOldContentAsync(DateTime olderThan, int archivedBy)
    {
        try
    {
   var contents = await _unitOfWork.Contents.Table
     .Where(c => c.DeletedAt == null &&
       DateTime.Parse(c.CreatedAt) < olderThan)
     .ToListAsync();

  foreach (var content in contents)
            {
                content.IsActive = false;
         content.UpdatedBy = archivedBy;
           content.MarkAsUpdated();
        await _unitOfWork.Contents.Update(content, content.Id);
            }

            await _unitOfWork.SaveChangesAsync();
            return Result.Success($"{contents.Count} contents archived");
        }
    catch (Exception ex)
   {
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ContentListDto>>> SearchContentAsync(string searchTerm)
    {
      try
        {
            var term = searchTerm.ToLower();
     var contents = await _unitOfWork.Contents.Table
     .Where(c => c.DeletedAt == null &&
       (c.Title.ToLower().Contains(term) ||
      c.Description.ToLower().Contains(term) ||
       (c.Tags != null && c.Tags.ToLower().Contains(term))))
                .OrderByDescending(c => c.CreatedAt)
    .Take(20)
             .ToListAsync();

         var dtos = _mapper.Map<List<ContentListDto>>(contents);
     return Result<List<ContentListDto>>.Success(dtos);
        }
        catch (Exception ex)
  {
            return Result<List<ContentListDto>>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ContentListDto>>> GetContentByTagAsync(string tag)
    {
        try
      {
  var contents = await _unitOfWork.Contents.Table
        .Where(c => c.DeletedAt == null &&
      c.Tags != null &&
  c.Tags.ToLower().Contains(tag.ToLower()))
   .OrderByDescending(c => c.CreatedAt)
       .ToListAsync();

      var dtos = _mapper.Map<List<ContentListDto>>(contents);
   return Result<List<ContentListDto>>.Success(dtos);
        }
  catch (Exception ex)
      {
  return Result<List<ContentListDto>>.InternalError($"Error: {ex.Message}");
        }
}

    public async Task<Result<List<string>>> GetAllContentTagsAsync()
    {
        try
        {
   var tags = await _unitOfWork.Contents.Table
          .Where(c => c.DeletedAt == null && !string.IsNullOrEmpty(c.Tags))
     .Select(c => c.Tags)
       .ToListAsync();

  var allTags = tags
          .SelectMany(t => t.Split(','))
    .Select(t => t.Trim())
    .Distinct()
      .OrderBy(t => t)
           .ToList();

            return Result<List<string>>.Success(allTags);
        }
        catch (Exception ex)
        {
        return Result<List<string>>.InternalError($"Error: {ex.Message}");
    }
    }

    public async Task<Result<List<ContentListDto>>> GetContentByAuthorAsync(string author)
    {
      try
        {
     var contents = await _unitOfWork.Contents.Table
  .Where(c => c.DeletedAt == null &&
         c.Author == author)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

     var dtos = _mapper.Map<List<ContentListDto>>(contents);
            return Result<List<ContentListDto>>.Success(dtos);
      }
        catch (Exception ex)
      {
            return Result<List<ContentListDto>>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAllContentAsync(int deletedBy, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
       var query = _unitOfWork.Contents.Table.Where(c => c.DeletedAt == null);

         if (startDate.HasValue)
        query = query.Where(c => DateTime.Parse(c.CreatedAt) >= startDate.Value);

       if (endDate.HasValue)
     query = query.Where(c => DateTime.Parse(c.CreatedAt) <= endDate.Value);

       var contents = await query.ToListAsync();

         foreach (var content in contents)
  {
                content.DeletedBy = deletedBy;
content.Delete();
      await _unitOfWork.Contents.Update(content, content.Id);
            }

            await _unitOfWork.SaveChangesAsync();
    return Result.Success($"{contents.Count} contents deleted");
        }
        catch (Exception ex)
 {
          return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ContentDetailDto>>> SeedMockContentAsync(int createdBy, int count = 10)
    {
        try
        {
      var random = new Random();
    var contents = new List<Content>();

            var contentTypes = Enum.GetValues(typeof(Domain.Enums.ContentType)).Cast<Domain.Enums.ContentType>().ToList();
     var titles = new[] { "Yangilik", "E'lon", "Aksiya", "Video darslik", "Maqola" };
          var authors = new[] { "Admin", "Editor", "Moderator" };

            for (int i = 0; i < count; i++)
       {
        var contentType = contentTypes[random.Next(contentTypes.Count)];
        var title = $"{titles[random.Next(titles.Length)]} #{i + 1}";

          var content = new Content
         {
   Title = title,
    Description = $"{title} uchun tavsif",
       ContentType = contentType,
          Author = authors[random.Next(authors.Length)],
        IsActive = random.Next(2) == 0,
        ValidFrom = DateTime.UtcNow.AddDays(-random.Next(30)),
     ValidUntil = random.Next(2) == 0 ? DateTime.UtcNow.AddDays(random.Next(30)) : null,
     Tags = $"tag{random.Next(1, 6)}, tag{random.Next(1, 6)}",
     DisplayOrder = i,
         ViewCount = random.Next(100),
       ClickCount = random.Next(50),
       CreatedBy = createdBy
       };

     await _unitOfWork.Contents.InsertAsync(content);
       contents.Add(content);
     }

      await _unitOfWork.SaveChangesAsync();

            var dtos = _mapper.Map<List<ContentDetailDto>>(contents);
            return Result<List<ContentDetailDto>>.Success(dtos, $"{count} mock contents created");
        }
        catch (Exception ex)
   {
      return Result<List<ContentDetailDto>>.InternalError($"Error: {ex.Message}");
        }
    }

    public Task GetAllContentAsync()
    {
        throw new NotImplementedException();
    }
}