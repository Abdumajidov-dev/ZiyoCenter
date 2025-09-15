namespace ZiyoMarket.Domain.Common;

/// <summary>
/// Barcha entity'lar uchun asosiy class
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Asosiy identifikator
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Yaratilgan sana (ISO formatda)
    /// </summary>
    public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

    /// <summary>
    /// O'zgartirilgan sana (ISO formatda)
    /// </summary>
    public string? UpdatedAt { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

    /// <summary>
    /// O'chirilgan sana (Soft Delete uchun)
    /// </summary>
    public string? DeletedAt { get; set; }

    /// <summary>
    /// O'chirilganmi yoki yo'qmi
    /// </summary>
    public bool IsDeleted => !string.IsNullOrEmpty(DeletedAt);

    /// <summary>
    /// Soft delete - o'chirish
    /// </summary>
    public virtual void Delete()
    {
        DeletedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// Soft delete'ni bekor qilish
    /// </summary>
    public virtual void Restore()
    {
        DeletedAt = null;
    }

    /// <summary>
    /// UpdatedAt ni yangilash
    /// </summary>
    public virtual void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
