namespace BlazorServerFunctions.Sample.Shared;

/// <summary>
/// Minimal discriminated result type used in the ResultMapper sample.
/// In a real application you would use a dedicated library such as
/// ErrorOr, FluentResults, or your own domain Result type.
/// </summary>
public sealed class Result<T>
{
    private Result(bool isSuccess, T? value, string? errorCode, string? errorMessage, int status)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        Status = status;
    }

    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? ErrorCode { get; }
    public string? ErrorMessage { get; }

    /// <summary>HTTP status code intended for the error response (ignored on success).</summary>
    public int Status { get; }

    public static Result<T> Ok(T value) => new(true, value, null, null, 200);

    public static Result<T> NotFound(string message = "Not found") =>
        new(false, default, "NOT_FOUND", message, 404);

    public static Result<T> Conflict(string message) =>
        new(false, default, "CONFLICT", message, 409);

    public static Result<T> Invalid(string message) =>
        new(false, default, "VALIDATION", message, 400);

    public static Result<T> Failure(string message) =>
        new(false, default, "FAILURE", message, 500);
}
