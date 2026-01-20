using System.Reflection;
using BlazorServerFunctions.Abstractions;
using Microsoft.CodeAnalysis;

namespace BlazorServerFunctions.Generator.IntegrationTests.Helpers;

public static class CompilationHelper
{
    public static IEnumerable<MetadataReference> GetBasicReferences()
    {
        yield return MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        yield return MetadataReference.CreateFromFile(typeof(Console).Assembly.Location);
        yield return MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location);
        yield return MetadataReference.CreateFromFile(typeof(ServerFunctionCollectionAttribute).Assembly.Location);
        
        yield return MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location);
        yield return MetadataReference.CreateFromFile(Assembly.Load("System.Collections").Location);

        // var references = AppDomain.CurrentDomain
        //     .GetAssemblies()
        //     .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
        //     .Select(a => MetadataReference.CreateFromFile(a.Location));
        //
        // foreach (reference portableExecutableReference in references)
        // {
        //     yield return reference;
        // }
        //
        // yield return MetadataReference.CreateFromFile(
        //     Assembly.Load("System.Threading.Tasks").Location
        // );
        //
        // yield return MetadataReference.CreateFromFile(typeof(ServerFunctionCollectionAttribute).Assembly.Location);
    }

    public static IEnumerable<MetadataReference> GetServerReferences()
    {
        yield return MetadataReference.CreateFromFile(
            typeof(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder).Assembly.Location);

        yield return MetadataReference.CreateFromFile(
            typeof(Microsoft.AspNetCore.Builder.WebApplication).Assembly.Location);

        yield return MetadataReference.CreateFromFile(
            typeof(Microsoft.AspNetCore.Http.Results).Assembly.Location);

        yield return MetadataReference.CreateFromFile(
            typeof(Microsoft.AspNetCore.Mvc.FromBodyAttribute).Assembly.Location);
    }

    public static IEnumerable<MetadataReference> GetClientReferences()
    {
        yield return MetadataReference.CreateFromFile(
            typeof(Microsoft.AspNetCore.Components.WebAssembly.Hosting.WebAssemblyHostBuilder).Assembly.Location);

        yield return MetadataReference.CreateFromFile(
            typeof(System.Net.Http.HttpClient).Assembly.Location);

        yield return MetadataReference.CreateFromFile(
            typeof(System.Net.Http.Json.HttpClientJsonExtensions).Assembly.Location);
    }
}