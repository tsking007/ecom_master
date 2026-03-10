namespace EcommerceApp.Domain.Common;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }
    public string ErrorCode { get; }

    protected Result(bool isSuccess, string error, string errorCode = "")
    {
        if (isSuccess && !string.IsNullOrEmpty(error))
            throw new InvalidOperationException("A successful result cannot carry an error message.");

        if (!isSuccess && string.IsNullOrEmpty(error))
            throw new InvalidOperationException("A failure result must have an error message.");

        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
    }

    // ── Non-generic factory methods ───────────────────────────────────────────

    public static Result Success()
        => new(true, string.Empty);

    public static Result Failure(string error, string errorCode = "GENERAL_ERROR")
        => new(false, error, errorCode);

    // ── Generic factory methods (shortcuts so callers don't need Result<T>) ──

    public static Result<T> Success<T>(T value)
        => new(value, true, string.Empty);

    public static Result<T> Failure<T>(string error, string errorCode = "GENERAL_ERROR")
        => new(default, false, error, errorCode);
}

public class Result<T> : Result
{
    private readonly T? _value;

    protected internal Result(T? value, bool isSuccess, string error, string errorCode = "")
        : base(isSuccess, error, errorCode)
    {
        _value = value;
    }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException(
            $"Cannot access the Value of a failed result. Error: {Error}");

    // Allows: Result<MyDto> result = myDto;
    public static implicit operator Result<T>(T value) => Success(value);
}