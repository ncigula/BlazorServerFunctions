namespace BlazorServerFunctions.Abstractions;

/// <summary>
/// Lightweight error description produced by
/// <see cref="IServerFunctionResultMapper{TResult,TValue}.GetError"/> on the server
/// and consumed by <see cref="IServerFunctionResultMapper{TResult,TValue}.WrapFailure"/>
/// on the client.
/// Mirrors the RFC 9457 / ProblemDetails fields without introducing a dependency on
/// <c>Microsoft.AspNetCore.Mvc.Core</c> in the shared abstractions library.
/// </summary>
public sealed class ServerFunctionError
{
    /// <summary>HTTP status code (e.g. 400, 404, 409, 500). Defaults to 500.</summary>
    public int Status { get; set; } = 500;

    /// <summary>Short, human-readable summary of the problem type.</summary>
    public string? Title { get; set; }

    /// <summary>Human-readable explanation specific to this occurrence.</summary>
    public string? Detail { get; set; }

    /// <summary>URI reference that identifies the problem type.</summary>
    public string? Type { get; set; }
}
