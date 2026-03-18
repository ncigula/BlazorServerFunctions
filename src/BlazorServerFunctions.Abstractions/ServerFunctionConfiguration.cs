namespace BlazorServerFunctions.Abstractions;

/// <summary>
/// Base class for compile-time source-generator configuration.
/// Subclass this, set properties in your constructor, and reference the type via
/// <c>[ServerFunctionCollection(Configuration = typeof(MyConfig))]</c>.
/// </summary>
/// <example>
/// <code>
/// public class MyApiConfig : ServerFunctionConfiguration
/// {
///     public MyApiConfig()
///     {
///         BaseRoute = "api/v1";
///         RouteNaming = RouteNaming.KebabCase;
///     }
/// }
///
/// [ServerFunctionCollection(Configuration = typeof(MyApiConfig))]
/// public interface IUserService { ... }
/// </code>
/// </example>
public class ServerFunctionConfiguration
{
    /// <summary>Base route prefix for all endpoints. Default: <c>"api/functions"</c>.</summary>
    public string BaseRoute { get; set; } = "api/functions";

    /// <summary>Naming convention applied to route segments. Default: <see cref="RouteNaming.PascalCase"/>.</summary>
    public RouteNaming RouteNaming { get; set; } = RouteNaming.PascalCase;

    /// <summary>
    /// Default HTTP method for all methods in this collection.
    /// When set, <c>[ServerFunction]</c> methods no longer require an explicit <c>HttpMethod</c>.
    /// Default: <c>null</c> (must be explicit per method).
    /// </summary>
    public string? DefaultHttpMethod { get; set; }

    /// <summary>Whether to generate Problem Details error responses. Default: <c>true</c>.</summary>
    public bool GenerateProblemDetails { get; set; } = true;

    /// <summary>Whether to add standard resilience (retries, circuit breaker) via Microsoft.Extensions.Http.Resilience. Default: <c>false</c>.</summary>
    public bool EnableResilience { get; set; }

    /// <summary>Whether to emit <c>#nullable enable</c> in generated files. Default: <c>true</c>.</summary>
    public bool Nullable { get; set; } = true;

    /// <summary>
    /// Custom <see cref="System.Net.Http.HttpClient"/> subtype to inject into generated proxies.
    /// Must extend <see cref="System.Net.Http.HttpClient"/>.
    /// Default: <c>null</c> (uses plain <see cref="System.Net.Http.HttpClient"/>).
    /// </summary>
    public Type? CustomHttpClientType { get; set; }

    /// <summary>API transport type. Default: <see cref="ApiType.REST"/>.</summary>
    public ApiType ApiType { get; set; } = ApiType.REST;
}
