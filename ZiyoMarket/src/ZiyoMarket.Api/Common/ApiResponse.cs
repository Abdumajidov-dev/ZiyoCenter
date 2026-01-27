namespace ZiyoMarket.Api.Common;

/// <summary>
/// Unified API response wrapper for all endpoints
/// </summary>
/// <typeparam name="T">Data type</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Success status
    /// </summary>
    public bool Status { get; set; }

    /// <summary>
    /// Response message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Response data
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Create success response
    /// </summary>
    public static ApiResponse<T> Success(T? data = default, string message = "Success")
    {
        return new ApiResponse<T>
        {
            Status = true,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// Create failure response
    /// </summary>
    public static ApiResponse<T> Failure(string message, T? data = default)
    {
        return new ApiResponse<T>
        {
            Status = false,
            Message = message,
            Data = data
        };
    }
}

/// <summary>
/// Non-generic API response for endpoints without data
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    /// <summary>
    /// Create success response without data
    /// </summary>
    public static ApiResponse Success(string message = "Success")
    {
        return new ApiResponse
        {
            Status = true,
            Message = message,
            Data = null
        };
    }

    /// <summary>
    /// Create failure response without data
    /// </summary>
    public static new ApiResponse Failure(string message)
    {
        return new ApiResponse
        {
            Status = false,
            Message = message,
            Data = null
        };
    }
}
