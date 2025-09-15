namespace ZiyoMarket.Domain.Common;

/// <summary>
/// Operation natijasini qaytarish uchun (success/error)
/// </summary>
public class Result
{
    /// <summary>
    /// Muvaffaqiyatlimi
    /// </summary>
    public bool IsSuccess { get; protected set; }

    /// <summary>
    /// Xatolikmi
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Xatolik xabari
    /// </summary>
    public string Error { get; protected set; } = string.Empty;

    /// <summary>
    /// Xatolik kodlari ro'yxati
    /// </summary>
    public List<string> Errors { get; protected set; } = new();

    protected Result(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error;
        if (!string.IsNullOrEmpty(error))
        {
            Errors.Add(error);
        }
    }

    protected Result(bool isSuccess, List<string> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors ?? new List<string>();
        Error = string.Join("; ", Errors);
    }

    /// <summary>
    /// Muvaffaqiyatli natija
    /// </summary>
    public static Result Success() => new(true, string.Empty);

    /// <summary>
    /// Xatolikli natija
    /// </summary>
    public static Result Failure(string error) => new(false, error);

    /// <summary>
    /// Ko'p xatolikli natija
    /// </summary>
    public static Result Failure(List<string> errors) => new(false, errors);

    /// <summary>
    /// Xatolik qo'shish
    /// </summary>
    public Result AddError(string error)
    {
        Errors.Add(error);
        Error = string.Join("; ", Errors);
        IsSuccess = false;
        return this;
    }
}

/// <summary>
/// Ma'lumot bilan birga natija qaytarish
/// </summary>
public class Result<T> : Result
{
    /// <summary>
    /// Qaytariladigan ma'lumot
    /// </summary>
    public T? Value { get; protected set; }

    protected Result(T value, bool isSuccess, string error) : base(isSuccess, error)
    {
        Value = value;
    }

    protected Result(T value, bool isSuccess, List<string> errors) : base(isSuccess, errors)
    {
        Value = value;
    }

    /// <summary>
    /// Muvaffaqiyatli natija ma'lumot bilan
    /// </summary>
    public static Result<T> Success(T value) => new(value, true, string.Empty);

    /// <summary>
    /// Xatolikli natija
    /// </summary>
    public static new Result<T> Failure(string error) => new(default, false, error);

    /// <summary>
    /// Ko'p xatolikli natija
    /// </summary>
    public static new Result<T> Failure(List<string> errors) => new(default, false, errors);

    /// <summary>
    /// Result'dan Result<T> ga o'tkazish
    /// </summary>
    public static Result<T> Failure(Result result) => new(default, false, result.Errors);
}