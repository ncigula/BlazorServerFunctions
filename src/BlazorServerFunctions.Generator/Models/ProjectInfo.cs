namespace BlazorServerFunctions.Generator.Models;

internal sealed record ProjectInfo
{
    public bool GenerateEndpoints { get; set; }
    public bool GenerateClients { get; set; }
}