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
    public bool EnableResilience { get; init; }
    public bool Nullable { get; init; } = true;

    /// <summary>
    /// Fully-qualified type name of the custom HttpClient subtype, or <c>null</c> for plain HttpClient.
    /// Stored as a string rather than <c>Type?</c> or <c>ITypeSymbol</c> because generator pipeline
    /// models must be equatable value objects safe for incremental caching.
    /// </summary>
    public string? CustomHttpClientType { get; init; }

    public ApiType ApiType { get; init; } = ApiType.REST;
}
