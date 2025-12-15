namespace ZiyoMarket.AdminPanel.Models;

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int? SellerId { get; set; }
    public string? SellerName { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public decimal DiscountApplied { get; set; }
    public decimal CashbackUsed { get; set; }
    public decimal FinalPrice { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? PaymentReference { get; set; }
    public DateTime? PaidAt { get; set; }
    public string DeliveryType { get; set; } = string.Empty;
    public string? DeliveryAddress { get; set; }
    public decimal DeliveryFee { get; set; }
    public string? CustomerNotes { get; set; }
    public string? SellerNotes { get; set; }
    public string? AdminNotes { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal DiscountAmount { get; set; }
}

public class UpdateOrderStatusDto
{
    public int OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? AdminNotes { get; set; }
}
