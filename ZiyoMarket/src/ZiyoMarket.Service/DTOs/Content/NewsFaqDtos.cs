using System.ComponentModel.DataAnnotations;

namespace ZiyoMarket.Service.DTOs.Content;

/// <summary>
/// Yangilik — Content (ContentType.Article) ustidagi soddalashtirilgan ko'rinish.
/// Shakli admin panelning NewsDto'siga mos.
/// </summary>
public class NewsDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public DateTime PublishDate { get; set; }

    /// <summary>
    /// Nashr qilinganmi
    /// </summary>
    public bool IsActive { get; set; }

    public int ViewsCount { get; set; }
}

public class SaveNewsDto
{
    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    /// <summary>
    /// Berilmasa — hozirgi vaqt
    /// </summary>
    public DateTime? PublishDate { get; set; }

    public bool IsActive { get; set; }
}

public class FaqDto
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public string Category { get; set; } = "General";
}

public class SaveFaqDto
{
    [Required, MaxLength(500)]
    public string Question { get; set; } = string.Empty;

    [Required]
    public string Answer { get; set; } = string.Empty;

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    [MaxLength(100)]
    public string? Category { get; set; }
}
