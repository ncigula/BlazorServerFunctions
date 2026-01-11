using Microsoft.CodeAnalysis;

namespace BlazorServerFunctions.Abstractions;

public sealed record MethodInfo
{
    public string Name { get; set; } = string.Empty;
    public IMethodSymbol? Symbol { get; set; }
    public string ReturnType { get; set; } = string.Empty;
    public bool IsAsync { get; set; }
    public IReadOnlyCollection<ParameterInfo> Parameters { get; set; } = [];
    public string? CustomRoute { get; set; }
    public bool RequireAuthorization { get; set; }
    public string HttpMethod { get; set; } = "POST";
}