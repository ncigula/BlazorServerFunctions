using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using BlazorServerFunctions.Generator;
using BlazorServerFunctions.Abstractions;
using System.Runtime.CompilerServices;

namespace BlazorServerFunctions.UnitTests;

public class SnapshotTests
{
    private static string GetSampleSource(string fileName)
    {
        var currentFile = GetCurrentFilePath();
        // SnapshotTests.cs is in tests\BlazorServerFunctions.UnitTests\Snapshots
        var solutionRoot = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(currentFile))));
        var path = Path.Combine(solutionRoot!, "samples", "BlazorServerFunctions.Sample.Shared", fileName);
        return File.ReadAllText(path);
    }

    private static string GetCurrentFilePath([CallerFilePath] string path = "") => path;

    [Fact]
    public async Task WeatherServiceSnapshotGeneratedCode()
    {
        // Load the real files
        var interfaceSource = GetSampleSource("IWeatherService.cs");
        var dtoSource = GetSampleSource("WeatherForecastDto.cs");

        // Create compilation
        var syntaxTrees = new[]
        {
            CSharpSyntaxTree.ParseText(interfaceSource),
            CSharpSyntaxTree.ParseText(dtoSource)
        };

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .Cast<MetadataReference>()
            .ToList();

        references.Add(MetadataReference.CreateFromFile(typeof(ServerFunctionCollectionAttribute).Assembly.Location));

        var compilation = CSharpCompilation.Create(
            "TestProject",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Create the generator and driver
        var generator = new ServerFunctionCollectionGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Add analyzer options (build properties)
        var optionsProvider = new TestAnalyzerConfigOptionsProvider(new Dictionary<string, string>
        {
            ["build_property.RootNamespace"] = "BlazorServerFunctions.Sample",
            ["build_property.ProjectName"] = "BlazorServerFunctions.Sample.Server",
            ["build_property.UsingMicrosoftNETSdkWeb"] = "true"
        });
        
        driver = driver.WithUpdatedAnalyzerConfigOptions(optionsProvider);

        // Run the generator
        driver = driver.RunGenerators(compilation);

        // Verify the results
        await Verifier.Verify(driver);
    }
}
