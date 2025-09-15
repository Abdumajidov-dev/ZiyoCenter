namespace ZiyoMarket.Domain.Common;

/// <summary>
/// Audit maydonlari bilan entity (kim yaratgan, kim o'zgartirgan)
/// </summary>
public abstract class BaseAuditableEntity : BaseEntity
{
    /// <summary>
    /// Kim yaratgan (User ID)
    /// </summary>
    public int? CreatedBy { get; set; }

    /// <summary>
    /// Kim o'zgartirgan (User ID)
    /// </summary>
    public int? UpdatedBy { get; set; }

    /// <summary>
    /// Kim o'chirgan (User ID)
    /// </summary>
    public int? DeletedBy { get; set; }

    /// <summary>
    /// Audit ma'lumotlari bilan o'chirish
    /// </summary>
    /// <param name="deletedBy">O'chirgan foydalanuvchi ID</param>
    public virtual void Delete(int deletedBy)
    {
        base.Delete();
        DeletedBy = deletedBy;
    }

    /// <summary>
    /// Audit ma'lumotlari bilan yangilash
    /// </summary>
    /// <param name="updatedBy">Yangilagan foydalanuvchi ID</param>
    public virtual void MarkAsUpdated(int updatedBy)
    {
        base.MarkAsUpdated();
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// Yaratish ma'lumotlarini o'rnatish
    /// </summary>
    /// <param name="createdBy">Yaratgan foydalanuvchi ID</param>
    public virtual void SetCreatedBy(int createdBy)
    {
        CreatedBy = createdBy;
    }
}