using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace BlazorServerFunctions.Generator.IntegrationTests;

public class WeatherServiceServerIntegrationTests
{
    [Fact]
    public void Generator_Produces_Valid_Server_Functions_For_WeatherService()
    {
        var sharedFiles = TestHelpers.GetProjectFiles("BlazorServerFunctions.Sample.Shared");
        var serverFiles = TestHelpers.GetProjectFiles("BlazorServerFunctions.Sample");

        var syntaxTrees = sharedFiles.Concat(serverFiles)
            .Select(f => CSharpSyntaxTree.ParseText(File.ReadAllText(f), path: f))
            .ToArray();

        var references = IntegrationTestReferences.GetAll();

        var compilation = CSharpCompilation.Create(
            "BlazorServerFunctions.Sample",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new ServerFunctionCollectionGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGenerators(compilation);

        var result = driver.GetRunResult();

        var errors = result.Diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();

        Assert.True(errors.Count == 0,
            "Generator produced errors:\n" + string.Join("\n", errors));

        var generatedFiles = result.GeneratedTrees
            .Select(t => Path.GetFileName(t.FilePath))
            .ToList();

        Assert.Contains("WeatherService.ServerFunctions.g.cs", generatedFiles);

        var updatedCompilation = compilation.AddSyntaxTrees(result.GeneratedTrees);

        using var ms = new MemoryStream();
        var emit = updatedCompilation.Emit(ms);

        Assert.True(emit.Success,
            "Generated code failed to compile:\n" +
            string.Join("\n", emit.Diagnostics));

        var serverFunctionsType = updatedCompilation.GetTypeByMetadataName(
            "BlazorServerFunctions.Generated.WeatherServiceServerFunctions");

        Assert.NotNull(serverFunctionsType);

        var diExtensions = updatedCompilation.GetTypeByMetadataName(
            "BlazorServerFunctions.Generated.WeatherServiceServiceCollectionExtensions");

        Assert.NotNull(diExtensions);

        var endpointExtensions = updatedCompilation.GetTypeByMetadataName(
            "BlazorServerFunctions.Generated.WeatherServiceEndpointRouteBuilderExtensions");

        Assert.NotNull(endpointExtensions);
    }
}