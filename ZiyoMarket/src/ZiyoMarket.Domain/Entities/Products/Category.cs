using ZiyoMarket.Domain.Common;

namespace ZiyoMarket.Domain.Entities.Products;

/// <summary>
/// Kategoriya entity'si
/// </summary>
public class Category : BaseAuditableEntity
{
    /// <summary>
    /// Kategoriya nomi
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Kategoriya tavsifi
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Ota kategoriya ID (ichki kategoriya uchun)
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// Faolmi
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Kategoriya rasmi URL
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Tartiblash uchun
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    // Navigation Properties

    /// <summary>
    /// Ota kategoriya
    /// </summary>
    public virtual Category? Parent { get; set; }

    /// <summary>
    /// Ichki kategoriyalar
    /// </summary>
    public virtual ICollection<Category> Children { get; set; } = new List<Category>();

    /// <summary>
    /// Bu kategoriyaga tegishli mahsulotlar
    /// </summary>
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    // Business Methods

    /// <summary>
    /// Asosiy kategoriyami (ota yo'q)
    /// </summary>
    public bool IsRootCategory => ParentId == null;

    /// <summary>
    /// Ichki kategoriyami (ota bor)
    /// </summary>
    public bool IsSubCategory => ParentId.HasValue;

    /// <summary>
    /// Ichki kategoriyalari bormi
    /// </summary>
    public bool HasChildren => Children.Any();

    /// <summary>
    /// To'liq yo'l (Kitoblar > Diniy kitoblar)
    /// </summary>
    public string GetFullPath()
    {
        if (IsRootCategory)
            return Name;

        var path = new List<string> { Name };
        var current = Parent;

        while (current != null)
        {
            path.Insert(0, current.Name);
            current = current.Parent;
        }

        return string.Join(" > ", path);
    }

    /// <summary>
    /// Kategoriya faollashtirish
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Kategoriya faolsizlashtirish
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    /// <summary>
    /// Kategoriya nomini o'zgartirish
    /// </summary>
    public void ChangeName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Kategoriya nomi bo'sh bo'la olmaydi");

        Name = newName.Trim();
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
    /// Rasm URL'ini yangilash
    /// </summary>
    public void UpdateImageUrl(string? imageUrl)
    {
        ImageUrl = imageUrl?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Tartiblash raqamini o'zgartirish
    /// </summary>
    public void ChangeDisplayOrder(int order)
    {
        DisplayOrder = order;
        MarkAsUpdated();
    }

    /// <summary>
    /// Ota kategoriyani o'zgartirish
    /// </summary>
    public void ChangeParent(int? parentId)
    {
        // O'zini o'ziga ota qilib qo'yishni oldini olish
        if (parentId == Id)
            throw new InvalidOperationException("Kategoriya o'zini ota qilib qo'ya olmaydi");

        ParentId = parentId;
        MarkAsUpdated();
    }

    /// <summary>
    /// Bu kategoriyada faol mahsulotlar soni
    /// </summary>
    public int GetActiveProductsCount()
    {
        return Products.Count(p => p.IsActive && !p.IsDeleted);
    }

    /// <summary>
    /// Kategoriyani o'chirish mumkinmi (mahsulot yo'q bo'lsa)
    /// </summary>
    public bool CanBeDeleted()
    {
        return !Products.Any() && !Children.Any();
    }
}