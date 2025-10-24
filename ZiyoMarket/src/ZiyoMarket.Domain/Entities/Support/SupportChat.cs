using ZiyoMarket.Domain.Common;
using ZiyoMarket.Domain.Enums;
using ZiyoMarket.Domain.Entities.Users;

namespace ZiyoMarket.Domain.Entities.Support;

/// <summary>
/// Support chat entity'si
/// </summary>
public class SupportChat : BaseEntity
{
    /// <summary>
    ///     
    /// yaratgan admin ID
    ///</summary>
    public int CreatedBy { get; set; }
    /// <summary>
    ///     
    /// o'chirgan admin ID
    ///</summary>
    public int DeletedBy { get; set; }
    /// <summary>
    /// permession uhcun rezolutsiya
    /// </summary>
    public string Resolution { get; set; } = string.Empty;
    /// <summary>
    /// Mijoz ID
    /// </summary>
    public int CustomerId { get; set; }
    /// <summary>
    /// Order ID (agar mavjud bo'lsa)
    /// </summary>
    public DateTime? StartedAt { get; set; }
    public int? OrderId { get; set; }
    /// <summary>
    /// oxirgi yangilagan foydalanuvchi ID
    /// </summary>
    public int? UpdatedBy { get; set; }

    /// <summary>
    /// Admin ID (javob beruvchi)
    /// </summary>
    public int? AdminId { get; set; }

    /// <summary>
    /// Chat mavzusi
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Chat holati
    /// </summary>
    public SupportChatStatus Status { get; set; } = SupportChatStatus.Open;

    /// <summary>
    /// Muhimlik darajasi
    /// </summary>
    public string? Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent

    /// <summary>
    /// Kategoriya (Buyurtma, Mahsulot, To'lov, etc.)
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Yopilgan sana
    /// </summary>
    public DateTime? ClosedAt { get; set; }

    /// <summary>
    /// Yopilish sababi
    /// </summary>
    public string? CloseReason { get; set; }

    /// <summary>
    /// Mijoz reytingi (1-5)
    /// </summary>
    public int? CustomerRating { get; set; }

    /// <summary>
    /// Mijoz sharhi
    /// </summary>
    public string? CustomerFeedback { get; set; }

    /// <summary>
    /// Admin javob bergan sana
    /// </summary>
    public string? FirstResponseAt { get; set; }

    /// <summary>
    /// Oxirgi faollik
    /// </summary>
    public string? LastActivityAt { get; set; } 

    /// <summary>
    /// Avtomatik yopilish sanasi (faollik yo'q bo'lsa)
    /// </summary>
    public string? AutoCloseAt { get; set; }

    /// <summary>
    /// Tags (qidiruv uchun)
    /// </summary>
    public string? Tags { get; set; }

    // Navigation Properties

    /// <summary>
    /// Mijoz
    /// </summary>
    public virtual Customer Customer { get; set; } = null!;

    /// <summary>
    /// Admin (javob beruvchi)
    /// </summary>
    public virtual Admin? Admin { get; set; }

    /// <summary>
    /// Chat xabarlari
    /// </summary>
    public virtual ICollection<SupportMessage> Messages { get; set; } = new List<SupportMessage>();

    // Business Methods

    /// <summary>
    /// Ochiq chatmi
    /// </summary>
    public bool IsOpen => Status == SupportChatStatus.Open;

    /// <summary>
    /// Jarayonda chatmi
    /// </summary>
    public bool IsInProgress => Status == SupportChatStatus.InProgress;

    /// <summary>
    /// Yopilgan chatmi
    /// </summary>
    public bool IsClosed => Status == SupportChatStatus.Closed;

    /// <summary>
    /// Admin javob berganmi
    /// </summary>
    public bool HasAdminResponse => !string.IsNullOrEmpty(FirstResponseAt);

    /// <summary>
    /// Urgent chatmi
    /// </summary>
    public bool IsUrgent => Priority == "Urgent";

    /// <summary>
    /// High priority chatmi
    /// </summary>
    public bool IsHighPriority => Priority == "High" || Priority == "Urgent";

    /// <summary>
    /// Avtomatik yopilishi kerakmi
    /// </summary>
    public bool ShouldAutoClose
    {
        get
        {
            if (string.IsNullOrEmpty(AutoCloseAt))
                return false;

            if (DateTime.TryParse(AutoCloseAt, out var autoCloseAt))
            {
                return autoCloseAt <= DateTime.UtcNow && !IsClosed;
            }

            return false; // noto‘g‘ri format bo‘lsa
        }
    }


    /// <summary>
    /// Chat'ni admin'ga assign qilish
    /// </summary>
    public void AssignToAdmin(int adminId)
    {
        if (IsClosed)
            throw new InvalidOperationException("Yopilgan chat'ni assign qilib bo'lmaydi");

        AdminId = adminId;
        Status = SupportChatStatus.InProgress;

        // Agar birinchi marta assign qilinayotgan bo'lsa
        if (string.IsNullOrEmpty(FirstResponseAt))
        {
            FirstResponseAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        }

        UpdateActivity();
    }

    /// <summary>
    /// Chat'ni yopish
    /// </summary>
    public void Close(string reason, int? closedByAdminId = null)
    {
        if (IsClosed)
            throw new InvalidOperationException("Chat allaqachon yopilgan");

        Status = SupportChatStatus.Closed;
        ClosedAt = DateTime.UtcNow;
        CloseReason = reason?.Trim();

        if (closedByAdminId.HasValue)
        {
            AdminId = closedByAdminId.Value;
        }

        UpdateActivity();
    }

    /// <summary>
    /// Chat'ni qayta ochish
    /// </summary>
    public void Reopen()
    {
        if (!IsClosed)
            throw new InvalidOperationException("Faqat yopilgan chat'ni qayta ochish mumkin");

        Status = SupportChatStatus.Open;
        ClosedAt = null;
        CloseReason = null;
        AutoCloseAt = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd HH:mm:ss");
        UpdateActivity();
    }


    /// <summary>
    /// Mavzuni yangilash
    /// </summary>
    public void UpdateSubject(string? newSubject)
    {
        Subject = newSubject?.Trim();
        UpdateActivity();
    }

    /// <summary>
    /// Muhimlik darajasini o'rnatish
    /// </summary>
    public void SetPriority(string priority)
    {
        var allowedPriorities = new[] { "Low", "Normal", "High", "Urgent" };

        if (!allowedPriorities.Contains(priority))
            throw new ArgumentException($"Noto'g'ri muhimlik darajasi: {priority}");

        Priority = priority;
        UpdateActivity();
    }

    /// <summary>
    /// Kategoriyani o'rnatish
    /// </summary>
    public void SetCategory(string? category)
    {
        Category = category?.Trim();
        UpdateActivity();
    }

    /// <summary>
    /// Mijoz reytingini o'rnatish
    /// </summary>
    public void SetCustomerRating(int rating, string? feedback = null)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Reyting 1 dan 5 gacha bo'lishi kerak");

        if (!IsClosed)
            throw new InvalidOperationException("Faqat yopilgan chat'ga reyting berish mumkin");

        CustomerRating = rating;
        CustomerFeedback = feedback?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Tags qo'shish
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
    /// Faollikni yangilash
    /// </summary>
    public void UpdateActivity()
    {
        LastActivityAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

        // Agar ochiq bo'lsa va avtomatik yopilish belgilanmagan bo'lsa
        if (IsOpen && string.IsNullOrEmpty(AutoCloseAt))
        {
            AutoCloseAt = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd HH:mm:ss");
        }

        MarkAsUpdated();
    }


    /// <summary>
    /// Javob berish vaqtini hisoblash
    /// </summary>
    public TimeSpan? GetResponseTime()
    {
        if (string.IsNullOrEmpty(FirstResponseAt))
            return null;

        if (DateTime.TryParse(CreatedAt, out var created) &&
            DateTime.TryParse(FirstResponseAt, out var firstResponse))
        {
            return firstResponse.Subtract(created);
        }

        return null; // noto‘g‘ri format bo‘lsa
    }

    /// <summary>
    /// Chat davomiyligi
    /// </summary>
    public TimeSpan? GetChatDuration()
    {
        if (!DateTime.TryParse(CreatedAt, out var created))
            return null;

        var endTime = DateTime.TryParse(DeletedAt, out var closed)
            ? closed
            : DateTime.UtcNow;

        return endTime.Subtract(created);
    }



    /// <summary>
    /// Faolsizlik vaqti
    /// </summary>
    public TimeSpan? GetInactivityTime()
    {
        if (DateTime.TryParse(LastActivityAt, out var lastActivity))
        {
            return DateTime.UtcNow.Subtract(lastActivity);
        }

        return null; // noto‘g‘ri yoki bo‘sh bo‘lsa
    }


    /// <summary>
    /// Xabarlar soni
    /// </summary>
    public int GetMessageCount()
    {
        return Messages.Count;
    }

    /// <summary>
    /// Mijoz xabarlari soni
    /// </summary>
    public int GetCustomerMessageCount()
    {
        return Messages.Count(m => m.SenderType == UserType.Customer);
    }

    /// <summary>
    /// Admin xabarlari soni
    /// </summary>
    public int GetAdminMessageCount()
    {
        return Messages.Count(m => m.SenderType == UserType.Admin);
    }

    /// <summary>
    /// Oxirgi xabar
    /// </summary>
    public SupportMessage? GetLastMessage()
    {
        return Messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();
    }

    /// <summary>
    /// Chat validatsiya
    /// </summary>
    public Result Validate()
    {
        var errors = new List<string>();

        if (CustomerId <= 0)
            errors.Add("Mijoz ID noto'g'ri");

        var allowedPriorities = new[] { "Low", "Normal", "High", "Urgent" };
        if (!allowedPriorities.Contains(Priority))
            errors.Add("Noto'g'ri muhimlik darajasi");

        if (CustomerRating.HasValue && (CustomerRating < 1 || CustomerRating > 5))
            errors.Add("Reyting 1 dan 5 gacha bo'lishi kerak");

        if (IsClosed && string.IsNullOrWhiteSpace(CloseReason))
            errors.Add("Yopilgan chat uchun sabab ko'rsatilishi kerak");

        return errors.Any() ? Result.Failure(errors) : Result.Success();
    }

    /// <summary>
    /// Chat ma'lumotlarini qisqa formatda
    /// </summary>
    public string GetShortDescription()
    {
        var desc = $"Chat #{Id}";

        if (!string.IsNullOrEmpty(Subject))
            desc += $" - {Subject}";

        desc += $" ({Status})";

        if (IsHighPriority)
            desc += $" [{Priority}]";

        return desc;
    }

    /// <summary>
    /// Chat ma'lumotlarini to'liq formatda
    /// </summary>
    public override string ToString()
    {
        return $"Chat #{Id} - {Customer?.FullName} ({Status}, {Priority})";
    }
}