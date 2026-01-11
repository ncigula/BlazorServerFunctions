namespace BlazorServerFunctions.Abstractions;

public sealed record ParameterInfo
{
    public required string Name { get; init; }
    public required string Type { get; init; }
    public bool HasDefaultValue { get; init; }
    public string? DefaultValue { get; init; }
}