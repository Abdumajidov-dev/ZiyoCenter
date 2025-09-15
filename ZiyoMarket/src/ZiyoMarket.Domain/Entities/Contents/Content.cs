using ZiyoMarket.Domain.Common;
using ZiyoMarket.Domain.Enums;

namespace ZiyoMarket.Domain.Entities.Content;

/// <summary>
/// Kontent entity'si
/// </summary>
public class Content : BaseAuditableEntity
{
    /// <summary>
    /// Kontent turi
    /// </summary>
    public ContentType ContentType { get; set; }

    /// <summary>
    /// Sarlavha
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Tavsif/matn
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Kontent URL (video, rasm, fayl)
    /// </summary>
    public string? ContentUrl { get; set; }

    /// <summary>
    /// Qo'shimcha ma'lumotlar (JSON)
    /// </summary>
    public string? ContentData { get; set; }

    /// <summary>
    /// Maqsadli auditoriya
    /// </summary>
    public string TargetAudience { get; set; } = "All"; // All, Customers, Sellers, Admins

    /// <summary>
    /// Faolmi
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Ko'rsatish tartibi
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Amal qilish boshlanish sanasi
    /// </summary>
    public DateTime ValidFrom { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Amal qilish tugash sanasi
    /// </summary>
    public DateTime? ValidUntil { get; set; }

    /// <summary>
    /// Ko'rilgan soni
    /// </summary>
    public int ViewCount { get; set; } = 0;

    /// <summary>
    /// Bosilgan soni (banner, link uchun)
    /// </summary>
    public int ClickCount { get; set; } = 0;

    /// <summary>
    /// Tags (qidiruv uchun)
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// SEO sarlavha
    /// </summary>
    public string? SeoTitle { get; set; }

    /// <summary>
    /// SEO tavsif
    /// </summary>
    public string? SeoDescription { get; set; }

    /// <summary>
    /// SEO kalit so'zlar
    /// </summary>
    public string? SeoKeywords { get; set; }

    /// <summary>
    /// Til kodi
    /// </summary>
    public string Language { get; set; } = "uz"; // uz, ru, en

    /// <summary>
    /// Avtor/muallif
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// Thumbnail rasm URL
    /// </summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// Video davomiyligi (agar video bo'lsa, sekund)
    /// </summary>
    public int? VideoDuration { get; set; }

    /// <summary>
    /// Fayl o'lchami (bytes)
    /// </summary>
    public long? FileSize { get; set; }

    /// <summary>
    /// MIME type
    /// </summary>
    public string? MimeType { get; set; }

    // Business Methods

    /// <summary>
    /// Hozir amal qiladimi
    /// </summary>
    public bool IsValidNow => IsActive &&
                             ValidFrom <= DateTime.UtcNow &&
                             (!ValidUntil.HasValue || ValidUntil.Value >= DateTime.UtcNow);

    /// <summary>
    /// Banner kontentmi
    /// </summary>
    public bool IsBanner => ContentType == ContentType.Banner;

    /// <summary>
    /// Video kontentmi
    /// </summary>
    public bool IsVideo => ContentType == ContentType.Video;

    /// <summary>
    /// Maqola kontentmi
    /// </summary>
    public bool IsArticle => ContentType == ContentType.Article;

    /// <summary>
    /// E'lon kontentmi
    /// </summary>
    public bool IsAnnouncement => ContentType == ContentType.Announcement;

    /// <summary>
    /// Muddati tugaganmi
    /// </summary>
    public bool IsExpired => ValidUntil.HasValue && ValidUntil.Value < DateTime.UtcNow;

    /// <summary>
    /// Tez orada tugayaptimi (3 kun qolgan)
    /// </summary>
    public bool IsExpiringSoon => ValidUntil.HasValue &&
                                 ValidUntil.Value.Subtract(DateTime.UtcNow).TotalDays <= 3 &&
                                 ValidUntil.Value > DateTime.UtcNow;

    /// <summary>
    /// Barcha foydalanuvchilar uchunmi
    /// </summary>
    public bool IsForAllUsers => TargetAudience.Equals("All", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Mijozlar uchunmi
    /// </summary>
    public bool IsForCustomers => TargetAudience.Contains("Customer", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Sotuvchilar uchunmi
    /// </summary>
    public bool IsForSellers => TargetAudience.Contains("Seller", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Admin'lar uchunmi
    /// </summary>
    public bool IsForAdmins => TargetAudience.Contains("Admin", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Kontentni faollashtirish
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Kontentni faolsizlashtirish
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    /// <summary>
    /// Sarlavhani yangilash
    /// </summary>
    public void UpdateTitle(string newTitle)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
            throw new ArgumentException("Sarlavha bo'sh bo'la olmaydi");

        Title = newTitle.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Tavsifni yangilash
    /// </summary>
    public void UpdateDescription(string? description)
    {
        Description = description?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Kontent URL'ini yangilash
    /// </summary>
    public void UpdateContentUrl(string? contentUrl)
    {
        ContentUrl = contentUrl?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Amal qilish muddatini o'rnatish
    /// </summary>
    public void SetValidityPeriod(DateTime validFrom, DateTime? validUntil = null)
    {
        if (validUntil.HasValue && validUntil.Value <= validFrom)
            throw new ArgumentException("Tugash sanasi boshlanish sanasidan keyin bo'lishi kerak");

        ValidFrom = validFrom;
        ValidUntil = validUntil;
        MarkAsUpdated();
    }

    /// <summary>
    /// Maqsadli auditoriyani o'rnatish
    /// </summary>
    public void SetTargetAudience(string targetAudience)
    {
        var allowedAudiences = new[] { "All", "Customers", "Sellers", "Admins", "Customers,Sellers" };

        if (!allowedAudiences.Any(a => a.Equals(targetAudience, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException($"Noto'g'ri maqsadli auditoriya: {targetAudience}");

        TargetAudience = targetAudience;
        MarkAsUpdated();
    }

    /// <summary>
    /// Ko'rsatish tartibini o'rnatish
    /// </summary>
    public void SetDisplayOrder(int order)
    {
        DisplayOrder = order;
        MarkAsUpdated();
    }

    /// <summary>
    /// Ko'rilish sonini oshirish
    /// </summary>
    public void IncrementViewCount()
    {
        ViewCount++;
        MarkAsUpdated();
    }

    /// <summary>
    /// Bosish sonini oshirish
    /// </summary>
    public void IncrementClickCount()
    {
        ClickCount++;
        MarkAsUpdated();
    }

    /// <summary>
    /// Tag qo'shish
    /// </summary>
    public void AddTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return;

        tag = tag.Trim().ToLowerInvariant();

        var currentTags = GetTagList();
        if (!currentTags.Contains(tag))
        {
            currentTags.Add(tag);
            Tags = string.Join(",", currentTags);
            MarkAsUpdated();
        }
    }

    /// <summary>
    /// Tag olib tashlash
    /// </summary>
    public void RemoveTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return;

        tag = tag.Trim().ToLowerInvariant();

        var currentTags = GetTagList();
        if (currentTags.Remove(tag))
        {
            Tags = currentTags.Any() ? string.Join(",", currentTags) : null;
            MarkAsUpdated();
        }
    }

    /// <summary>
    /// Tag'lar ro'yxatini olish
    /// </summary>
    public List<string> GetTagList()
    {
        if (string.IsNullOrWhiteSpace(Tags))
            return new List<string>();

        return Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                  .Select(t => t.Trim().ToLowerInvariant())
                  .Where(t => !string.IsNullOrEmpty(t))
                  .ToList();
    }

    /// <summary>
    /// SEO ma'lumotlarini yangilash
    /// </summary>
    public void UpdateSeoInfo(string? seoTitle, string? seoDescription, string? seoKeywords)
    {
        SeoTitle = seoTitle?.Trim();
        SeoDescription = seoDescription?.Trim();
        SeoKeywords = seoKeywords?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Tilni o'rnatish
    /// </summary>
    public void SetLanguage(string language)
    {
        var allowedLanguages = new[] { "uz", "ru", "en" };

        if (!allowedLanguages.Contains(language.ToLowerInvariant()))
            throw new ArgumentException($"Noto'g'ri til kodi: {language}");

        Language = language.ToLowerInvariant();
        MarkAsUpdated();
    }

    /// <summary>
    /// Muallif ma'lumotini o'rnatish
    /// </summary>
    public void SetAuthor(string? author)
    {
        Author = author?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Thumbnail rasmini o'rnatish
    /// </summary>
    public void SetThumbnail(string? thumbnailUrl)
    {
        ThumbnailUrl = thumbnailUrl?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Video davomiyligini o'rnatish
    /// </summary>
    public void SetVideoDuration(int? durationInSeconds)
    {
        if (durationInSeconds.HasValue && durationInSeconds.Value < 0)
            throw new ArgumentException("Video davomiyligi manfiy bo'la olmaydi");

        VideoDuration = durationInSeconds;
        MarkAsUpdated();
    }

    /// <summary>
    /// Fayl ma'lumotlarini o'rnatish
    /// </summary>
    public void SetFileInfo(long? fileSize, string? mimeType)
    {
        if (fileSize.HasValue && fileSize.Value < 0)
            throw new ArgumentException("Fayl o'lchami manfiy bo'la olmaydi");

        FileSize = fileSize;
        MimeType = mimeType?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Foydalanuvchi uchun ko'rish ruxsati bormi
    /// </summary>
    public bool CanBeViewedBy(UserType userType)
    {
        if (!IsValidNow)
            return false;

        if (IsForAllUsers)
            return true;

        return userType switch
        {
            UserType.Customer => IsForCustomers,
            UserType.Seller => IsForSellers,
            UserType.Admin => IsForAdmins,
            _ => false
        };
    }

    /// <summary>
    /// CTR (Click-Through Rate) hisoblash
    /// </summary>
    public decimal GetClickThroughRate()
    {
        if (ViewCount == 0)
            return 0;

        return (decimal)ClickCount / ViewCount * 100;
    }

    /// <summary>
    /// Fayl o'lchamini human-readable formatda
    /// </summary>
    public string GetFileSizeFormatted()
    {
        if (!FileSize.HasValue)
            return "";

        var size = FileSize.Value;

        if (size < 1024)
            return $"{size} B";

        if (size < 1024 * 1024)
            return $"{size / 1024:F1} KB";

        if (size < 1024 * 1024 * 1024)
            return $"{size / (1024 * 1024):F1} MB";

        return $"{size / (1024 * 1024 * 1024):F1} GB";
    }

    /// <summary>
    /// Video davomiyligini human-readable formatda
    /// </summary>
    public string GetVideoDurationFormatted()
    {
        if (!VideoDuration.HasValue)
            return "";

        var totalSeconds = VideoDuration.Value;
        var hours = totalSeconds / 3600;
        var minutes = (totalSeconds % 3600) / 60;
        var seconds = totalSeconds % 60;

        if (hours > 0)
            return $"{hours:D2}:{minutes:D2}:{seconds:D2}";

        return $"{minutes:D2}:{seconds:D2}";
    }

    /// <summary>
    /// Kontent validatsiya
    /// </summary>
    public Result Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Title))
            errors.Add("Sarlavha bo'sh bo'la olmaydi");

        if (ValidUntil.HasValue && ValidUntil.Value <= ValidFrom)
            errors.Add("Tugash sanasi boshlanish sanasidan keyin bo'lishi kerak");

        var allowedAudiences = new[] { "All", "Customers", "Sellers", "Admins", "Customers,Sellers" };
        if (!allowedAudiences.Any(a => a.Equals(TargetAudience, StringComparison.OrdinalIgnoreCase)))
            errors.Add("Noto'g'ri maqsadli auditoriya");

        var allowedLanguages = new[] { "uz", "ru", "en" };
        if (!allowedLanguages.Contains(Language.ToLowerInvariant()))
            errors.Add("Noto'g'ri til kodi");

        if (VideoDuration.HasValue && VideoDuration.Value < 0)
            errors.Add("Video davomiyligi manfiy bo'la olmaydi");

        if (FileSize.HasValue && FileSize.Value < 0)
            errors.Add("Fayl o'lchami manfiy bo'la olmaydi");

        return errors.Any() ? Result.Failure(errors) : Result.Success();
    }

    /// <summary>
    /// Kontent ma'lumotlarini qisqa formatda
    /// </summary>
    public string GetShortDescription()
    {
        var desc = $"{ContentType}: {Title}";

        if (!IsActive)
            desc += " [NOFAOL]";
        else if (IsExpired)
            desc += " [MUDDATI TUGAGAN]";
        else if (IsExpiringSoon)
            desc += " [TEZ TUGAYDI]";

        return desc;
    }

    /// <summary>
    /// Kontent ma'lumotlarini to'liq formatda
    /// </summary>
    public override string ToString()
    {
        return $"{ContentType} - {Title} ({TargetAudience})";
    }
}