namespace ZiyoMarket.Service.DTOs.Reports;

/// <summary>Dashboard'dagi "So'nggi buyurtmalar" qatori (admin panel OrderDto nomlari bilan)</summary>
public class DashboardRecentOrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>Dashboard'dagi "Eng ko'p sotilgan" qatori</summary>
public class DashboardTopProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int SalesCount { get; set; }
    public string? ImageUrl { get; set; }
}

/// <summary>Oylik grafik nuqtasi, masalan ("Noy", 1 250 000)</summary>
public class DashboardMonthlyPointDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Value { get; set; }
}
