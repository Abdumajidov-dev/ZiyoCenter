using System.Text.Json.Serialization;

namespace ZiyoMarket.Service.DTOs.Payments.Click;

public class ClickRequestDto
{
    [JsonPropertyName("click_trans_id")]
    public long ClickTransId { get; set; }

    [JsonPropertyName("service_id")]
    public int ServiceId { get; set; }

    [JsonPropertyName("click_paydoc_id")]
    public long ClickPaydocId { get; set; }

    [JsonPropertyName("merchant_trans_id")]
    public string MerchantTransId { get; set; } = string.Empty;

    [JsonPropertyName("merchant_prepare_id")]
    public int? MerchantPrepareId { get; set; }

    [JsonPropertyName("amount")]
    public float Amount { get; set; }

    [JsonPropertyName("action")]
    public int Action { get; set; }

    [JsonPropertyName("error")]
    public int Error { get; set; }

    [JsonPropertyName("error_note")]
    public string ErrorNote { get; set; } = string.Empty;

    [JsonPropertyName("sign_time")]
    public string SignTime { get; set; } = string.Empty;

    [JsonPropertyName("sign_string")]
    public string SignString { get; set; } = string.Empty;
}

public class ClickResponseDto
{
    [JsonPropertyName("click_trans_id")]
    public long ClickTransId { get; set; }

    [JsonPropertyName("merchant_trans_id")]
    public string MerchantTransId { get; set; } = string.Empty;

    [JsonPropertyName("merchant_prepare_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MerchantPrepareId { get; set; }

    [JsonPropertyName("merchant_confirm_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MerchantConfirmId { get; set; }

    [JsonPropertyName("error")]
    public int Error { get; set; }

    [JsonPropertyName("error_note")]
    public string ErrorNote { get; set; } = string.Empty;
}

public class ClickSettings
{
    public int ServiceId { get; set; }
    public string SecretKey { get; set; } = string.Empty;
    public int MerchantUserId { get; set; }
}
