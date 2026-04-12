using System.Text.Json.Serialization;

namespace ZiyoMarket.AdminPanel.Models;

public class CustomerModel
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("first_name")] public string FirstName { get; set; } = string.Empty;
    [JsonPropertyName("last_name")] public string LastName { get; set; } = string.Empty;
    [JsonPropertyName("phone")] public string Phone { get; set; } = string.Empty;
    [JsonPropertyName("address")] public string? Address { get; set; }
    [JsonPropertyName("cashback_balance")] public decimal CashbackBalance { get; set; }
    [JsonPropertyName("is_active")] public bool IsActive { get; set; }
    [JsonPropertyName("total_orders")] public int TotalOrders { get; set; }
    [JsonPropertyName("orders_count")] public int OrdersCount { get; set; }
    [JsonPropertyName("total_spent")] public decimal TotalSpent { get; set; }
    [JsonPropertyName("created_at")] public string? CreatedAt { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
    public string StatusText => IsActive ? "Faol" : "Faol emas";
    public int EffectiveOrders => TotalOrders > 0 ? TotalOrders : OrdersCount;
    public string Initials
    {
        get
        {
            var f = string.IsNullOrEmpty(FirstName) ? "?" : FirstName[0].ToString();
            var l = string.IsNullOrEmpty(LastName) ? "" : LastName[0].ToString();
            return (f + l).ToUpper();
        }
    }
    public string FormattedCashback => $"{CashbackBalance:N0} so'm";
    public string FormattedTotalSpent => $"{TotalSpent:N0} so'm";
}

public class CustomerDetailModel
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("first_name")] public string FirstName { get; set; } = string.Empty;
    [JsonPropertyName("last_name")] public string LastName { get; set; } = string.Empty;
    [JsonPropertyName("phone")] public string Phone { get; set; } = string.Empty;
    [JsonPropertyName("address")] public string? Address { get; set; }
    [JsonPropertyName("cashback_balance")] public decimal CashbackBalance { get; set; }
    [JsonPropertyName("is_active")] public bool IsActive { get; set; }
    [JsonPropertyName("total_orders")] public int TotalOrders { get; set; }
    [JsonPropertyName("total_spent")] public decimal TotalSpent { get; set; }
    [JsonPropertyName("total_cashback_earned")] public decimal TotalCashbackEarned { get; set; }
    [JsonPropertyName("total_cashback_used")] public decimal TotalCashbackUsed { get; set; }
    [JsonPropertyName("last_order_date")] public string? LastOrderDate { get; set; }
    [JsonPropertyName("created_at")] public string? CreatedAt { get; set; }
    [JsonPropertyName("updated_at")] public string? UpdatedAt { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Initials
    {
        get
        {
            var f = string.IsNullOrEmpty(FirstName) ? "?" : FirstName[0].ToString();
            var l = string.IsNullOrEmpty(LastName) ? "" : LastName[0].ToString();
            return (f + l).ToUpper();
        }
    }
    public string FormattedCashback => $"{CashbackBalance:N0} so'm";
    public string FormattedTotalSpent => $"{TotalSpent:N0} so'm";
    public string FormattedCashbackEarned => $"{TotalCashbackEarned:N0} so'm";
    public string FormattedCashbackUsed => $"{TotalCashbackUsed:N0} so'm";
}

public class CustomerListDataWrapper
{
    [JsonPropertyName("items")] public List<CustomerModel> Items { get; set; } = new();
    [JsonPropertyName("total_count")] public int TotalCount { get; set; }
    [JsonPropertyName("page")] public int Page { get; set; }
    [JsonPropertyName("page_size")] public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)(PageSize > 0 ? PageSize : 20));
}

public class CustomerListApiResponse
{
    [JsonPropertyName("success")] public bool Success { get; set; }
    [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;
    [JsonPropertyName("data")] public CustomerListDataWrapper? Data { get; set; }
}

public class CustomerDetailApiResponse
{
    [JsonPropertyName("success")] public bool Success { get; set; }
    [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;
    [JsonPropertyName("data")] public CustomerDetailModel? Data { get; set; }
}
