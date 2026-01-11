using Microsoft.CodeAnalysis;

namespace BlazorServerFunctions.Abstractions;

public sealed record InterfaceInfo
{
    public required INamedTypeSymbol Symbol { get; init; }
    public required string Name { get; init; }
    public required string Namespace { get; init; }
    public required string RoutePrefix { get; init; }
    public required bool RequireAuthorization { get; init; }
    public required IReadOnlyCollection<MethodInfo> Methods { get; init; } = [];
}