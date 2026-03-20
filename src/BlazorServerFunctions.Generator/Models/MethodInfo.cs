namespace BlazorServerFunctions.Generator.Models;

internal sealed record MethodInfo
{
    public string Name { get; set; } = "";
    public string ReturnType { get; set; } = "void";
    public string? CustomRoute { get; set; }
    public bool RequireAuthorization { get; set; }
    public HttpMethod HttpMethod { get; set; }
    public AsyncType AsyncType { get; set; }
    public List<ParameterInfo> Parameters { get; set; } = [];
    public bool HasCancellationToken { get; set; }

    /// <summary>
    /// Resolved output cache duration in seconds (0 = no caching).
    /// Already validated: streaming methods and non-GET methods always have this set to 0.
    /// </summary>
    public int CacheSeconds { get; set; }

    /// <summary>Resolved rate-limiting policy name, or <c>null</c> if none applies.</summary>
    public string? RateLimitPolicy { get; set; }

    /// <summary>Resolved named authorization policy, or <c>null</c> if none applies.</summary>
    public string? Policy { get; set; }

    /// <summary>Comma-separated role names, or <c>null</c> if none applies.</summary>
    public string? Roles { get; set; }

    /// <summary>Whether to emit <c>.ValidateAntiforgery()</c> on this endpoint.</summary>
    public bool RequireAntiForgery { get; set; }

    /// <summary>
    /// Ordered list of fully-qualified filter type names (global:: prefix) to emit as
    /// <c>.AddEndpointFilter&lt;T&gt;()</c> calls. Empty = no filters.
    /// </summary>
    public List<string> Filters { get; set; } = [];
}