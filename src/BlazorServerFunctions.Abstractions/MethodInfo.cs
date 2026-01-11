using Microsoft.CodeAnalysis;

namespace BlazorServerFunctions.Abstractions;

public sealed record MethodInfo
{
    public required string Name { get; init; }
    public required IMethodSymbol Symbol { get; init; }
    public required string ReturnType { get; init; }
    public required bool IsAsync { get; init; }
    public required IReadOnlyCollection<ParameterInfo> Parameters { get; init; } = [];
    public string? CustomRoute { get; init; }
    public required bool RequireAuthorization { get; init; }
    public string HttpMethod { get; init; } = "POST";
}