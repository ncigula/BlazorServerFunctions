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
}