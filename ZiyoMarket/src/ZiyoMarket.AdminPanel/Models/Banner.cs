using System.Text.Json.Serialization;

namespace ZiyoMarket.AdminPanel.Models;

public class BannerModel
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("image_url")] public string? ImageUrl { get; set; }
    [JsonPropertyName("external_url")] public string? ExternalUrl { get; set; }
    [JsonPropertyName("is_published")] public bool IsPublished { get; set; }
    [JsonPropertyName("sort_order")] public int SortOrder { get; set; }
    [JsonPropertyName("target_audience")] public string TargetAudience { get; set; } = "All";
    [JsonPropertyName("start_date")] public DateTime? StartDate { get; set; }
    [JsonPropertyName("end_date")] public DateTime? EndDate { get; set; }
    [JsonPropertyName("is_active")] public bool IsActive { get; set; }
    [JsonPropertyName("is_expired")] public bool IsExpired { get; set; }
    [JsonPropertyName("created_at")] public DateTime CreatedAt { get; set; }

    public string StatusText => !IsPublished ? "Nofaol" : IsExpired ? "Muddati tugagan" : IsActive ? "Faol" : "Rejalashtirilgan";
}

public class SaveBannerDto
{
    [JsonPropertyName("type")] public string Type { get; set; } = "Banner";
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("image_url")] public string? ImageUrl { get; set; }
    [JsonPropertyName("external_url")] public string? ExternalUrl { get; set; }
    [JsonPropertyName("is_published")] public bool IsPublished { get; set; } = true;
    [JsonPropertyName("sort_order")] public int SortOrder { get; set; }
    [JsonPropertyName("target_audience")] public string TargetAudience { get; set; } = "All";
    [JsonPropertyName("start_date")] public DateTime? StartDate { get; set; }
    [JsonPropertyName("end_date")] public DateTime? EndDate { get; set; }
}

public class BannerListApiResponse
{
    [JsonPropertyName("success")] public bool Success { get; set; }
    [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;
    [JsonPropertyName("data")] public List<BannerModel>? Data { get; set; }
}

public class BannerDetailApiResponse
{
    [JsonPropertyName("success")] public bool Success { get; set; }
    [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;
    [JsonPropertyName("data")] public BannerModel? Data { get; set; }
}
