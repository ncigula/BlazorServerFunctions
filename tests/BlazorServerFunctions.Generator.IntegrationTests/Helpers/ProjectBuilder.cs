namespace BlazorServerFunctions.Generator.IntegrationTests.Helpers;

/// <summary>
/// Fluent builder for creating multi-project test scenarios
/// </summary>
public class ProjectBuilder
{
    private readonly List<ProjectDefinition> _projects = new();

    public ProjectBuilder AddSharedProject(string name, string source)
    {
        _projects.Add(new ProjectDefinition
        {
            Name = name,
            Source = source,
            Type = ProjectType.Library
        });
        return this;
    }

    public ProjectBuilder AddClientProject(string name, string source, params string[] references)
    {
        _projects.Add(new ProjectDefinition
        {
            Name = name,
            Source = source,
            Type = ProjectType.Client,
            References = references.ToList()
        });
        return this;
    }

    public ProjectBuilder AddServerProject(string name, string source, params string[] references)
    {
        _projects.Add(new ProjectDefinition
        {
            Name = name,
            Source = source,
            Type = ProjectType.Server,
            References = references.ToList()
        });
        return this;
    }

    public MultiProjectScenario Build()
    {
        return new MultiProjectScenario(_projects);
    }
}