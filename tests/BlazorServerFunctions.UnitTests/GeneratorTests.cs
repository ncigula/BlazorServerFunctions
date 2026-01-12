using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using BlazorServerFunctions.Generator;
using BlazorServerFunctions.Abstractions;
using Xunit;

namespace BlazorServerFunctions.UnitTests;

public class GeneratorTests
{
    [Fact]
    public void WeatherServiceGeneratesCodeWithoutExceptions()
    {
        // Define the source code to be analyzed
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

        // Check for exceptions
        var runResult = driver.GetRunResult();
        Assert.Empty(runResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
    }
}
