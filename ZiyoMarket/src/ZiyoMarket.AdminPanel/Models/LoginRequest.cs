using System.Text.Json.Serialization;

namespace ZiyoMarket.AdminPanel.Models;

public class LoginRequest
{
    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("user_type")]
    public string UserType { get; set; } = "Admin";
}
