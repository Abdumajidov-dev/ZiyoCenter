using ZiyoMarket.Domain.Common;
using ZiyoMarket.Domain.Enums;
using ZiyoMarket.Domain.Entities.Orders;

namespace ZiyoMarket.Domain.Entities.Delivery;

/// <summary>
/// Buyurtma yetkazib berish entity'si
/// </summary>
public class OrderDelivery : BaseEntity
{
    /// <summary>
    /// Buyurtma ID
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Yetkazib berish hamkori ID
    /// </summary>
    public int DeliveryPartnerId { get; set; }

    /// <summary>
    /// Tracking kod (kuzatuv raqami)
    /// </summary>
    public string? TrackingCode { get; set; }

    /// <summary>
    /// Yetkazib berish holati
    /// </summary>
    public DeliveryStatus DeliveryStatus { get; set; } = DeliveryStatus.Assigned;

    /// <summary>
    /// Yetkazib berish manzili
    /// </summary>
    public string DeliveryAddress { get; set; } = string.Empty;

    /// <summary>
    /// Yetkazib berish narxi
    /// </summary>
    public decimal DeliveryFee { get; set; }

    /// <summary>
    /// Tayinlangan sana
    /// </summary>
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Olib ketilgan sana
    /// </summary>
    public DateTime? PickedUpAt { get; set; }

    /// <summary>
    /// Yetkazilgan sana
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

    /// <summary>
    /// Muvaffaqiyatsiz bo'lgan sana
    /// </summary>
    public DateTime? FailedAt { get; set; }

    /// <summary>
    /// Muvaffaqiyatsizlik sababi
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// Qabul qiluvchi ismi
    /// </summary>
    public string? ReceiverName { get; set; }

    /// <summary>
    /// Qabul qiluvchi telefoni
    /// </summary>
    public string? ReceiverPhone { get; set; }

    /// <summary>
    /// Yetkazib beruvchi ID/nomi
    /// </summary>
    public string? DeliveryPersonId { get; set; }

    /// <summary>
    /// Izohlar
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Taxminiy yetkazib berish sanasi
    /// </summary>
    public DateTime? EstimatedDeliveryDate { get; set; }

    /// <summary>
    /// Urinishlar soni
    /// </summary>
    public int AttemptCount { get; set; } = 0;

    /// <summary>
    /// GPS koordinatalar (yetkazib berilgan joy)
    /// </summary>
    public string? DeliveryLocation { get; set; } // "lat,lng"

    // Navigation Properties

    /// <summary>
    /// Buyurtma
    /// </summary>
    public virtual Order Order { get; set; } = null!;

    /// <summary>
    /// Yetkazib berish hamkori
    /// </summary>
    public virtual DeliveryPartner DeliveryPartner { get; set; } = null!;

    // Business Methods

    /// <summary>
    /// Yetkazib berish jarayondami
    /// </summary>
    public bool IsInProgress => DeliveryStatus == DeliveryStatus.PickedUp ||
                               DeliveryStatus == DeliveryStatus.InTransit;

    /// <summary>
    /// Yetkazib berish yakunlanganmi
    /// </summary>
    public bool IsCompleted => DeliveryStatus == DeliveryStatus.Delivered;

    /// <summary>
    /// Yetkazib berish muvaffaqiyatsizmi
    /// </summary>
    public bool IsFailed => DeliveryStatus == DeliveryStatus.Failed;

    /// <summary>
    /// Kechikkanmi (taxminiy sanadan)
    /// </summary>
    public bool IsDelayed => EstimatedDeliveryDate.HasValue &&
                            EstimatedDeliveryDate.Value < DateTime.UtcNow &&
                            !IsCompleted && !IsFailed;

    /// <summary>
    /// Tracking kod generate qilish
    /// </summary>
    public static string GenerateTrackingCode()
    {
        var today = DateTime.UtcNow;
        var random = new Random().Next(100000, 999999);
        return $"TRK-{today:yyyyMMdd}-{random}";
    }

    /// <summary>
    /// Olib ketish
    /// </summary>
    public void PickUp(string? deliveryPersonId = null)
    {
        if (DeliveryStatus != DeliveryStatus.Assigned)
            throw new InvalidOperationException("Faqat assigned holatidigina olib ketish mumkin");

        DeliveryStatus = DeliveryStatus.PickedUp;
        PickedUpAt = DateTime.UtcNow;
        DeliveryPersonId = deliveryPersonId;
        MarkAsUpdated();
    }

    /// <summary>
    /// Yo'lda
    /// </summary>
    public void MarkInTransit()
    {
        if (DeliveryStatus != DeliveryStatus.PickedUp)
            throw new InvalidOperationException("Faqat picked up holatidigina in transit qilish mumkin");

        DeliveryStatus = DeliveryStatus.InTransit;
        MarkAsUpdated();
    }

    /// <summary>
    /// Yetkazib berish
    /// </summary>
    public void Deliver(string? receiverName = null, string? deliveryLocation = null)
    {
        if (DeliveryStatus != DeliveryStatus.InTransit && DeliveryStatus != DeliveryStatus.PickedUp)
            throw new InvalidOperationException("Yetkazib berish uchun in transit yoki picked up holatida bo'lishi kerak");

        DeliveryStatus = DeliveryStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        ReceiverName = receiverName?.Trim();
        DeliveryLocation = deliveryLocation?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Muvaffaqiyatsiz
    /// </summary>
    public void MarkAsFailed(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Muvaffaqiyatsizlik sababi ko'rsatilishi kerak");

        DeliveryStatus = DeliveryStatus.Failed;
        FailedAt = DateTime.UtcNow;
        FailureReason = reason.Trim();
        AttemptCount++;
        MarkAsUpdated();
    }

    /// <summary>
    /// Qayta urinish
    /// </summary>
    public void Retry(string? notes = null)
    {
        if (DeliveryStatus != DeliveryStatus.Failed)
            throw new InvalidOperationException("Faqat failed holatidigina qayta urinish mumkin");

        DeliveryStatus = DeliveryStatus.Assigned;
        FailedAt = null;
        FailureReason = null;

        if (!string.IsNullOrWhiteSpace(notes))
        {
            Notes = (Notes ?? "") + $" | Qayta urinish: {notes}";
        }

        // Taxminiy sanani yangilash
        if (DeliveryPartner != null)
        {
            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(DeliveryPartner.EstimatedDays);
        }

        MarkAsUpdated();
    }

    /// <summary>
    /// Tracking kod o'rnatish
    /// </summary>
    public void SetTrackingCode(string? trackingCode)
    {
        TrackingCode = trackingCode?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Taxminiy yetkazib berish sanasini o'rnatish
    /// </summary>
    public void SetEstimatedDeliveryDate(DateTime estimatedDate)
    {
        if (estimatedDate <= DateTime.UtcNow)
            throw new ArgumentException("Taxminiy sana kelajakda bo'lishi kerak");

        EstimatedDeliveryDate = estimatedDate;
        MarkAsUpdated();
    }

    /// <summary>
    /// Qabul qiluvchi ma'lumotlarini yangilash
    /// </summary>
    public void UpdateReceiverInfo(string? name, string? phone)
    {
        ReceiverName = name?.Trim();
        ReceiverPhone = phone?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Manzilni yangilash
    /// </summary>
    public void UpdateDeliveryAddress(string newAddress)
    {
        if (IsCompleted)
            throw new InvalidOperationException("Yetkazib berilgan buyurtma manzilini o'zgartirib bo'lmaydi");

        if (string.IsNullOrWhiteSpace(newAddress))
            throw new ArgumentException("Manzil bo'sh bo'la olmaydi");

        DeliveryAddress = newAddress.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Izohni yangilash
    /// </summary>
    public void UpdateNotes(string? notes)
    {
        Notes = notes?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Yetkazib berish davomiyligi
    /// </summary>
    public TimeSpan? GetDeliveryDuration()
    {
        if (!DeliveredAt.HasValue)
            return null;

        var startDate = PickedUpAt ?? AssignedAt;
        return DeliveredAt.Value.Subtract(startDate);
    }

    /// <summary>
    /// Jami davomiylik (assign'dan deliver'gacha)
    /// </summary>
    public TimeSpan GetTotalDuration()
    {
        var endDate = DeliveredAt ?? FailedAt ?? DateTime.UtcNow;
        return endDate.Subtract(AssignedAt);
    }

    /// <summary>
    /// Kechikish vaqti
    /// </summary>
    public TimeSpan? GetDelayTime()
    {
        if (!EstimatedDeliveryDate.HasValue)
            return null;

        var actualDate = DeliveredAt ?? DateTime.UtcNow;
        var delay = actualDate.Subtract(EstimatedDeliveryDate.Value);

        return delay.TotalHours > 0 ? delay : null;
    }

    /// <summary>
    /// Status tarixini olish
    /// </summary>
    public List<string> GetStatusHistory()
    {
        var history = new List<string>
        {
            $"Assigned: {AssignedAt:dd.MM.yyyy HH:mm}"
        };

        if (PickedUpAt.HasValue)
            history.Add($"Picked Up: {PickedUpAt:dd.MM.yyyy HH:mm}");

        if (DeliveredAt.HasValue)
            history.Add($"Delivered: {DeliveredAt:dd.MM.yyyy HH:mm}");

        if (FailedAt.HasValue)
            history.Add($"Failed: {FailedAt:dd.MM.yyyy HH:mm} - {FailureReason}");

        return history;
    }

    /// <summary>
    /// Delivery validatsiya
    /// </summary>
    public Result Validate()
    {
        var errors = new List<string>();

        if (OrderId <= 0)
            errors.Add("Buyurtma ID noto'g'ri");

        if (DeliveryPartnerId <= 0)
            errors.Add("Yetkazib berish hamkori ID noto'g'ri");

        if (string.IsNullOrWhiteSpace(DeliveryAddress))
            errors.Add("Yetkazib berish manzili bo'sh bo'la olmaydi");

        if (DeliveryFee < 0)
            errors.Add("Yetkazib berish narxi manfiy bo'la olmaydi");

        if (EstimatedDeliveryDate.HasValue && EstimatedDeliveryDate <= AssignedAt)
            errors.Add("Taxminiy yetkazib berish sanasi tayinlangan sanadan keyin bo'lishi kerak");

        if (DeliveryStatus == DeliveryStatus.Failed && string.IsNullOrWhiteSpace(FailureReason))
            errors.Add("Muvaffaqiyatsizlik sababi ko'rsatilishi kerak");

        return errors.Any() ? Result.Failure(errors) : Result.Success();
    }

    /// <summary>
    /// Yetkazib berish ma'lumotlarini to'liq formatda
    /// </summary>
    public override string ToString()
    {
        var info = $"Order #{Order?.OrderNumber} - {DeliveryStatus}";

        if (!string.IsNullOrEmpty(TrackingCode))
            info += $" (Tracking: {TrackingCode})";

        return info;
    }
}