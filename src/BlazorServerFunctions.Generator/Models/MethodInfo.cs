namespace BlazorServerFunctions.Generator.Models;

internal sealed record MethodInfo
{
    public string Name { get; set; } = "";
    public string ReturnType { get; set; } = "void";
    public string? CustomRoute { get; set; }
    public bool RequireAuthorization { get; set; }
    public string HttpMethod { get; set; } = "GET";
    public AsyncType AsyncType { get; set; }
    public bool HasCancellationToken { get; set; }
    public List<ParameterInfo> Parameters { get; set; } = new();
}