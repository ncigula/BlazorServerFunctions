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
}