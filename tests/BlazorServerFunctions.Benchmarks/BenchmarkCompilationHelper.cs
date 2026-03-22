using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using BlazorServerFunctions.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace BlazorServerFunctions.Benchmarks;

internal static class BenchmarkCompilationHelper
{
    /// <summary>
    /// Builds a CSharpCompilation configured as a server project (has IEndpointRouteBuilder)
    /// containing <paramref name="interfaceCount"/> BSF interfaces each with
    /// <paramref name="methodsPerInterface"/> methods.
    /// </summary>
    internal static CSharpCompilation BuildServerCompilation(
        int interfaceCount,
        int methodsPerInterface = 3)
    {
        var source = BuildInterfaceSource(interfaceCount, methodsPerInterface);
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = BuildServerReferences();

        return CSharpCompilation.Create(
            "BenchmarkTest",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    /// <summary>
    /// Generates C# source with N interfaces, each having M [ServerFunction]-annotated methods.
    /// </summary>
    internal static string BuildInterfaceSource(int interfaceCount, int methodsPerInterface = 3)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using BlazorServerFunctions.Abstractions;");
        sb.AppendLine();

        for (var i = 1; i <= interfaceCount; i++)
        {
            sb.Append(CultureInfo.InvariantCulture, $"[ServerFunctionCollection(RoutePrefix = \"api/bench{i}\")]").AppendLine();
            sb.Append(CultureInfo.InvariantCulture, $"public interface IService{i}").AppendLine();
            sb.AppendLine("{");

            for (var m = 1; m <= methodsPerInterface; m++)
            {
                sb.AppendLine("    [ServerFunction(HttpMethod = \"GET\")]");
                sb.Append(CultureInfo.InvariantCulture, $"    Task<string> Method{m}Async(int id, string name);").AppendLine();
                sb.AppendLine();
            }

            sb.AppendLine("}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static List<MetadataReference> BuildServerReferences()
    {
        // Mirrors GeneratorTestHelper.RunGeneratorAsServer():
        // Load all AppDomain assemblies, strip AspNetCore/Http references that would
        // confuse project-type detection, then add server-specific markers back.
        var references = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .Cast<MetadataReference>()
            .ToList();

        references.RemoveAll(r =>
        {
            var display = r.Display ?? string.Empty;
            return display.Contains("Microsoft.AspNetCore", StringComparison.OrdinalIgnoreCase) ||
                   display.Contains("Microsoft.Extensions.Http", StringComparison.OrdinalIgnoreCase);
        });

        // Abstractions (contains ServerFunctionCollectionAttribute)
        references.Add(MetadataReference.CreateFromFile(
            typeof(ServerFunctionCollectionAttribute).Assembly.Location));

        // Server marker: IEndpointRouteBuilder signals "this is a server project"
        var routing = typeof(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder).Assembly;
        references.Add(MetadataReference.CreateFromFile(routing.Location));

        return references;
    }
}
