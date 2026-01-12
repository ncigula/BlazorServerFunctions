using System.Runtime.CompilerServices;
using BlazorServerFunctions.Abstractions;
using BlazorServerFunctions.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace BlazorServerFunctions.UnitTests.Snapshots;

public class SnapshotTests
{
    private static IEnumerable<string> GetProjectFiles(string projectDirectoryName)
    {
        var currentFile = GetCurrentFilePath();
        var solutionRoot = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(currentFile))));
        var projectPath = Path.Combine(solutionRoot!, "samples", projectDirectoryName);
        
        return Directory.EnumerateFiles(projectPath, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") && 
                        !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));
    }

    private static string GetCurrentFilePath([CallerFilePath] string path = "") => path;

    [Fact]
    public async Task WeatherServiceServerSnapshot()
    {
        // Load the real files from Shared and Sample (Server)
        var sharedFiles = GetProjectFiles("BlazorServerFunctions.Sample.Shared");
        var serverFiles = GetProjectFiles("BlazorServerFunctions.Sample");

        var syntaxTrees = sharedFiles.Concat(serverFiles)
            .Select(f => CSharpSyntaxTree.ParseText(File.ReadAllText(f), path: f))
            .ToArray();

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .Cast<MetadataReference>()
            .ToList();

        references.Add(MetadataReference.CreateFromFile(typeof(ServerFunctionCollectionAttribute).Assembly.Location));

        var compilation = CSharpCompilation.Create(
            "BlazorServerFunctions.Sample",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Create the generator and driver
        var generator = new ServerFunctionCollectionGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Add analyzer options (build properties) for Server
        var optionsProvider = new TestAnalyzerConfigOptionsProvider(new Dictionary<string, string>
        {
            ["build_property.RootNamespace"] = "BlazorServerFunctions.Sample",
            ["build_property.ProjectName"] = "BlazorServerFunctions.Sample",
            ["build_property.UsingMicrosoftNETSdkWeb"] = "true"
        });
        
        driver = driver.WithUpdatedAnalyzerConfigOptions(optionsProvider);

        // Run the generator
        driver = driver.RunGenerators(compilation);

        // Verify the results
        await Verifier.Verify(driver);
    }

    [Fact]
    public async Task WeatherServiceClientSnapshot()
    {
        // Load the real files from Shared and Sample.Client
        var sharedFiles = GetProjectFiles("BlazorServerFunctions.Sample.Shared");
        var clientFiles = GetProjectFiles("BlazorServerFunctions.Sample.Client");

        var syntaxTrees = sharedFiles.Concat(clientFiles)
            .Select(f => CSharpSyntaxTree.ParseText(File.ReadAllText(f), path: f))
            .ToArray();

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .Cast<MetadataReference>()
            .ToList();

        references.Add(MetadataReference.CreateFromFile(typeof(ServerFunctionCollectionAttribute).Assembly.Location));

        var compilation = CSharpCompilation.Create(
            "BlazorServerFunctions.Sample.Client",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Create the generator and driver
        var generator = new ServerFunctionCollectionGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Add analyzer options (build properties) for Client
        var optionsProvider = new TestAnalyzerConfigOptionsProvider(new Dictionary<string, string>
        {
            ["build_property.RootNamespace"] = "BlazorServerFunctions.Sample.Client",
            ["build_property.ProjectName"] = "BlazorServerFunctions.Sample.Client",
            ["build_property.UsingMicrosoftNETSdkBlazorWebAssembly"] = "true"
        });
        
        driver = driver.WithUpdatedAnalyzerConfigOptions(optionsProvider);

        // Run the generator
        driver = driver.RunGenerators(compilation);

        // Verify the results
        await Verifier.Verify(driver);
    }
}
