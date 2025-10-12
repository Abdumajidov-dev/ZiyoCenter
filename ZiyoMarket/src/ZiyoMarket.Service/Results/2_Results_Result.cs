using System.Collections.Generic;
using System.Linq;

namespace ZiyoMarket.Service.Results;

/// <summary>
/// Result pattern for operation results without data
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public string Message { get; }
    public List<string> Errors { get; }
    public int StatusCode { get; }

    protected Result(bool isSuccess, string message, List<string> errors, int statusCode)
    {
        IsSuccess = isSuccess;
        Message = message;
        Errors = errors ?? new List<string>();
        StatusCode = statusCode;
    }

    public static Result Success(string message = "Operation successful", int statusCode = 200)
        => new Result(true, message, new List<string>(), statusCode);

    public static Result Failure(string error, int statusCode = 400)
        => new Result(false, error, new List<string> { error }, statusCode);

    public static Result Failure(List<string> errors, int statusCode = 400)
        => new Result(false, errors.FirstOrDefault() ?? "Operation failed", errors, statusCode);

    public static Result NotFound(string message = "Resource not found")
        => new Result(false, message, new List<string> { message }, 404);

    public static Result Unauthorized(string message = "Unauthorized access")
        => new Result(false, message, new List<string> { message }, 401);

    public static Result Forbidden(string message = "Access denied")
        => new Result(false, message, new List<string> { message }, 403);

    public static Result BadRequest(string message = "Bad request")
        => new Result(false, message, new List<string> { message }, 400);

    public static Result Conflict(string message = "Conflict detected")
        => new Result(false, message, new List<string> { message }, 409);

    public static Result InternalError(string message = "Internal server error")
        => new Result(false, message, new List<string> { message }, 500);
}

/// <summary>
/// Result pattern for operation results with data
/// </summary>
/// <typeparam name="T">Type of data</typeparam>
public class Result<T> : Result
{
    public T? Data { get; }

    private Result(bool isSuccess, T? data, string message, List<string> errors, int statusCode)
        : base(isSuccess, message, errors, statusCode)
    {
        Data = data;
    }

    public static Result<T> Success(T data, string message = "Operation successful", int statusCode = 200)
        => new Result<T>(true, data, message, new List<string>(), statusCode);

    public static new Result<T> Failure(string error, int statusCode = 400)
        => new Result<T>(false, default, error, new List<string> { error }, statusCode);

    public static new Result<T> Failure(List<string> errors, int statusCode = 400)
        => new Result<T>(false, default, errors.FirstOrDefault() ?? "Operation failed", errors, statusCode);

    public static new Result<T> NotFound(string message = "Resource not found")
        => new Result<T>(false, default, message, new List<string> { message }, 404);

    public static new Result<T> Unauthorized(string message = "Unauthorized access")
        => new Result<T>(false, default, message, new List<string> { message }, 401);

    public static new Result<T> Forbidden(string message = "Access denied")
        => new Result<T>(false, default, message, new List<string> { message }, 403);

    public static new Result<T> BadRequest(string message = "Bad request")
        => new Result<T>(false, default, message, new List<string> { message }, 400);

    public static new Result<T> Conflict(string message = "Conflict detected")
        => new Result<T>(false, default, message, new List<string> { message }, 409);

    public static new Result<T> InternalError(string message = "Internal server error")
        => new Result<T>(false, default, message, new List<string> { message }, 500);
}

/// <summary>
/// Pagination response wrapper
/// </summary>
public class PaginationResponse<T>
{
    public List<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)System.Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PaginationResponse(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
