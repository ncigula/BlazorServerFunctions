namespace BlazorServerFunctions.Generator;

internal sealed class MethodInfo
{
    public string Name { get; set; } = "";
    public string ReturnType { get; set; } = "void";
    public string? CustomRoute { get; set; }
    public bool RequireAuthorization { get; set; }
    public string HttpMethod { get; set; } = "POST";
    public bool IsAsync { get; set; }
    public List<ParameterInfo> Parameters { get; set; } = new();
}