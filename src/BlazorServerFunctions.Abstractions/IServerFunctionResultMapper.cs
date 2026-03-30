namespace BlazorServerFunctions.Abstractions;

/// <summary>
/// Converts between a result wrapper type (e.g. <c>Result&lt;T&gt;</c>,
/// <c>Result&lt;T, TError&gt;</c>, <c>OneOf&lt;T1, T2&gt;</c>) and the HTTP
/// representation used by generated endpoints and client proxies.
/// </summary>
/// <typeparam name="TResult">The full result wrapper type returned by the service method.</typeparam>
/// <typeparam name="TValue">
/// The success value type — the first generic type argument of <typeparamref name="TResult"/>.
/// This is the type serialised as the HTTP response body on a 200 OK.
/// </typeparam>
/// <remarks>
/// <para>
/// Implement this interface once per result library and configure it on the collection:
/// <code>
/// [ServerFunctionCollection(ResultMapper = typeof(MyResultMapper&lt;&gt;))]
/// public interface IOrderService { ... }
/// </code>
/// </para>
/// <para>
/// <b>Server side</b> — the generated endpoint calls <see cref="IsSuccess"/>,
/// <see cref="GetValue"/>, and <see cref="GetError"/> to build the HTTP response.
/// </para>
/// <para>
/// <b>Client side</b> — the generated proxy calls <see cref="WrapValue"/> and
/// <see cref="WrapFailure"/> to reconstruct the result wrapper from the HTTP response.
/// </para>
/// <para>
/// Instances are created with <c>new()</c>, so implementations must have a public
/// parameterless constructor. Mappers are intentionally stateless converters;
/// if you need DI dependencies, use the exception + global <c>IExceptionHandler</c>
/// pattern instead.
/// </para>
/// </remarks>
public interface IServerFunctionResultMapper<TResult, TValue>
    where TValue : notnull
{
    // ── Server side ─────────────────────────────────────────────────────────

    /// <summary>Returns <c>true</c> when <paramref name="result"/> represents success.</summary>
    bool IsSuccess(TResult result);

    /// <summary>
    /// Extracts the success value. Only called when <see cref="IsSuccess"/> returns <c>true</c>.
    /// </summary>
    TValue GetValue(TResult result);

    /// <summary>
    /// Extracts failure information. Only called when <see cref="IsSuccess"/> returns <c>false</c>.
    /// The returned <see cref="ServerFunctionError"/> is used to build a ProblemDetails HTTP response.
    /// </summary>
    ServerFunctionError GetError(TResult result);

    // ── Client side ─────────────────────────────────────────────────────────

    /// <summary>
    /// Wraps a deserialised success value back into the result type.
    /// Called by the generated client proxy when the HTTP response is 2xx.
    /// </summary>
    TResult WrapValue(TValue value);

    /// <summary>
    /// Wraps failure information into the result type.
    /// Called by the generated client proxy when the HTTP response is 4xx or 5xx.
    /// The <see cref="ServerFunctionError"/> is populated from the ProblemDetails response body.
    /// </summary>
    TResult WrapFailure(ServerFunctionError error);
}
