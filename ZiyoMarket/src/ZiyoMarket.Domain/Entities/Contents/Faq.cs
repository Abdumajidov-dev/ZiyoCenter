using ZiyoMarket.Domain.Common;

namespace ZiyoMarket.Domain.Entities.Content;

/// <summary>
/// Ko'p so'raladigan savol (FAQ)
/// </summary>
public class Faq : BaseAuditableEntity
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public int SortOrder { get; set; }

    /// <summary>
    /// Mijozlarga ko'rinadimi
    /// </summary>
    public bool IsActive { get; set; } = true;
}
