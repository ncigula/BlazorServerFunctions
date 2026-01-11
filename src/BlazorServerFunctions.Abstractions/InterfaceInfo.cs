using Microsoft.CodeAnalysis;

namespace BlazorServerFunctions.Abstractions;

public sealed record InterfaceInfo
{
    public INamedTypeSymbol? Symbol { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string RoutePrefix { get; set; } = string.Empty;
    public bool RequireAuthorization { get; set; }
    public IReadOnlyCollection<MethodInfo> Methods { get; set; } = [];
}