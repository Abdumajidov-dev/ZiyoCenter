using System.ComponentModel.DataAnnotations.Schema;
using ZiyoMarket.Domain.Common;

namespace ZiyoMarket.Domain.Entities.Products;

/// <summary>
/// Mahsulotning galereya rasmlari (ko'p rasm saqlash uchun)
/// </summary>
public class ProductImage : BaseAuditableEntity
{
    /// <summary>
    /// Qaysi mahsulotga tegishli ekanligi
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Rasmning internetdagi manzili
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Ushbu rasm asosiy (qopqoq/cover) rasmmi?
    /// </summary>
    public bool IsPrimary { get; set; } = false;

    /// <summary>
    /// Rasmlarning namoyish etilish tartibi
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    // Navigation property
    
    [ForeignKey("ProductId")]
    public virtual Product? Product { get; set; }
}
