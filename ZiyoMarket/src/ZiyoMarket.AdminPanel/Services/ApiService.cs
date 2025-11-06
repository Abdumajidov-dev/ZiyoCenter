using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ZiyoMarket.AdminPanel.Models;

namespace ZiyoMarket.AdminPanel.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private string? _authToken;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://ziyocenter.onrender.com/api/");
    }

    public void SetAuthToken(string token)
    {
        _authToken = token;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public string? GetAuthToken() => _authToken;

    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("auth/login", request);

            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                return new ApiResponse<LoginResponse>
                {
                    Success = true,
                    Message = "Login successful",
                    Data = loginResponse
                };
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return new ApiResponse<LoginResponse>
            {
                Success = false,
                Message = $"Login failed: {errorContent}"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<LoginResponse>
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
    {
        try
        {
            var response = await _httpClient.GetAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<T>();
                return new ApiResponse<T>
                {
                    Success = true,
                    Data = data
                };
            }

            return new ApiResponse<T>
            {
                Success = false,
                Message = $"Request failed with status {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, data);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<T>();
                return new ApiResponse<T>
                {
                    Success = true,
                    Data = result
                };
            }

            return new ApiResponse<T>
            {
                Success = false,
                Message = $"Request failed with status {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync(endpoint, data);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<T>();
                return new ApiResponse<T>
                {
                    Success = true,
                    Data = result
                };
            }

            return new ApiResponse<T>
            {
                Success = false,
                Message = $"Request failed with status {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<T>> DeleteAsync<T>(string endpoint)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<T>();
                return new ApiResponse<T>
                {
                    Success = true,
                    Data = result
                };
            }

            return new ApiResponse<T>
            {
                Success = false,
                Message = $"Request failed with status {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }
}
