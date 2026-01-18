namespace BlazorServerFunctions.Generator.Models;

internal sealed class ParameterInfo
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public bool HasDefaultValue { get; set; }
    public string? DefaultValue { get; set; }
}