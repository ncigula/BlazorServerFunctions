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
}