namespace DataBuilder.Cli.Common;

/// <summary>
/// Represents the result of an operation that returns a value.
/// Provides a type-safe alternative to exceptions for expected failure cases.
/// </summary>
/// <typeparam name="T">The type of the value returned on success.</typeparam>
public record Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public string? Error { get; init; }
    public Exception? Exception { get; init; }

    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static Result<T> Failure(string error) => new() { IsSuccess = false, Error = error };
    public static Result<T> Failure(Exception ex) => new() { IsSuccess = false, Error = ex.Message, Exception = ex };

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
        => IsSuccess ? onSuccess(Value!) : onFailure(Error!);
}

/// <summary>
/// Represents the result of an operation that does not return a value.
/// Provides a type-safe alternative to exceptions for expected failure cases.
/// </summary>
public record Result
{
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }

    public static Result Success() => new() { IsSuccess = true };
    public static Result Failure(string error) => new() { IsSuccess = false, Error = error };
}
