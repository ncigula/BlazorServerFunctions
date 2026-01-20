using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace BlazorServerFunctions.Generator.IntegrationTests.Helpers;

public class CompiledProject
{
    public required ProjectDefinition Definition { get; init; }
    public required CSharpCompilation Compilation { get; init; }
    public required GeneratorDriverRunResult GeneratorResults { get; init; }
    public required MetadataReference AssemblyReference { get; init; }

    public IReadOnlyCollection<string> GeneratedFileNames => 
        GeneratorResults.GeneratedTrees
            .Select(t => Path.GetFileName(t.FilePath))
            .ToList();

    public bool HasGeneratedFile(string fileName) => 
        GeneratedFileNames.Any(f => f.Contains(fileName));

    public int GeneratedFileCount => GeneratorResults.GeneratedTrees.Length;

    public string GetGeneratedFileContent(string fileName)
    {
        var tree = GeneratorResults.GeneratedTrees
            .FirstOrDefault(t => t.FilePath.Contains(fileName));
        
        return tree?.ToString() ?? throw new InvalidOperationException($"File {fileName} not found");
    }

    public void AssertHasServerFiles(params string[] interfaceNames)
    {
        foreach (var name in interfaceNames)
        {
            Assert.True(HasGeneratedFile($"{name}ServerExtensions.g.cs"),
                $"Expected {name}ServerExtensions.g.cs in {Definition.Name}");
        }
        
        Assert.True(HasGeneratedFile("ServerFunctionEndpointsRegistration.g.cs"),
            $"Expected ServerFunctionEndpointsRegistration.g.cs in {Definition.Name}");
    }

    public void AssertHasClientFiles(params string[] interfaceNames)
    {
        foreach (var name in interfaceNames)
        {
            var clientName = name.TrimStart('I') + "Client.g.cs";
            Assert.True(HasGeneratedFile(clientName),
                $"Expected {clientName} in {Definition.Name}");
        }
        
        Assert.True(HasGeneratedFile("ServerFunctionClientsRegistration.g.cs"),
            $"Expected ServerFunctionClientsRegistration.g.cs in {Definition.Name}");
    }

    public void AssertHasNoGeneratedFiles()
    {
        Assert.Equal(0, GeneratedFileCount);
    }

    public void AssertCompilesSuccessfully()
    {
        var compilationWithGenerated = Compilation.AddSyntaxTrees(GeneratorResults.GeneratedTrees);
        var emitResult = compilationWithGenerated.Emit(Stream.Null);
        
        if (!emitResult.Success)
        {
            var errors = string.Join(Environment.NewLine,
                emitResult.Diagnostics
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .Select(d => d.ToString()));
            
            Assert.Fail($"Compilation failed:{Environment.NewLine}{errors}");
        }
    }
}