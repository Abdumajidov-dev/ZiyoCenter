using ZiyoMarket.Domain.Common;

namespace ZiyoMarket.Domain.Entities.Products;

/// <summary>
/// Mahsulot va Kategoriya o'rtasidagi Many-to-Many bog'liqlik jadvali
/// </summary>
public class ProductCategory : BaseEntity
{
    /// <summary>
    /// Mahsulot ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Kategoriya ID
    /// </summary>
    public int CategoryId { get; set; }

    // Navigation Properties
    public virtual Product Product { get; set; } = null!;
    public virtual Category Category { get; set; } = null!;
}
