namespace BlazorServerFunctions.Generator.Models;

internal sealed record ParameterInfo
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public bool HasDefaultValue { get; set; }
    public string? DefaultValue { get; set; }
    public bool IsRouteParameter { get; set; }
    public bool IsValueType { get; set; }
    public FileKind FileKind { get; set; } = FileKind.None;
}