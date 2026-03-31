namespace BlazorServerFunctions.Abstractions;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ServerFunctionAttribute : Attribute
{
    public string? Route { get; set; }
        
    public bool RequireAuthorization { get; set; }

    public string HttpMethod { get; set; } = string.Empty;

    /// <summary>
    /// Number of seconds to cache the response via ASP.NET Core Output Cache.
    /// Use <c>-1</c> (default) to inherit the value from <see cref="ServerFunctionConfiguration.CacheSeconds"/>.
    /// Use <c>0</c> to explicitly disable caching for this method even when a config default is set.
    /// Only valid on <c>GET</c> endpoints; setting this on other HTTP methods is an error (BSF020).
    /// Requires <c>builder.Services.AddOutputCache()</c> and <c>app.UseOutputCache()</c> in the server pipeline.
    /// </summary>
    public int CacheSeconds { get; set; } = -1;

    /// <summary>
    /// Name of the ASP.NET Core rate-limiting policy to apply via
    /// <c>.RequireRateLimiting("policyName")</c>.
    /// Use <c>null</c> (default) to inherit the value from <see cref="ServerFunctionConfiguration.RateLimitPolicy"/>.
    /// Use <c>""</c> (empty string) to explicitly disable rate limiting for this method even when a config default is set.
    /// Requires <c>builder.Services.AddRateLimiter(...)</c> and <c>app.UseRateLimiter()</c> in the server pipeline.
    /// </summary>
    public string? RateLimitPolicy { get; set; }

    /// <summary>
    /// Name of the ASP.NET Core authorization policy to apply via
    /// <c>.RequireAuthorization("policyName")</c>.
    /// Use <c>null</c> (default) to inherit the value from <see cref="ServerFunctionConfiguration.Policy"/>.
    /// Use <c>""</c> (empty string) to explicitly disable the named policy for this method even when a config default is set.
    /// Does not affect the boolean <see cref="RequireAuthorization"/> setting.
    /// </summary>
    public string? Policy { get; set; }

    /// <summary>
    /// Comma-separated role names applied via
    /// <c>.RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Manager" })</c>.
    /// Use <c>null</c> (default) to apply no role restriction on this method.
    /// Does not affect <see cref="RequireAuthorization"/> or <see cref="Policy"/>.
    /// Setting this to an empty string is an error (BSF021).
    /// </summary>
    public string? Roles { get; set; }

    /// <summary>
    /// When <c>true</c>, adds <c>.ValidateAntiforgery()</c> to the generated minimal API endpoint.
    /// Default: <c>false</c>.
    /// Requires antiforgery services (<c>builder.Services.AddAntiforgery()</c>) and the
    /// antiforgery middleware (<c>app.UseAntiforgery()</c>) in the server pipeline.
    /// </summary>
    public bool RequireAntiForgery { get; set; }

    /// <summary>
    /// One or more endpoint filter types applied via <c>.AddEndpointFilter&lt;TFilter&gt;()</c>
    /// on the generated minimal API endpoint, in declaration order.
    /// Each type must implement <c>IEndpointFilter</c>.
    /// Example: <c>Filters = new[] { typeof(MyFilter) }</c>
    /// </summary>
    public Type[]? Filters { get; set; }

    /// <summary>
    /// OpenAPI operation summary — the short label shown in Swagger UI next to the operation.
    /// Maps to <c>.WithSummary("...")</c> on the generated endpoint.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// OpenAPI operation description — a longer explanation that supports Markdown.
    /// Maps to <c>.WithDescription("...")</c> on the generated endpoint.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Tag(s) that override the auto-generated tag (interface name with the leading "I" stripped).
    /// When set, replaces the default <c>.WithTags(interfaceName)</c> call.
    /// Example: <c>Tags = new[] { "Products", "Catalog" }</c>
    /// </summary>
    public string[]? Tags { get; set; }

    /// <summary>
    /// Additional HTTP status codes to document via <c>.Produces(statusCode)</c>.
    /// Emitted alongside the existing <c>.Produces&lt;T&gt;(200)</c> annotation.
    /// Use for documenting 404, 409, etc.
    /// Example: <c>ProducesStatusCodes = new[] { 404, 409 }</c>
    /// </summary>
    public int[]? ProducesStatusCodes { get; set; }

    /// <summary>
    /// When <c>true</c>, emits <c>.ExcludeFromDescription()</c> on the generated endpoint
    /// instead of <c>.WithOpenApi()</c>, hiding this endpoint from the OpenAPI documentation.
    /// </summary>
    public bool ExcludeFromOpenApi { get; set; }
}