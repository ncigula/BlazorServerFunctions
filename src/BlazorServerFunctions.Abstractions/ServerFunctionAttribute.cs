namespace BlazorServerFunctions.Abstractions;

[AttributeUsage(AttributeTargets.Method)]
public class ServerFunctionAttribute
{
    public string? Route { get; set; }
    public bool RequireAuthorization { get; set; }
    public string HttpMethod { get; set; } = "POST";
}