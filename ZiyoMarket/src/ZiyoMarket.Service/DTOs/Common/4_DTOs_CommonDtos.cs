using System;
using System.Collections.Generic;

namespace ZiyoMarket.Service.DTOs.Common;

/// <summary>
/// Base pagination request
/// </summary>
public class PaginationRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    
    public int Skip => (PageNumber - 1) * PageSize;
    
    public PaginationRequest()
    {
    }
    
    public PaginationRequest(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber < 1 ? 1 : pageNumber;
        PageSize = pageSize < 1 ? 20 : pageSize > 100 ? 100 : pageSize;
    }
}

/// <summary>
/// Base filter request with pagination
/// </summary>
public class BaseFilterRequest : PaginationRequest
{
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

/// <summary>
/// API Response wrapper
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
    public int StatusCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> SuccessResponse(T data, string message = "Operation successful")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = 200
        };
    }

    public static ApiResponse<T> ErrorResponse(string message, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = new List<string> { message },
            StatusCode = statusCode
        };
    }

    public static ApiResponse<T> ErrorResponse(List<string> errors, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = errors.Count > 0 ? errors[0] : "Operation failed",
            Errors = errors,
            StatusCode = statusCode
        };
    }
}

/// <summary>
/// Lookup DTO for dropdown lists
/// </summary>
public class LookupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Date range DTO
/// </summary>
public class DateRangeDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    public DateRangeDto()
    {
        StartDate = DateTime.Today;
        EndDate = DateTime.Today;
    }
    
    public DateRangeDto(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }
    
    public int DaysCount => (EndDate - StartDate).Days + 1;
}

/// <summary>
/// File upload DTO
/// </summary>
public class FileUploadDto
{
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Statistics DTO
/// </summary>
public class StatisticsDto
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal? PreviousValue { get; set; }
    public decimal? ChangePercentage { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
}

/// <summary>
/// Chart data DTO
/// </summary>
public class ChartDataDto
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string? Color { get; set; }
}

/// <summary>
/// Key-Value pair DTO
/// </summary>
public class KeyValueDto
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Audit info DTO
/// </summary>
public class AuditInfoDto
{
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
