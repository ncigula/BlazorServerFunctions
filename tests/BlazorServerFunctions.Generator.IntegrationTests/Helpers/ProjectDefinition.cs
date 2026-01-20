namespace BlazorServerFunctions.Generator.IntegrationTests.Helpers;

public class ProjectDefinition
{
    public string Name { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public ProjectType Type { get; init; }
    public IReadOnlyCollection<string> References { get; init; } = [];
}