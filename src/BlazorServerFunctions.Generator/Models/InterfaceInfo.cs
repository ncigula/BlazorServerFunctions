namespace BlazorServerFunctions.Generator.Models;

internal sealed record InterfaceInfo
{
    public string Name { get; set; } = "";
    public string Namespace { get; set; } = "";
    public string? RoutePrefix { get; set; }
    public bool RequireAuthorization { get; set; }
    public string? CorsPolicy { get; set; }
    public ConfigurationInfo Configuration { get; init; } = ConfigurationInfo.Default;
    public List<MethodInfo> Methods { get; init; } = [];

    /// <summary>
    /// Fully-qualified open-generic type name of the result mapper, e.g.
    /// <c>global::MyApp.ResultMapper</c> (no type parameters).
    /// <c>null</c> when no <c>ResultMapper</c> is configured on <c>[ServerFunctionCollection]</c>.
    /// Stored as a string rather than an <c>ITypeSymbol</c> so the model remains an equatable
    /// value type suitable for incremental generator caching.
    /// </summary>
    public string? ResultMapperTypeName { get; set; }
}