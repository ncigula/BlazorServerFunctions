namespace BlazorServerFunctions.Generator.Models;

internal sealed record InterfaceInfo
{
    public string Name { get; set; } = "";
    public string Namespace { get; set; } = "";
    public string? RoutePrefix { get; set; }
    public bool RequireAuthorization { get; set; }
    public List<MethodInfo> Methods { get; init; } = [];
}