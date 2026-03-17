namespace BlazorServerFunctions.Generator.Models;

internal sealed record ProjectInfo
{
    public bool GenerateEndpoints { get; set; }
    public bool GenerateClients { get; set; }

    /// <summary>
    /// True when the project is neither a server nor a WASM client (plain class library).
    /// Source libraries with only locally-defined interfaces skip registration generation —
    /// the consuming Client/Server project generates registration when it references this library.
    /// </summary>
    public bool IsLibrary { get; set; }
}