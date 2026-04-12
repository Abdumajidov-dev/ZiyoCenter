using System.Text.Json;
using System.Text.Json.Serialization;

namespace ZiyoMarket.AdminPanel.Models;

public class NotificationModel
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;
    [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
    [JsonPropertyName("priority")] public string Priority { get; set; } = "Normal";
    [JsonPropertyName("data")] public string? Data { get; set; }
    [JsonPropertyName("action_url")] public string? ActionUrl { get; set; }
    [JsonPropertyName("is_read")] public bool IsRead { get; set; }
    [JsonPropertyName("created_at")] public string? CreatedAt { get; set; }

    public bool IsPaymentReceipt => Type == "PaymentReceiptUploaded";

    public PaymentReceiptData? ParsedData
    {
        get
        {
            if (string.IsNullOrEmpty(Data)) return null;
            try { return JsonSerializer.Deserialize<PaymentReceiptData>(Data); }
            catch { return null; }
        }
    }
}

public class PaymentReceiptData
{
    [JsonPropertyName("order_id")] public int OrderId { get; set; }
    [JsonPropertyName("customer_id")] public int CustomerId { get; set; }
    [JsonPropertyName("customer_name")] public string CustomerName { get; set; } = string.Empty;
    [JsonPropertyName("order_number")] public string OrderNumber { get; set; } = string.Empty;
    [JsonPropertyName("final_price")] public decimal FinalPrice { get; set; }
    [JsonPropertyName("receipt_url")] public string? ReceiptUrl { get; set; }
    public string FormattedPrice => $"{FinalPrice:N0} so'm";
}

public class NotificationListResponse
{
    [JsonPropertyName("success")] public bool Success { get; set; }
    [JsonPropertyName("data")] public List<NotificationModel>? Data { get; set; }
}

// Order detail for admin panel
public class AdminOrderModel
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("order_number")] public string OrderNumber { get; set; } = string.Empty;
    [JsonPropertyName("customer_name")] public string CustomerName { get; set; } = string.Empty;
    [JsonPropertyName("customer_phone")] public string CustomerPhone { get; set; } = string.Empty;
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
    [JsonPropertyName("final_price")] public decimal FinalPrice { get; set; }
    [JsonPropertyName("payment_method")] public string PaymentMethod { get; set; } = string.Empty;
    [JsonPropertyName("delivery_type")] public string DeliveryType { get; set; } = string.Empty;
    [JsonPropertyName("delivery_address")] public string? DeliveryAddress { get; set; }
    [JsonPropertyName("customer_notes")] public string? CustomerNotes { get; set; }
    [JsonPropertyName("admin_notes")] public string? AdminNotes { get; set; }
    [JsonPropertyName("payment_receipt_url")] public string? PaymentReceiptUrl { get; set; }
    [JsonPropertyName("created_at")] public string? CreatedAt { get; set; }
    [JsonPropertyName("order_items")] public List<AdminOrderItemModel> OrderItems { get; set; } = new();
    public string FormattedPrice => $"{FinalPrice:N0} so'm";
    public bool HasReceipt => !string.IsNullOrEmpty(PaymentReceiptUrl);
    public bool IsImage => HasReceipt &&
        (PaymentReceiptUrl!.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
         PaymentReceiptUrl.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
         PaymentReceiptUrl.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
         PaymentReceiptUrl.EndsWith(".webp", StringComparison.OrdinalIgnoreCase));
}

public class AdminOrderItemModel
{
    [JsonPropertyName("product_name")] public string ProductName { get; set; } = string.Empty;
    [JsonPropertyName("quantity")] public int Quantity { get; set; }
    [JsonPropertyName("unit_price")] public decimal UnitPrice { get; set; }
    [JsonPropertyName("total_price")] public decimal TotalPrice { get; set; }
    public string FormattedPrice => $"{TotalPrice:N0} so'm";
}

public class AdminOrderDetailResponse
{
    [JsonPropertyName("success")] public bool Success { get; set; }
    [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;
    [JsonPropertyName("data")] public AdminOrderModel? Data { get; set; }
}
