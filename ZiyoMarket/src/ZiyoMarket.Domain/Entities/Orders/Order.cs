using ZiyoMarket.Domain.Common;
using ZiyoMarket.Domain.Entities.Users;
using ZiyoMarket.Domain.Entities.Delivery;
using ZiyoMarket.Domain.Enums;

namespace ZiyoMarket.Domain.Entities.Orders;

/// <summary>
/// Buyurtma entity'si - offline va online sotuvlar
/// </summary>
public class Order : BaseAuditableEntity
{
    // ==================== PROPERTIES ====================

    /// <summary>
    /// Buyurtma raqami (unique)
    /// Format: ORD-20250118-001
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// Mijoz ID
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Sotuvchi ID (NULL agar online buyurtma)
    /// </summary>
    public int? SellerId { get; set; }

    /// <summary>
    /// Buyurtma sanasi (string format: "yyyy-MM-dd HH:mm:ss")
    /// </summary>
    public string OrderDate { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

    /// <summary>
    /// Buyurtma holati
    /// </summary>
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    // ==================== PRICING ====================

    /// <summary>
    /// Jami narx (barcha item'lar summasi)
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
    /// Yakuniy narx (TotalPrice - DiscountApplied - CashbackUsed)
    /// </summary>
    public decimal FinalPrice { get; set; }

    // ==================== PAYMENT ====================

    /// <summary>
    /// To'lov usuli
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// To'lov reference (transaction ID from payment gateway)
    /// </summary>
    public string? PaymentReference { get; set; }

    /// <summary>
    /// To'lov qilingan sana (string format: "yyyy-MM-dd HH:mm:ss")
    /// </summary>
    public string? PaidAt { get; set; }

    // ==================== DELIVERY ====================

    /// <summary>
    /// Yetkazib berish turi
    /// </summary>
    public DeliveryType DeliveryType { get; set; } = DeliveryType.Pickup;

    /// <summary>
    /// Yetkazib berish manzili
    /// </summary>
    public string? DeliveryAddress { get; set; }

    /// <summary>
    /// Yetkazib berish to'lovi
    /// </summary>
    public decimal DeliveryFee { get; set; } = 0;

    // ==================== NOTES ====================

    /// <summary>
    /// Mijoz izohlari
    /// </summary>
    public string? CustomerNotes { get; set; }

    /// <summary>
    /// Sotuvchi izohlari
    /// </summary>
    public string? SellerNotes { get; set; }

    /// <summary>
    /// Admin izohlari
    /// </summary>
    public string? AdminNotes { get; set; }

    // ==================== NAVIGATION PROPERTIES ====================

    /// <summary>
    /// Mijoz
    /// </summary>
    public virtual Customer Customer { get; set; } = null!;

    /// <summary>
    /// Sotuvchi (NULL agar online)
    /// </summary>
    public virtual Seller? Seller { get; set; }

    /// <summary>
    /// Buyurtma item'lari
    /// </summary>
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    /// <summary>
    /// Qo'llangan chegirmalar
    /// </summary>
    public virtual ICollection<OrderDiscount> OrderDiscounts { get; set; } = new List<OrderDiscount>();

    /// <summary>
    /// Yetkazib berish ma'lumotlari
    /// </summary>
    public virtual OrderDelivery? OrderDelivery { get; set; }

    /// <summary>
    /// Cashback tranzaksiyalari (earned)
    /// </summary>
    public virtual ICollection<CashbackTransaction> CashbackTransactions { get; set; } = new List<CashbackTransaction>();

    // ==================== COMPUTED PROPERTIES ====================

    /// <summary>
    /// Online buyurtmami
    /// </summary>
    public bool IsOnlineOrder => !SellerId.HasValue;

    /// <summary>
    /// Offline buyurtmami
    /// </summary>
    public bool IsOfflineOrder => SellerId.HasValue;

    /// <summary>
    /// To'lov qilinganmi
    /// </summary>
    public bool IsPaid => !string.IsNullOrEmpty(PaidAt);

    /// <summary>
    /// Buyurtmani bekor qilish mumkinmi
    /// </summary>
    public bool CanBeCancelled => Status == OrderStatus.Pending || Status == OrderStatus.Confirmed;

    /// <summary>
    /// Buyurtma tugallanganmi
    /// </summary>
    public bool IsCompleted => Status == OrderStatus.Delivered;

    /// <summary>
    /// Buyurtma bekor qilinganmi
    /// </summary>
    public bool IsCancelled => Status == OrderStatus.Cancelled;

    /// <summary>
    /// To'lov talab qilinadimi
    /// </summary>
    public bool RequiresPayment => !IsPaid && Status != OrderStatus.Cancelled;

    /// <summary>
    /// Yetkazib berish talab qilinadimi
    /// </summary>
    public bool RequiresDelivery => DeliveryType != DeliveryType.Pickup && !IsCompleted && !IsCancelled;

    /// <summary>
    /// Buyurtmadagi item'lar soni
    /// </summary>
    public int ItemsCount => OrderItems?.Count ?? 0;

    /// <summary>
    /// Chegirma foizi
    /// </summary>
    public decimal DiscountPercentage => TotalPrice > 0 ? (DiscountApplied / TotalPrice) * 100 : 0;

    /// <summary>
    /// Cashback ishlatish foizi
    /// </summary>
    public decimal CashbackUsagePercentage => TotalPrice > 0 ? (CashbackUsed / TotalPrice) * 100 : 0;

    // ==================== STATIC FACTORY METHODS ====================

    /// <summary>
    /// Buyurtma raqamini generate qilish
    /// Format: ORD-20250118-001
    /// </summary>
    public static string GenerateOrderNumber()
    {
        var today = DateTime.UtcNow;
        var random = new Random().Next(100, 999);
        return $"ORD-{today:yyyyMMdd}-{random}";
    }

    /// <summary>
    /// Online buyurtma yaratish (Customer tomonidan)
    /// </summary>
    public static Order CreateOnlineOrder(
        int customerId,
        List<OrderItem> items,
        PaymentMethod paymentMethod,
        DeliveryType deliveryType,
        string? deliveryAddress = null,
        decimal cashbackToUse = 0,
        string? customerNotes = null)
    {
        if (items == null || !items.Any())
            throw new ArgumentException("Buyurtmada kamida 1 ta item bo'lishi kerak");

        if (deliveryType != DeliveryType.Pickup && string.IsNullOrWhiteSpace(deliveryAddress))
            throw new ArgumentException("Yetkazib berish manzili kerak");

        var totalPrice = items.Sum(i => i.TotalPrice);

        if (cashbackToUse > totalPrice)
            throw new ArgumentException("Cashback summasi buyurtma summasidan ko'p bo'la olmaydi");

        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            CustomerId = customerId,
            SellerId = null, // Online order
            OrderDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            Status = OrderStatus.Pending,
            TotalPrice = totalPrice,
            CashbackUsed = cashbackToUse,
            FinalPrice = totalPrice - cashbackToUse,
            PaymentMethod = paymentMethod,
            DeliveryType = deliveryType,
            DeliveryAddress = deliveryAddress,
            CustomerNotes = customerNotes,
            CreatedBy = customerId
        };

        foreach (var item in items)
        {
            item.OrderId = order.Id;
            order.OrderItems.Add(item);
        }

        return order;
    }

    /// <summary>
    /// Offline buyurtma yaratish (Seller tomonidan)
    /// </summary>
    public static Order CreateOfflineOrder(
        int customerId,
        int sellerId,
        List<OrderItem> items,
        decimal discountAmount = 0,
        string? sellerNotes = null)
    {
        if (items == null || !items.Any())
            throw new ArgumentException("Buyurtmada kamida 1 ta item bo'lishi kerak");

        var totalPrice = items.Sum(i => i.TotalPrice);

        if (discountAmount > totalPrice)
            throw new ArgumentException("Chegirma summasi buyurtma summasidan ko'p bo'la olmaydi");

        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            CustomerId = customerId,
            SellerId = sellerId,
            OrderDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            Status = OrderStatus.Confirmed, // Offline order darhol confirmed
            TotalPrice = totalPrice,
            DiscountApplied = discountAmount,
            FinalPrice = totalPrice - discountAmount,
            PaymentMethod = PaymentMethod.Cash, // Offline always cash
            DeliveryType = DeliveryType.Pickup, // Offline always pickup
            SellerNotes = sellerNotes,
            CreatedBy = sellerId
        };

        foreach (var item in items)
        {
            item.OrderId = order.Id;
            order.OrderItems.Add(item);
        }

        // Mark as paid immediately for cash orders
        order.PaidAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

        return order;
    }

    // ==================== STATUS MANAGEMENT ====================

    /// <summary>
    /// Buyurtmani tasdiqlash (Pending → Confirmed)
    /// </summary>
    public void Confirm(int confirmedBy)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Faqat Pending buyurtmani tasdiqlash mumkin. Hozirgi status: {Status}");

        Status = OrderStatus.Confirmed;
        UpdatedBy = confirmedBy;
        MarkAsUpdated();
    }

    /// <summary>
    /// Tayyorlanayotgan holatiga o'tkazish (Confirmed → Preparing)
    /// </summary>
    public void MarkAsPreparing(int updatedBy)
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException($"Faqat Confirmed buyurtmani Preparing holatiga o'tkazish mumkin");

        Status = OrderStatus.Preparing;
        UpdatedBy = updatedBy;
        MarkAsUpdated();
    }

    /// <summary>
    /// Olib ketishga tayyor (Preparing → ReadyForPickup)
    /// </summary>
    public void MarkAsReadyForPickup(int updatedBy)
    {
        if (Status != OrderStatus.Preparing)
            throw new InvalidOperationException($"Faqat Preparing buyurtmani ReadyForPickup holatiga o'tkazish mumkin");

        if (DeliveryType != DeliveryType.Pickup)
            throw new InvalidOperationException("Faqat Pickup buyurtmalarni ReadyForPickup holatiga o'tkazish mumkin");

        Status = OrderStatus.ReadyForPickup;
        UpdatedBy = updatedBy;
        MarkAsUpdated();
    }

    /// <summary>
    /// Jo'natildi (ReadyForPickup → Shipped)
    /// </summary>
    public void MarkAsShipped(int updatedBy)
    {
        if (Status != OrderStatus.Preparing && Status != OrderStatus.ReadyForPickup)
            throw new InvalidOperationException($"Buyurtmani Shipped holatiga o'tkazib bo'lmaydi");

        if (DeliveryType == DeliveryType.Pickup)
            throw new InvalidOperationException("Pickup buyurtmalarni Shipped holatiga o'tkazish mumkin emas");

        Status = OrderStatus.Shipped;
        UpdatedBy = updatedBy;
        MarkAsUpdated();
    }

    /// <summary>
    /// Yetkazildi (Shipped/ReadyForPickup → Delivered)
    /// </summary>
    public void MarkAsDelivered(int updatedBy)
    {
        if (Status != OrderStatus.Shipped && Status != OrderStatus.ReadyForPickup)
            throw new InvalidOperationException($"Buyurtmani Delivered holatiga o'tkazib bo'lmaydi");

        if (!IsPaid)
            throw new InvalidOperationException("To'lanmagan buyurtmani Delivered holatiga o'tkazib bo'lmaydi");

        Status = OrderStatus.Delivered;
        UpdatedBy = updatedBy;
        MarkAsUpdated();
    }

    /// <summary>
    /// Buyurtmani bekor qilish
    /// </summary>
    public void Cancel(int cancelledBy, string? reason = null)
    {
        if (!CanBeCancelled)
            throw new InvalidOperationException($"Bu holatdagi buyurtmani bekor qilib bo'lmaydi. Status: {Status}");

        Status = OrderStatus.Cancelled;
        AdminNotes = string.IsNullOrEmpty(AdminNotes)
            ? $"Bekor qilindi: {reason}"
            : $"{AdminNotes}\nBekor qilindi: {reason}";
        UpdatedBy = cancelledBy;
        MarkAsUpdated();
    }

    // ==================== PAYMENT METHODS ====================

    /// <summary>
    /// To'lovni qayd qilish
    /// </summary>
    public void ProcessPayment(PaymentMethod method, string? reference = null)
    {
        if (IsPaid)
            throw new InvalidOperationException("Buyurtma allaqachon to'langan");

        if (IsCancelled)
            throw new InvalidOperationException("Bekor qilingan buyurtma uchun to'lov qabul qilinmaydi");

        PaymentMethod = method;
        PaymentReference = reference;
        PaidAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        MarkAsUpdated();
    }

    /// <summary>
    /// To'lovni qaytarish (bekor qilishda)
    /// </summary>
    public void RefundPayment(string? reason = null)
    {
        if (!IsPaid)
            throw new InvalidOperationException("To'lanmagan buyurtmani refund qilib bo'lmaydi");

        AdminNotes = string.IsNullOrEmpty(AdminNotes)
            ? $"To'lov qaytarildi: {reason}"
            : $"{AdminNotes}\nTo'lov qaytarildi: {reason}";

        // Don't clear PaidAt - keep history
        MarkAsUpdated();
    }

    // ==================== DISCOUNT METHODS ====================

    /// <summary>
    /// Chegirma qo'llash (seller tomonidan)
    /// </summary>
    public void ApplyDiscount(decimal discountAmount, int discountReasonId, int appliedBy, string? notes = null)
    {
        if (discountAmount <= 0)
            throw new ArgumentException("Chegirma summasi musbat bo'lishi kerak");

        if (discountAmount > TotalPrice)
            throw new ArgumentException("Chegirma summasi buyurtma summasidan ko'p bo'la olmaydi");

        if (IsCompleted || IsCancelled)
            throw new InvalidOperationException("Tugallangan yoki bekor qilingan buyurtmaga chegirma qo'shib bo'lmaydi");

        // Create discount record
        var orderDiscount = new OrderDiscount
        {
            OrderId = this.Id,
            DiscountReasonId = discountReasonId,
            Amount = discountAmount,
            AppliedBy = appliedBy,
            Notes = notes
        };

        OrderDiscounts.Add(orderDiscount);

        // Update totals
        DiscountApplied += discountAmount;
        RecalculateFinalPrice();
        MarkAsUpdated();
    }

    /// <summary>
    /// Chegirmani olib tashlash
    /// </summary>
    public void RemoveDiscount(int orderDiscountId)
    {
        var discount = OrderDiscounts.FirstOrDefault(d => d.Id == orderDiscountId);
        if (discount == null)
            throw new ArgumentException("Chegirma topilmadi");

        if (IsCompleted || IsCancelled)
            throw new InvalidOperationException("Tugallangan yoki bekor qilingan buyurtmadan chegirma olib bo'lmaydi");

        DiscountApplied -= discount.Amount;
        OrderDiscounts.Remove(discount);
        RecalculateFinalPrice();
        MarkAsUpdated();
    }

    /// <summary>
    /// Chegirmani item'larga taqsimlash (eng arzon item'dan boshlab)
    /// </summary>
    public void DistributeDiscountToItems()
    {
        if (DiscountApplied <= 0)
            return;

        // Sort items by price (cheapest first)
        var sortedItems = OrderItems.OrderBy(i => i.UnitPrice).ToList();
        var remainingDiscount = DiscountApplied;

        foreach (var item in sortedItems)
        {
            if (remainingDiscount <= 0)
                break;

            var itemTotal = item.SubTotal;
            var itemDiscount = Math.Min(remainingDiscount, itemTotal);

            item.ApplyDiscount(itemDiscount);
            remainingDiscount -= itemDiscount;
        }
    }

    // ==================== CASHBACK METHODS ====================

    /// <summary>
    /// Cashback ishlatish
    /// </summary>
    public void UseCashback(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Cashback summasi musbat bo'lishi kerak");

        if (amount > TotalPrice - DiscountApplied)
            throw new ArgumentException("Cashback summasi buyurtma summasidan ko'p bo'la olmaydi");

        CashbackUsed = amount;
        RecalculateFinalPrice();
        MarkAsUpdated();
    }

    /// <summary>
    /// Cashback'ni qaytarish (bekor qilishda)
    /// </summary>
    public void RefundCashback()
    {
        if (CashbackUsed <= 0)
            return;

        CashbackUsed = 0;
        RecalculateFinalPrice();
        MarkAsUpdated();
    }

    /// <summary>
    /// Bu buyurtmadan cashback yig'ilishi kerakmi
    /// </summary>
    public bool ShouldEarnCashback()
    {
        return IsCompleted &&
               IsPaid &&
               FinalPrice > 0 &&
               !IsCancelled;
    }

    /// <summary>
    /// Cashback summasi (2%)
    /// </summary>
    public decimal CalculateCashbackAmount()
    {
        if (!ShouldEarnCashback())
            return 0;

        return Math.Round(FinalPrice * 0.02m, 2);
    }

    // ==================== ITEM MANAGEMENT ====================

    /// <summary>
    /// Item qo'shish
    /// </summary>
    public void AddItem(OrderItem item)
    {
        if (IsCompleted || IsCancelled)
            throw new InvalidOperationException("Tugallangan yoki bekor qilingan buyurtmaga item qo'shib bo'lmaydi");

        item.OrderId = this.Id;
        OrderItems.Add(item);
        RecalculateTotalPrice();
        MarkAsUpdated();
    }

    /// <summary>
    /// Item o'chirish
    /// </summary>
    public void RemoveItem(int orderItemId)
    {
        if (IsCompleted || IsCancelled)
            throw new InvalidOperationException("Tugallangan yoki bekor qilingan buyurtmadan item olib bo'lmaydi");

        var item = OrderItems.FirstOrDefault(i => i.Id == orderItemId);
        if (item == null)
            throw new ArgumentException("Item topilmadi");

        OrderItems.Remove(item);
        RecalculateTotalPrice();
        MarkAsUpdated();
    }

    /// <summary>
    /// Item miqdorini yangilash
    /// </summary>
    public void UpdateItemQuantity(int orderItemId, int newQuantity)
    {
        if (IsCompleted || IsCancelled)
            throw new InvalidOperationException("Tugallangan yoki bekor qilingan buyurtmada item yangilab bo'lmaydi");

        var item = OrderItems.FirstOrDefault(i => i.Id == orderItemId);
        if (item == null)
            throw new ArgumentException("Item topilmadi");

        item.UpdateQuantity(newQuantity);
        RecalculateTotalPrice();
        MarkAsUpdated();
    }

    // ==================== DELIVERY METHODS ====================

    /// <summary>
    /// Yetkazib berish to'lovini qo'shish
    /// </summary>
    public void SetDeliveryFee(decimal fee)
    {
        if (fee < 0)
            throw new ArgumentException("Yetkazib berish to'lovi manfiy bo'la olmaydi");

        if (DeliveryType == DeliveryType.Pickup)
            throw new InvalidOperationException("Pickup buyurtmada yetkazib berish to'lovi bo'lmaydi");

        DeliveryFee = fee;
        RecalculateFinalPrice();
        MarkAsUpdated();
    }

    /// <summary>
    /// Yetkazib berish manzilini yangilash
    /// </summary>
    public void UpdateDeliveryAddress(string newAddress)
    {
        if (DeliveryType == DeliveryType.Pickup)
            throw new InvalidOperationException("Pickup buyurtmada manzil bo'lmaydi");

        if (Status == OrderStatus.Delivered || Status == OrderStatus.Shipped)
            throw new InvalidOperationException("Jo'natilgan yoki yetkazilgan buyurtmada manzil o'zgartirib bo'lmaydi");

        DeliveryAddress = newAddress;
        MarkAsUpdated();
    }

    // ==================== CALCULATION METHODS ====================

    /// <summary>
    /// Jami narxni qayta hisoblash
    /// </summary>
    private void RecalculateTotalPrice()
    {
        TotalPrice = OrderItems.Sum(i => i.SubTotal);
        RecalculateFinalPrice();
    }

    /// <summary>
    /// Yakuniy narxni qayta hisoblash
    /// </summary>
    private void RecalculateFinalPrice()
    {
        FinalPrice = TotalPrice - DiscountApplied - CashbackUsed + DeliveryFee;

        if (FinalPrice < 0)
            FinalPrice = 0;
    }

    // ==================== VALIDATION METHODS ====================

    /// <summary>
    /// Buyurtmani validatsiya qilish
    /// </summary>
    public Result Validate()
    {
        var errors = new List<string>();

        if (CustomerId <= 0)
            errors.Add("Mijoz ID noto'g'ri");

        if (string.IsNullOrWhiteSpace(OrderNumber))
            errors.Add("Buyurtma raqami bo'sh bo'lishi mumkin emas");

        if (!OrderItems.Any())
            errors.Add("Buyurtmada kamida 1 ta item bo'lishi kerak");

        if (TotalPrice <= 0)
            errors.Add("Buyurtma summasi musbat bo'lishi kerak");

        if (DiscountApplied > TotalPrice)
            errors.Add("Chegirma summasi buyurtma summasidan ko'p bo'la olmaydi");

        if (CashbackUsed > TotalPrice - DiscountApplied)
            errors.Add("Cashback summasi buyurtma summasidan ko'p bo'la olmaydi");

        if (DeliveryType != DeliveryType.Pickup && string.IsNullOrWhiteSpace(DeliveryAddress))
            errors.Add("Yetkazib berish manzili kerak");

        // Validate all items
        foreach (var item in OrderItems)
        {
            var itemValidation = item.ValidateForOrder();
            if (!itemValidation.IsSuccess)
                errors.AddRange(itemValidation.Errors);
        }

        return errors.Any() ? Result.Failure(errors) : Result.Success();
    }

    // ==================== NOTES MANAGEMENT ====================

    /// <summary>
    /// Mijoz izohini yangilash
    /// </summary>
    public void UpdateCustomerNotes(string? notes)
    {
        CustomerNotes = notes?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Sotuvchi izohini yangilash
    /// </summary>
    public void UpdateSellerNotes(string? notes, int sellerId)
    {
        if (!SellerId.HasValue || SellerId.Value != sellerId)
            throw new InvalidOperationException("Faqat o'z buyurtmangizga izoh qo'sha olasiz");

        SellerNotes = notes?.Trim();
        UpdatedBy = sellerId;
        MarkAsUpdated();
    }

    /// <summary>
    /// Admin izohini yangilash
    /// </summary>
    public void UpdateAdminNotes(string? notes, int adminId)
    {
        AdminNotes = notes?.Trim();
        UpdatedBy = adminId;
        MarkAsUpdated();
    }

    // ==================== UTILITY METHODS ====================

    /// <summary>
    /// Buyurtma ma'lumotlarini qisqacha string formatda
    /// </summary>
    public override string ToString()
    {
        return $"Order {OrderNumber} - {Status} - {FinalPrice:N2} so'm ({ItemsCount} items)";
    }

    /// <summary>
    /// Buyurtma summary
    /// </summary>
    public string GetSummary()
    {
        var orderType = IsOnlineOrder ? "Online" : "Offline";
        var paymentStatus = IsPaid ? "To'langan" : "To'lanmagan";
        return $"{orderType} buyurtma #{OrderNumber} - {Status} - {paymentStatus} - {FinalPrice:N2} so'm";
    }

    /// <summary>
    /// Status text (O'zbekcha)
    /// </summary>
    public string GetStatusText()
    {
        return Status switch
        {
            OrderStatus.Pending => "Kutilmoqda",
            OrderStatus.Confirmed => "Tasdiqlangan",
            OrderStatus.Preparing => "Tayyorlanmoqda",
            OrderStatus.ReadyForPickup => "Olib ketishga tayyor",
            OrderStatus.Shipped => "Jo'natildi",
            OrderStatus.Delivered => "Yetkazildi",
            OrderStatus.Cancelled => "Bekor qilindi",
            _ => Status.ToString()
        };
    }

    /// <summary>
    /// Clone order (for testing)
    /// </summary>
    public Order Clone()
    {
        var clone = new Order
        {
            OrderNumber = this.OrderNumber,
            CustomerId = this.CustomerId,
            SellerId = this.SellerId,
            OrderDate = this.OrderDate,
            Status = this.Status,
            TotalPrice = this.TotalPrice,
            DiscountApplied = this.DiscountApplied,
            CashbackUsed = this.CashbackUsed,
            FinalPrice = this.FinalPrice,
            PaymentMethod = this.PaymentMethod,
            PaymentReference = this.PaymentReference,
            PaidAt = this.PaidAt,
            DeliveryType = this.DeliveryType,
            DeliveryAddress = this.DeliveryAddress,
            DeliveryFee = this.DeliveryFee,
            CustomerNotes = this.CustomerNotes,
            SellerNotes = this.SellerNotes,
            AdminNotes = this.AdminNotes
        };

        return clone;
    }
}