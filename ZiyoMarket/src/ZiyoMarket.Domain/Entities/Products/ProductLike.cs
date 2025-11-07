using ZiyoMarket.Domain.Common;
using ZiyoMarket.Domain.Entities.Users;

namespace ZiyoMarket.Domain.Entities.Products;

/// <summary>
/// Mahsulot yoqtirish entity'si
/// </summary>
public class ProductLike : BaseEntity
{
    /// <summary>
    /// Mijoz ID
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Mahsulot ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Yoqtirilgan sana
    /// </summary>
    public DateTime LikedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties

    /// <summary>
    /// Mijoz
    /// </summary>
    public virtual Customer Customer { get; set; } = null!;

    /// <summary>
    /// Mahsulot
    /// </summary>
    public virtual Product Product { get; set; } = null!;
    public bool IsDeleted { get; set; } = false;
    // Business Methods

    /// <summary>
    /// Yoqtirishni bekor qilish
    /// </summary>
    public void Unlike()
    {
        Delete();
    }

    /// <summary>
    /// Yoqtirish vaqtini yangilash
    /// </summary>
    public void RefreshLike()
    {
        LikedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    /// <summary>
    /// Mahsulot hali ham faolmi
    /// </summary>
    public bool IsProductActive()
    {
        return Product != null && Product.IsActive && !Product.IsDeleted;
    }

    /// <summary>
    /// Mijoz hali ham faolmi
    /// </summary>
    public bool IsCustomerActive()
    {
        return Customer != null && Customer.IsActive && !Customer.IsDeleted;
    }

    /// <summary>
    /// Like yaroqli holatdami
    /// </summary>
    public bool IsValid()
    {
        return IsProductActive() && IsCustomerActive() && !IsDeleted;
    }
}