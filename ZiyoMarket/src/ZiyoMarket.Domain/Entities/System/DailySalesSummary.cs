using System.Text.Json;
using ZiyoMarket.Domain.Common;

namespace ZiyoMarket.Domain.Entities.System;

/// <summary>
/// Kunlik sotuvlar xulosasi entity'si
/// </summary>
public class DailySalesSummary : BaseEntity
{
    /// <summary>
    /// Sotuv sanasi
    /// </summary>
    public DateOnly SaleDate { get; set; }

    /// <summary>
    /// Jami buyurtmalar soni
    /// </summary>
    public int TotalOrders { get; set; } = 0;

    /// <summary>
    /// Jami daromad
    /// </summary>
    public decimal TotalRevenue { get; set; } = 0;

    /// <summary>
    /// Jami chegirma berilgan
    /// </summary>
    public decimal TotalDiscount { get; set; } = 0;

    /// <summary>
    /// Jami cashback ishlatilgan
    /// </summary>
    public decimal CashbackUsed { get; set; } = 0;

    /// <summary>
    /// Jami cashback berilgan
    /// </summary>
    public decimal CashbackGiven { get; set; } = 0;

    /// <summary>
    /// Online buyurtmalar soni
    /// </summary>
    public int OnlineOrders { get; set; } = 0;

    /// <summary>
    /// Offline buyurtmalar soni (sotuvchi orqali)
    /// </summary>
    public int OfflineOrders { get; set; } = 0;

    /// <summary>
    /// Bekor qilingan buyurtmalar soni
    /// </summary>
    public int CancelledOrders { get; set; } = 0;

    /// <summary>
    /// Yetkazib berilgan buyurtmalar soni
    /// </summary>
    public int DeliveredOrders { get; set; } = 0;

    /// <summary>
    /// Yangi mijozlar soni
    /// </summary>
    public int NewCustomers { get; set; } = 0;

    /// <summary>
    /// Qaytgan mijozlar soni
    /// </summary>
    public int ReturningCustomers { get; set; } = 0;

    /// <summary>
    /// Jami sotilgan item'lar soni
    /// </summary>
    public int TotalItemsSold { get; set; } = 0;

    /// <summary>
    /// O'rtacha buyurtma qiymati
    /// </summary>
    public decimal AverageOrderValue { get; set; } = 0;

    /// <summary>
    /// Naqd to'lovlar summasi
    /// </summary>
    public decimal CashPayments { get; set; } = 0;

    /// <summary>
    /// Karta to'lovlar summasi
    /// </summary>
    public decimal CardPayments { get; set; } = 0;

    /// <summary>
    /// Eng ko'p sotilgan mahsulot ID
    /// </summary>
    public int? TopSellingProductId { get; set; }

    /// <summary>
    /// Eng ko'p sotilgan mahsulot miqdori
    /// </summary>
    public int TopSellingProductQuantity { get; set; } = 0;

    /// <summary>
    /// Eng faol sotuvchi ID
    /// </summary>
    public int? TopSellerId { get; set; }

    /// <summary>
    /// Eng faol sotuvchi sotuv summasi
    /// </summary>
    public decimal TopSellerRevenue { get; set; } = 0;

    /// <summary>
    /// Eng faol mijoz ID
    /// </summary>
    public int? TopCustomerId { get; set; }

    /// <summary>
    /// Eng faol mijoz xarid summasi
    /// </summary>
    public decimal TopCustomerSpending { get; set; } = 0;

    /// <summary>
    /// Qo'shimcha metrikalar (JSON)
    /// </summary>
    public string? AdditionalMetrics { get; set; }

    // Business Methods

    /// <summary>
    /// Bugungi kunmi
    /// </summary>
    public bool IsToday => SaleDate == DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// Kechami
    /// </summary>
    public bool IsYesterday => SaleDate == DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

    /// <summary>
    /// Shu haftadami
    /// </summary>
    public bool IsThisWeek
    {
        get
        {
            var today = DateTime.UtcNow.Date;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);
            var saleDateTime = SaleDate.ToDateTime(TimeOnly.MinValue);

            return saleDateTime >= startOfWeek && saleDateTime < endOfWeek;
        }
    }

    /// <summary>
    /// Shu oyinmi
    /// </summary>
    public bool IsThisMonth
    {
        get
        {
            var today = DateTime.UtcNow;
            return SaleDate.Year == today.Year && SaleDate.Month == today.Month;
        }
    }

    /// <summary>
    /// Muvaffaqiyatli buyurtmalar foizi
    /// </summary>
    public decimal SuccessRate
    {
        get
        {
            if (TotalOrders == 0) return 100;
            return ((decimal)(TotalOrders - CancelledOrders) / TotalOrders) * 100;
        }
    }

    /// <summary>
    /// Conversion rate (yangi mijozlar / jami mijozlar)
    /// </summary>
    public decimal ConversionRate
    {
        get
        {
            var totalCustomers = NewCustomers + ReturningCustomers;
            if (totalCustomers == 0) return 0;
            return ((decimal)NewCustomers / totalCustomers) * 100;
        }
    }

    /// <summary>
    /// Online vs Offline foizi
    /// </summary>
    public decimal OnlinePercentage
    {
        get
        {
            if (TotalOrders == 0) return 0;
            return ((decimal)OnlineOrders / TotalOrders) * 100;
        }
    }

    /// <summary>
    /// Discount rate (chegirma foizi)
    /// </summary>
    public decimal DiscountRate
    {
        get
        {
            if (TotalRevenue == 0) return 0;
            return (TotalDiscount / (TotalRevenue + TotalDiscount)) * 100;
        }
    }

    /// <summary>
    /// Cashback rate (cashback berilgan foizi)
    /// </summary>
    public decimal CashbackRate
    {
        get
        {
            if (TotalRevenue == 0) return 0;
            return (CashbackGiven / TotalRevenue) * 100;
        }
    }

    /// <summary>
    /// Kunlik hisobotni yaratish/yangilash
    /// </summary>
    public static DailySalesSummary CreateOrUpdate(DateOnly saleDate)
    {
        return new DailySalesSummary
        {
            SaleDate = saleDate,
            CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
        };
    }

    /// <summary>
    /// Buyurtma qo'shish
    /// </summary>
    public void AddOrder(decimal orderValue, bool isOnline, bool isCancelled, bool isDelivered,
                        bool isNewCustomer, decimal discountApplied, decimal cashbackUsed,
                        decimal cashbackGiven, bool isCashPayment, int itemCount)
    {
        TotalOrders++;

        if (!isCancelled)
        {
            TotalRevenue += orderValue;
            TotalDiscount += discountApplied;
            CashbackUsed += cashbackUsed;
            CashbackGiven += cashbackGiven;
            TotalItemsSold += itemCount;

            if (isCashPayment)
                CashPayments += orderValue;
            else
                CardPayments += orderValue;
        }
        else
        {
            CancelledOrders++;
        }

        if (isOnline)
            OnlineOrders++;
        else
            OfflineOrders++;

        if (isDelivered)
            DeliveredOrders++;

        if (isNewCustomer)
            NewCustomers++;
        else
            ReturningCustomers++;

        // O'rtacha buyurtma qiymatini qayta hisoblash
        RecalculateAverageOrderValue();
        MarkAsUpdated();
    }

    /// <summary>
    /// Mahsulot sotuv ma'lumotini yangilash
    /// </summary>
    public void UpdateTopSellingProduct(int productId, int quantitySold)
    {
        if (quantitySold > TopSellingProductQuantity)
        {
            TopSellingProductId = productId;
            TopSellingProductQuantity = quantitySold;
            MarkAsUpdated();
        }
    }

    /// <summary>
    /// Eng faol sotuvchi ma'lumotini yangilash
    /// </summary>
    public void UpdateTopSeller(int sellerId, decimal revenue)
    {
        if (revenue > TopSellerRevenue)
        {
            TopSellerId = sellerId;
            TopSellerRevenue = revenue;
            MarkAsUpdated();
        }
    }

    /// <summary>
    /// Eng faol mijoz ma'lumotini yangilash
    /// </summary>
    public void UpdateTopCustomer(int customerId, decimal spending)
    {
        if (spending > TopCustomerSpending)
        {
            TopCustomerId = customerId;
            TopCustomerSpending = spending;
            MarkAsUpdated();
        }
    }

    /// <summary>
    /// O'rtacha buyurtma qiymatini qayta hisoblash
    /// </summary>
    public void RecalculateAverageOrderValue()
    {
        var successfulOrders = TotalOrders - CancelledOrders;
        AverageOrderValue = successfulOrders > 0 ? TotalRevenue / successfulOrders : 0;
    }

    /// <summary>
    /// Qo'shimcha metrikalarni o'rnatish
    /// </summary>
    public void SetAdditionalMetrics<T>(T metrics) where T : class
    {
        AdditionalMetrics = System.Text.Json.JsonSerializer.Serialize(metrics);
        MarkAsUpdated();
    }

    /// <summary>
    /// Qo'shimcha metrikalarni olish
    /// </summary>
    public T? GetAdditionalMetrics<T>() where T : class
    {
        if (string.IsNullOrEmpty(AdditionalMetrics))
            return null;

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(AdditionalMetrics);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Hisobotni nolga tiklash
    /// </summary>
    public void Reset()
    {
        TotalOrders = 0;
        TotalRevenue = 0;
        TotalDiscount = 0;
        CashbackUsed = 0;
        CashbackGiven = 0;
        OnlineOrders = 0;
        OfflineOrders = 0;
        CancelledOrders = 0;
        DeliveredOrders = 0;
        NewCustomers = 0;
        ReturningCustomers = 0;
        TotalItemsSold = 0;
        AverageOrderValue = 0;
        CashPayments = 0;
        CardPayments = 0;
        TopSellingProductId = null;
        TopSellingProductQuantity = 0;
        TopSellerId = null;
        TopSellerRevenue = 0;
        TopCustomerId = null;
        TopCustomerSpending = 0;
        AdditionalMetrics = null;
        MarkAsUpdated();
    }

    /// <summary>
    /// Boshqa kun bilan solishtirish
    /// </summary>
    public ComparisonResult CompareTo(DailySalesSummary other)
    {
        return new ComparisonResult
        {
            RevenueChange = CalculatePercentageChange(other.TotalRevenue, TotalRevenue),
            OrdersChange = CalculatePercentageChange(other.TotalOrders, TotalOrders),
            AverageOrderValueChange = CalculatePercentageChange(other.AverageOrderValue, AverageOrderValue),
            ConversionRateChange = other.ConversionRate - ConversionRate,
            SuccessRateChange = other.SuccessRate - SuccessRate
        };
    }

    /// <summary>
    /// Foiz o'zgarishini hisoblash
    /// </summary>
    private static decimal CalculatePercentageChange(decimal oldValue, decimal newValue)
    {
        if (oldValue == 0) return newValue > 0 ? 100 : 0;
        return ((newValue - oldValue) / oldValue) * 100;
    }

    /// <summary>
    /// Hisobot ma'lumotlarini qisqa formatda
    /// </summary>
    public string GetSummaryText()
    {
        return $"{SaleDate:dd.MM.yyyy}: {TotalOrders} buyurtma, {TotalRevenue:C} daromad, {SuccessRate:F1}% muvaffaqiyat";
    }

    /// <summary>
    /// Performance darajasini baholash
    /// </summary>
    public string GetPerformanceLevel()
    {
        // Bu logika business requirements asosida o'zgartirilishi mumkin
        if (TotalRevenue >= 10000000) return "Excellent"; // 10M so'm+
        if (TotalRevenue >= 5000000) return "Good";       // 5M so'm+
        if (TotalRevenue >= 2000000) return "Average";    // 2M so'm+
        if (TotalRevenue >= 500000) return "Below";       // 500K so'm+
        return "Poor";
    }

    /// <summary>
    /// Key Performance Indicators (KPI)
    /// </summary>
    public Dictionary<string, object> GetKPIs()
    {
        return new Dictionary<string, object>
        {
            ["TotalRevenue"] = TotalRevenue,
            ["TotalOrders"] = TotalOrders,
            ["AverageOrderValue"] = AverageOrderValue,
            ["SuccessRate"] = SuccessRate,
            ["ConversionRate"] = ConversionRate,
            ["OnlinePercentage"] = OnlinePercentage,
            ["DiscountRate"] = DiscountRate,
            ["CashbackRate"] = CashbackRate,
            ["ItemsPerOrder"] = TotalOrders > 0 ? (decimal)TotalItemsSold / TotalOrders : 0,
            ["CustomerRetentionRate"] = ConversionRate > 0 ? 100 - ConversionRate : 0
        };
    }

    /// <summary>
    /// Hisobot validatsiya
    /// </summary>
    public Result Validate()
    {
        var errors = new List<string>();

        if (SaleDate > DateOnly.FromDateTime(DateTime.UtcNow))
            errors.Add("Sotuv sanasi kelajakda bo'la olmaydi");

        if (TotalOrders < 0)
            errors.Add("Jami buyurtmalar soni manfiy bo'la olmaydi");

        if (TotalRevenue < 0)
            errors.Add("Jami daromad manfiy bo'la olmaydi");

        if (OnlineOrders + OfflineOrders != TotalOrders)
            errors.Add("Online va Offline buyurtmalar yig'indisi jami buyurtmalarga teng bo'lishi kerak");

        if (CancelledOrders > TotalOrders)
            errors.Add("Bekor qilingan buyurtmalar jami buyurtmalardan ko'p bo'la olmaydi");

        if (NewCustomers + ReturningCustomers > 0 && TotalOrders == 0)
            errors.Add("Mijozlar bor lekin buyurtmalar yo'q");

        if (TotalDiscount < 0 || CashbackUsed < 0 || CashbackGiven < 0)
            errors.Add("Chegirma va cashback qiymatlari manfiy bo'la olmaydi");

        return errors.Any() ? Result.Failure(errors) : Result.Success();
    }

    /// <summary>
    /// Hisobot ma'lumotlarini to'liq formatda
    /// </summary>
    public override string ToString()
    {
        return $"Sales Summary {SaleDate:yyyy-MM-dd}: {TotalOrders} orders, {TotalRevenue:C} revenue";
    }

    /// <summary>
    /// Solishtirish natijasi
    /// </summary>
    public class ComparisonResult
    {
        public decimal RevenueChange { get; set; }
        public decimal OrdersChange { get; set; }
        public decimal AverageOrderValueChange { get; set; }
        public decimal ConversionRateChange { get; set; }
        public decimal SuccessRateChange { get; set; }

        public bool IsImprovement => RevenueChange > 0 && OrdersChange > 0;
        public bool IsDecline => RevenueChange < 0 && OrdersChange < 0;
    }
}