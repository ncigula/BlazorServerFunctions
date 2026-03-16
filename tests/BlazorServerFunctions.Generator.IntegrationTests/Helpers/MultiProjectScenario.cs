using BlazorServerFunctions.Generator.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace BlazorServerFunctions.Generator.IntegrationTests.Helpers;

public class MultiProjectScenario
{
    private readonly IReadOnlyCollection<ProjectDefinition> _projects;
    private readonly Dictionary<string, CompiledProject> _compiledProjects = new(StringComparer.Ordinal);

    public MultiProjectScenario(IReadOnlyCollection<ProjectDefinition> projects)
    {
        _projects = projects;
        CompileAll();
    }

    private void CompileAll()
    {
        // Compile in dependency order (libraries first, then client, then server)
        var ordered = TopologicalSort(_projects);

        foreach (var project in ordered)
        {
            var compilation = CreateCompilation(project);
            var generatorResults = RunGenerator(compilation);
            
            _compiledProjects[project.Name] = new CompiledProject
            {
                Definition = project,
                Compilation = compilation,
                GeneratorResults = generatorResults,
                AssemblyReference = EmitToReference(compilation, generatorResults)
            };
        }
    }

    private CSharpCompilation CreateCompilation(ProjectDefinition project)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(project.Source);
        
        var references = CompilationHelper.GetBasicReferences().ToList();

        // Add project type specific references
        switch (project.Type)
        {
            case ProjectType.Server:
                references.AddRange(CompilationHelper.GetServerReferences());
                break;
            case ProjectType.Client:
                references.AddRange(CompilationHelper.GetClientReferences());
                break;
            case ProjectType.Library:
                // Libraries don't need special references
                break;
        }

        // Add referenced project assemblies (and their transitive dependencies)
        var seen = new HashSet<string>(StringComparer.Ordinal);
        AddReferencesRecursive(project.References, references, seen);

        return CSharpCompilation.Create(
            project.Name,
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private void AddReferencesRecursive(
        IEnumerable<string> refNames,
        List<MetadataReference> references,
        HashSet<string> seen)
    {
        foreach (var refName in refNames)
        {
            if (!seen.Add(refName)) continue;
            if (!_compiledProjects.TryGetValue(refName, out var refProject)) continue;

            references.Add(refProject.AssemblyReference);
            AddReferencesRecursive(refProject.Definition.References, references, seen);
        }
    }

    private static GeneratorDriverRunResult RunGenerator(CSharpCompilation compilation)
    {
        var generator = new ServerFunctionCollectionGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGenerators(compilation);
        return driver.GetRunResult();
    }

    private static PortableExecutableReference EmitToReference(
        CSharpCompilation compilation,
        GeneratorDriverRunResult generatorResults)
    {
        // Include generated trees so consuming projects can reference generated types
        var compilationWithGenerated = compilation.AddSyntaxTrees(generatorResults.GeneratedTrees);
        using var stream = new MemoryStream();
        var emitResult = compilationWithGenerated.Emit(stream);
        
        if (!emitResult.Success)
        {
            var errors = string.Join(Environment.NewLine, 
                emitResult.Diagnostics
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .Select(d => d.ToString()));
            throw new InvalidOperationException($"Compilation failed:{Environment.NewLine}{errors}");
        }

        stream.Seek(0, SeekOrigin.Begin);
        return MetadataReference.CreateFromStream(stream);
    }

    private static List<ProjectDefinition> TopologicalSort(IReadOnlyCollection<ProjectDefinition> projects)
    {
        // Sort projects so dependencies are compiled first
        var sorted = new List<ProjectDefinition>();
        var visited = new HashSet<string>(StringComparer.Ordinal);

        void Visit(ProjectDefinition project)
        {
            if (visited.Contains(project.Name))
                return;

            foreach (var refName in project.References)
            {
                var refProject = projects.FirstOrDefault(p => string.Equals(p.Name, refName, StringComparison.Ordinal));
                if (refProject != null)
                {
                    Visit(refProject);
                }
            }

            visited.Add(project.Name);
            sorted.Add(project);
        }

        foreach (var project in projects)
        {
            Visit(project);
        }

        return sorted;
    }

    public CompiledProject GetProject(string name)
    {
        return _compiledProjects[name];
    }

    public CompiledProject Server => _compiledProjects.Values.First(p => p.Definition.Type == ProjectType.Server);
    public CompiledProject Client => _compiledProjects.Values.First(p => p.Definition.Type == ProjectType.Client);
    
    public IEnumerable<CompiledProject> SharedProjects => 
        _compiledProjects.Values.Where(p => p.Definition.Type == ProjectType.Library);
}