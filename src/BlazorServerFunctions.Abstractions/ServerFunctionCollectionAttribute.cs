namespace BlazorServerFunctions.Abstractions;

[AttributeUsage(AttributeTargets.Interface)]
public sealed class ServerFunctionCollectionAttribute : Attribute
{
    public string? RoutePrefix { get; set; }
    
    public bool RequireAuthorization { get; set; }
}