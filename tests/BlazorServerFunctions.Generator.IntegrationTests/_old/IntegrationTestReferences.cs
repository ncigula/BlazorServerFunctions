using BlazorServerFunctions.Abstractions;
using Microsoft.CodeAnalysis;

namespace BlazorServerFunctions.Generator.IntegrationTests._old;

public static class IntegrationTestReferences
{
    public static IEnumerable<MetadataReference> GetAll()
    {
        var refs = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .ToList();

        refs.Add(MetadataReference.CreateFromFile(
            typeof(ServerFunctionCollectionAttribute).Assembly.Location));

        return refs;
    }
}