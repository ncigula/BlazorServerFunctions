using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace BlazorServerFunctions.Generator.IntegrationTests;

public class WeatherServiceServerSnapshotTests
{
    [Fact]
    public async Task WeatherServiceServerSnapshot()
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

        await Verifier.Verify(driver);
    }
}