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

    /// <summary>
    /// Default output cache duration in seconds applied to all <c>GET</c> endpoints in this collection.
    /// <c>0</c> (default) means no caching. Individual methods can override this via
    /// <see cref="ServerFunctionAttribute.CacheSeconds"/> — set to <c>0</c> on a method to opt out
    /// even when a collection-level default is configured.
    /// Requires <c>builder.Services.AddOutputCache()</c> and <c>app.UseOutputCache()</c>.
    /// </summary>
    public int CacheSeconds { get; set; }

    /// <summary>
    /// Default rate-limiting policy name applied to all endpoints in this collection.
    /// <c>null</c> (default) means no rate limiting. Individual methods can override this via
    /// <see cref="ServerFunctionAttribute.RateLimitPolicy"/> — set to <c>""</c> on a method to
    /// opt out even when a collection-level default is configured.
    /// Requires <c>builder.Services.AddRateLimiter(...)</c> and <c>app.UseRateLimiter()</c>.
    /// </summary>
    public string? RateLimitPolicy { get; set; }

    /// <summary>
    /// Default named authorization policy applied to all endpoints in this collection
    /// via <c>.RequireAuthorization("policyName")</c>.
    /// <c>null</c> (default) means no named policy. Individual methods can override via
    /// <see cref="ServerFunctionAttribute.Policy"/> — set to <c>""</c> to opt out.
    /// </summary>
    public string? Policy { get; set; }

    /// <summary>
    /// Default named CORS policy applied to all endpoints in this collection
    /// via <c>group.RequireCors("policyName")</c>.
    /// <c>null</c> (default) means no CORS policy.
    /// The <see cref="ServerFunctionCollectionAttribute.CorsPolicy"/> attribute overrides this value.
    /// Requires <c>builder.Services.AddCors(...)</c> and <c>app.UseCors()</c> in the server pipeline.
    /// </summary>
    public string? CorsPolicy { get; set; }
}
