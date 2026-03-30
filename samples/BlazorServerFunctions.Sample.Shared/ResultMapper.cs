using BlazorServerFunctions.Abstractions;

namespace BlazorServerFunctions.Sample.Shared;

/// <summary>
/// Maps <see cref="Result{T}"/> to and from the HTTP representation used by
/// BlazorServerFunctions generated endpoints and client proxies.
/// <para>
/// Register on the collection with <c>ResultMapper = typeof(ResultMapper&lt;&gt;)</c>.
/// </para>
/// </summary>
public sealed class ResultMapper<T> : IServerFunctionResultMapper<Result<T>, T>
    where T : notnull
{
    // ── Server side ──────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public bool IsSuccess(Result<T> result) => result.IsSuccess;

    /// <inheritdoc/>
    public T GetValue(Result<T> result) => result.Value!;

    /// <inheritdoc/>
    public ServerFunctionError GetError(Result<T> result) => new()
    {
        Status = result.Status,
        Title = result.ErrorCode,
        Detail = result.ErrorMessage,
        Type = result.ErrorCode switch
        {
            "NOT_FOUND" => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            "CONFLICT" => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            "VALIDATION" => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
        },
    };

    // ── Client side ──────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public Result<T> WrapValue(T value) => Result<T>.Ok(value);

    /// <inheritdoc/>
    public Result<T> WrapFailure(ServerFunctionError error) => error.Status switch
    {
        404 => Result<T>.NotFound(error.Detail ?? "Not found"),
        409 => Result<T>.Conflict(error.Detail ?? "Conflict"),
        400 => Result<T>.Invalid(error.Detail ?? "Validation error"),
        _ => Result<T>.Failure(error.Detail ?? "An error occurred"),
    };
}
