using ZiyoMarket.Domain.Common;
using ZiyoMarket.Domain.Entities.Delivery;
using ZiyoMarket.Domain.Entities.Users;
using ZiyoMarket.Domain.Enums;

namespace ZiyoMarket.Domain.Entities.Orders;

/// <summary>
/// Buyurtma entity'si
/// </summary>
public class Order : BaseAuditableEntity
{
    /// <summary>
    /// Buyurtma uchun to‘lov talab qilinadimi
    /// </summary>
    public bool RequiresPayment
    {
        get
        {
            // Naqd to‘lov (Cash) bo‘lmasa yoki hali to‘lanmagan bo‘lsa — to‘lov talab qilinadi
            return PaymentMethod != PaymentMethod.Cash
                   && Status != OrderStatus.Delivered
                   && Status != OrderStatus.Cancelled;
        }
    }

    /// <summary>
    /// Mijoz ID
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Sotuvchi ID (agar sotuvchi yaratgan bo'lsa)
    /// </summary>
    public int? SellerId { get; set; }

    /// <summary>
    /// Buyurtma raqami (unique)
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// Buyurtma sanasi
    /// </summary>
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Buyurtma holati
    /// </summary>
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    /// <summary>
    /// Jami narx (chegirma va cashback'dan oldin)
    /// </summary>
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Qo'llangan chegirma summasi
    /// </summary>
    public decimal DiscountApplied { get; set; } = 0;

    /// <summary>
    /// Ishlatilgan cashback summasi
    /// </summary>
    public decimal CashbackUsed { get; set; } = 0;

    /// <summary>
    /// Yakuniy narx (to'lanadigan summa)
    /// </summary>
    public decimal FinalPrice => TotalPrice - DiscountApplied - CashbackUsed;

    /// <summary>
    /// To'lov usuli
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    /// <summary>
    /// Yetkazib berish turi
    /// </summary>
    public DeliveryType DeliveryType { get; set; } = DeliveryType.Pickup;

    /// <summary>
    /// Yetkazib berish manzili
    /// </summary>
    public string? DeliveryAddress { get; set; }

    /// <summary>
    /// Izohlar
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Tasdiqlangan sana
    /// </summary>
    public DateTime? ConfirmedAt { get; set; }

    /// <summary>
    /// Jo'natilgan sana
    /// </summary>
    public DateTime? ShippedAt { get; set; }

    /// <summary>
    /// Yetkazilgan sana
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

    /// <summary>
    /// Bekor qilingan sana
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>
    /// Bekor qilish sababi
    /// </summary>
    public string? CancellationReason { get; set; }

    // Navigation Properties

    /// <summary>
    /// Mijoz
    /// </summary>
    public virtual Customer Customer { get; set; } = null!;

    /// <summary>
    /// Sotuvchi
    /// </summary>
    public virtual Seller? Seller { get; set; }

    /// <summary>
    /// Buyurtma item'lari
    /// </summary>
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    /// <summary>
    /// Buyurtma chegirmalari
    /// </summary>
    public virtual ICollection<OrderDiscount> OrderDiscounts { get; set; } = new List<OrderDiscount>();

    /// <summary>
    /// Yetkazib berish ma'lumotlari
    /// </summary>
    public virtual OrderDelivery? OrderDelivery { get; set; }

    /// <summary>
    /// Cashback tranzaksiyalari
    /// </summary>
    public virtual ICollection<CashbackTransaction> CashbackTransactions { get; set; } = new List<CashbackTransaction>();

    // Business Methods

    /// <summary>
    /// Buyurtmani bekor qilish mumkinmi
    /// </summary>
    public bool CanBeCancelled => Status == OrderStatus.Pending || Status == OrderStatus.Confirmed;

    /// <summary>
    /// Buyurtma tasdiqlanganmi
    /// </summary>
    public bool IsConfirmed => Status != OrderStatus.Pending && Status != OrderStatus.Cancelled;

    /// <summary>
    /// Buyurtma yakunlanganmi
    /// </summary>
    public bool IsCompleted => Status == OrderStatus.Delivered;

    /// <summary>
    /// Online buyurtmami
    /// </summary>
    public bool IsOnlineOrder => PaymentMethod == PaymentMethod.Card;

    /// <summary>
    /// Offline buyurtmami (sotuvchi tomonidan)
    /// </summary>
    public bool IsOfflineOrder => SellerId.HasValue;

    /// <summary>
    /// Buyurtma raqamini generate qilish
    /// </summary>
    public static string GenerateOrderNumber()
    {
        var today = DateTime.UtcNow;
        var random = new Random().Next(1000, 9999);
        return $"ORD-{today:yyyyMMdd}-{random}";
    }

    /// <summary>
    /// Buyurtmani tasdiqlash
    /// </summary>
    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Faqat pending buyurtmalarni tasdiqlash mumkin");

        Status = OrderStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    /// <summary>
    /// Tayyorlashni boshlash
    /// </summary>
    public void StartPreparing()
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException("Faqat tasdiqlangan buyurtmalarni tayyorlash mumkin");

        Status = OrderStatus.Preparing;
        MarkAsUpdated();
    }

    /// <summary>
    /// Olib ketishga tayyor
    /// </summary>
    public void MarkReadyForPickup()
    {
        if (Status != OrderStatus.Preparing)
            throw new InvalidOperationException("Faqat tayyorlanayotgan buyurtmalarni tayyor deb belgilash mumkin");

        Status = OrderStatus.ReadyForPickup;
        MarkAsUpdated();
    }

    /// <summary>
    /// Jo'natish
    /// </summary>
    public void Ship()
    {
        if (Status != OrderStatus.ReadyForPickup && Status != OrderStatus.Preparing)
            throw new InvalidOperationException("Buyurtmani jo'natish uchun tayyor bo'lishi kerak");

        Status = OrderStatus.Shipped;
        ShippedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    /// <summary>
    /// Yetkazib berish
    /// </summary>
    public void Deliver()
    {
        if (Status != OrderStatus.Shipped && Status != OrderStatus.ReadyForPickup)
            throw new InvalidOperationException("Faqat jo'natilgan yoki tayyor buyurtmalarni yetkazish mumkin");

        Status = OrderStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    /// <summary>
    /// Buyurtmani bekor qilish
    /// </summary>
    public void Cancel(string reason)
    {
        if (!CanBeCancelled)
            throw new InvalidOperationException("Bu buyurtmani bekor qilib bo'lmaydi");

        Status = OrderStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason;
        MarkAsUpdated();
    }

    /// <summary>
    /// Chegirma qo'llash
    /// </summary>
    public void ApplyDiscount(decimal discountAmount, int discountReasonId, int? appliedBySellerId = null)
    {
        if (discountAmount < 0)
            throw new ArgumentException("Chegirma summasi manfiy bo'la olmaydi");

        if (discountAmount > TotalPrice)
            throw new ArgumentException("Chegirma summasi jami narxdan ko'p bo'la olmaydi");

        DiscountApplied += discountAmount;

        // Discount reason yozuvi
        var orderDiscount = new OrderDiscount
        {
            OrderId = Id,
            DiscountReasonId = discountReasonId,
            DiscountAmount = discountAmount,
            AppliedBySellerId = appliedBySellerId
        };

        OrderDiscounts.Add(orderDiscount);
        MarkAsUpdated();
    }

    /// <summary>
    /// Cashback ishlatish
    /// </summary>
    public void UseCashback(decimal cashbackAmount)
    {
        if (cashbackAmount < 0)
            throw new ArgumentException("Cashback summasi manfiy bo'la olmaydi");

        var remainingAmount = FinalPrice - cashbackAmount;
        if (remainingAmount < 0)
            throw new ArgumentException("Cashback summasi buyurtma qiymatidan ko'p bo'la olmaydi");

        CashbackUsed += cashbackAmount;
        MarkAsUpdated();
    }

    /// <summary>
    /// Buyurtmadan cashback hisoblash (2%)
    /// </summary>
    public decimal CalculateCashbackAmount()
    {
        if (Status != OrderStatus.Delivered)
            return 0;

        // Faqat yetkazilgan buyurtmalardan cashback
        return FinalPrice * 0.02m; // 2%
    }

    /// <summary>
    /// Jami item'lar soni
    /// </summary>
    public int GetTotalItemsCount()
    {
        return OrderItems.Sum(oi => oi.Quantity);
    }

    /// <summary>
    /// Buyurtma davomiyligi
    /// </summary>
    public TimeSpan GetOrderDuration()
    {
        var endDate = DeliveredAt ?? CancelledAt ?? DateTime.UtcNow;
        return endDate.Subtract(OrderDate);
    }

    /// <summary>
    /// Buyurtma item qo'shish
    /// </summary>
    public void AddItem(int productId, int quantity, decimal unitPrice)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Faqat pending buyurtmalarga item qo'shish mumkin");

        var existingItem = OrderItems.FirstOrDefault(oi => oi.ProductId == productId);

        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var newItem = new OrderItem
            {
                OrderId = Id,
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = unitPrice
            };
            OrderItems.Add(newItem);
        }

        RecalculateTotal();
    }

    /// <summary>
    /// Jami narxni qayta hisoblash
    /// </summary>
    public void RecalculateTotal()
    {
        TotalPrice = OrderItems.Sum(oi => oi.TotalPrice);
        MarkAsUpdated();
    }

    /// <summary>
    /// To'lov usulini o'rnatish
    /// </summary>
    public void SetPaymentMethod(PaymentMethod paymentMethod)
    {
        PaymentMethod = paymentMethod;
        MarkAsUpdated();
    }

    /// <summary>
    /// Yetkazib berish turini o'rnatish
    /// </summary>
    public void SetDeliveryType(DeliveryType deliveryType, string? deliveryAddress = null)
    {
        DeliveryType = deliveryType;

        if (deliveryType != DeliveryType.Pickup && string.IsNullOrWhiteSpace(deliveryAddress))
            throw new ArgumentException("Yetkazib berish uchun manzil kerak");

        DeliveryAddress = deliveryAddress;
        MarkAsUpdated();
    }
}