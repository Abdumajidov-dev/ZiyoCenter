using System.Text.Json.Serialization;

namespace ZiyoMarket.AdminPanel.Models;

// API qaytaradigan to'liq wrapper: { success, message, data }
public class LoginApiResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public LoginResponse? Data { get; set; }
}

// API data qismi: { access_token, expires_at, user }
public class LoginResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [JsonPropertyName("user")]
    public UserProfile? User { get; set; }
}

public class UserProfile
{
    [JsonPropertyName("user_id")]
    public int UserId { get; set; }

    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("last_name")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("user_type")]
    public string UserType { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;
}
