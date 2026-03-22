namespace BlazorServerFunctions.Generator.Models;

/// <summary>
/// Immutable snapshot of resolved configuration for one <c>[ServerFunctionCollection]</c> interface.
/// Defaults must exactly mirror <see cref="ServerFunctionConfiguration"/> so that interfaces
/// without an explicit <c>Configuration</c> property produce identical output to previous versions.
/// </summary>
internal sealed record ConfigurationInfo
{
    public static readonly ConfigurationInfo Default = new();

    public string BaseRoute { get; init; } = "api/functions";
    public RouteNaming RouteNaming { get; init; } = RouteNaming.PascalCase;
    public string? DefaultHttpMethod { get; init; }
    public bool GenerateProblemDetails { get; init; } = true;
    public bool Nullable { get; init; } = true;

    /// <summary>
    /// Fully-qualified type name of the custom HttpClient subtype, or <c>null</c> for plain HttpClient.
    /// Stored as a string rather than <c>Type?</c> or <c>ITypeSymbol</c> because generator pipeline
    /// models must be equatable value objects safe for incremental caching.
    /// </summary>
    public string? CustomHttpClientType { get; init; }

    public ApiType ApiType { get; init; } = ApiType.REST;

    /// <summary>Default output cache duration in seconds for GET endpoints (0 = no caching).</summary>
    public int CacheSeconds { get; init; }

    /// <summary>Default rate-limiting policy name (null = no rate limiting).</summary>
    public string? RateLimitPolicy { get; init; }

    /// <summary>Default named authorization policy (null = no named policy).</summary>
    public string? Policy { get; init; }

    /// <summary>Default named CORS policy (null = no CORS policy).</summary>
    public string? CorsPolicy { get; init; }
}
