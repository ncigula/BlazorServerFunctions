using System.Reflection;
using BlazorServerFunctions.Abstractions;
using Microsoft.CodeAnalysis;

namespace BlazorServerFunctions.Generator.IntegrationTests.Helpers;

public static class CompilationHelper
{
    /// <summary>
    /// Returns metadata references needed for all project types.
    /// IMPORTANT: Must NOT include project-type-marker assemblies:
    ///   - Microsoft.AspNetCore.Routing (contains IEndpointRouteBuilder → detected as Server)
    ///   - Microsoft.AspNetCore.Components.WebAssembly (contains WebAssemblyHostBuilder → detected as Client)
    /// Those must only appear in GetServerReferences() / GetClientReferences().
    /// </summary>
    public static IEnumerable<MetadataReference> GetBasicReferences()
    {
        // .NET core framework
        yield return MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        yield return MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location);
        yield return MetadataReference.CreateFromFile(typeof(Console).Assembly.Location);
        yield return MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location);
        yield return MetadataReference.CreateFromFile(Assembly.Load("System.Collections").Location);
        yield return MetadataReference.CreateFromFile(Assembly.Load("System.Collections.Specialized").Location);
        yield return MetadataReference.CreateFromFile(Assembly.Load("System.Threading.Tasks").Location);
        yield return MetadataReference.CreateFromFile(Assembly.Load("System.Linq").Location);

        // Abstractions
        yield return MetadataReference.CreateFromFile(typeof(ServerFunctionCollectionAttribute).Assembly.Location);

        // Generated client proxy and registration code dependencies
        yield return MetadataReference.CreateFromFile(typeof(System.Uri).Assembly.Location);
        yield return MetadataReference.CreateFromFile(typeof(System.Net.Http.HttpClient).Assembly.Location);
        yield return MetadataReference.CreateFromFile(typeof(System.Net.Http.Json.HttpClientJsonExtensions).Assembly.Location);
        yield return MetadataReference.CreateFromFile(Assembly.Load("System.Net.Primitives").Location);
        yield return MetadataReference.CreateFromFile(Assembly.Load("System.Net.Http").Location);
        yield return MetadataReference.CreateFromFile(Assembly.Load("System.Text.Json").Location);
        yield return MetadataReference.CreateFromFile(Assembly.Load("System.Web.HttpUtility").Location);
        yield return MetadataReference.CreateFromFile(Assembly.Load("Microsoft.Extensions.DependencyInjection.Abstractions").Location);
        yield return MetadataReference.CreateFromFile(Assembly.Load("Microsoft.Extensions.Http").Location);
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

        yield return MetadataReference.CreateFromFile(
            Assembly.Load("Microsoft.AspNetCore.Routing.Abstractions").Location);

        yield return MetadataReference.CreateFromFile(
            Assembly.Load("Microsoft.AspNetCore.Http.Abstractions").Location);
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
