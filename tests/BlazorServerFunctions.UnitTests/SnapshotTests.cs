using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using BlazorServerFunctions.Generator;
using BlazorServerFunctions.Abstractions;
using VerifyXunit;
using Xunit;

namespace BlazorServerFunctions.UnitTests;

public class SnapshotTests
{
    [Fact]
    public async Task WeatherServiceSnapshotGeneratedCode()
    {
        // Define the source code
        var source = """
using System;
using System.Threading.Tasks;
using BlazorServerFunctions.Abstractions;

namespace BlazorServerFunctions.Sample.Shared;

public sealed class WeatherForecastDto
{
    public DateOnly Date { get; set; }
    public int TemperatureC { get; set; }
    public string? Summary { get; set; }
}

[ServerFunctionCollection]
public interface IWeatherService
{
    Task<WeatherForecastDto[]> GetWeatherForecastsAsync();
}
""";

        // Create compilation
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ServerFunctionCollectionAttribute).Assembly.Location),
            // Add other necessary references if needed
        };

        var compilation = CSharpCompilation.Create(
            "TestProject",
            new[] { syntaxTree },
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
