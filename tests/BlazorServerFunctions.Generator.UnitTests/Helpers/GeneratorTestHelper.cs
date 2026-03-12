using Microsoft.CodeAnalysis.CSharp;

namespace BlazorServerFunctions.Generator.UnitTests.Helpers;

public static class GeneratorTestHelper
{
    public static GeneratorDriverRunResult RunGeneratorAsServer(
        string source,
        IIncrementalGenerator generator,
        params PortableExecutableReference[] extraReferences)
    {
        return RunGenerator(source, generator, ProjectType.Server, extraReferences);
    }

    public static GeneratorDriverRunResult RunGeneratorAsClient(
        string source,
        IIncrementalGenerator generator,
        params PortableExecutableReference[] extraReferences)
    {
        return RunGenerator(source, generator, ProjectType.Client, extraReferences);
    }

    public static GeneratorDriverRunResult RunGeneratorAsLibrary(
        string source,
        IIncrementalGenerator generator,
        params PortableExecutableReference[] extraReferences)
    {
        return RunGenerator(source, generator, ProjectType.Library, extraReferences);
    }

    private static GeneratorDriverRunResult RunGenerator(
        string source,
        IIncrementalGenerator generator,
        ProjectType projectType,
        params PortableExecutableReference[] extraReferences)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .ToList();

        references.RemoveAll(r =>
        {
            var display = r.Display ?? string.Empty;
            return display.Contains("Microsoft.AspNetCore", StringComparison.OrdinalIgnoreCase) ||
                   display.Contains("Microsoft.Extensions.Http", StringComparison.OrdinalIgnoreCase);
        });

        references.Add(MetadataReference.CreateFromFile(
            typeof(ServerFunctionCollectionAttribute).Assembly.Location));
        
        switch (projectType)
        {
            case ProjectType.Server:
                var routing = typeof(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder).Assembly;
                references.Add(MetadataReference.CreateFromFile(routing.Location));
                break;

            case ProjectType.Client:
                var blazorWasm = typeof(Microsoft.AspNetCore.Components.WebAssembly.Hosting.WebAssemblyHostBuilder).Assembly;
                references.Add(MetadataReference.CreateFromFile(blazorWasm.Location));
                break;

            case ProjectType.Library:
                break;
        }

        references.AddRange(extraReferences);

        var compilation = CSharpCompilation.Create(
            "Tests",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Debug: Print compilation diagnostics
        var compilationDiagnostics = compilation.GetDiagnostics();
        foreach (var diag in compilationDiagnostics)
        {
            Console.WriteLine($"[Test Compilation] {diag}");
        }

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGenerators(compilation);

        return driver.GetRunResult();
    }

    public static GeneratorDriverRunResult RunGenerator(
        string source,
        IIncrementalGenerator generator,
        ProjectType projectType)
    {
        //var metadata = MetadataReference.CreateFromFile(
        //    typeof(ServerFunctionCollectionAttribute).Assembly.Location);

        return projectType switch
        {
            ProjectType.Server => RunGeneratorAsServer(source, generator),
            ProjectType.Client => RunGeneratorAsClient(source, generator),
            ProjectType.Library => RunGeneratorAsLibrary(source, generator),
            _ => throw new ArgumentOutOfRangeException(nameof(projectType)),
        };
    }

    public static GeneratorDriverRunResult RunServerFunctionCollectionGenerator(
        string source,
        ProjectType projectType) =>
        RunGenerator(
            source,
            new ServerFunctionCollectionGenerator(),
            projectType);
}