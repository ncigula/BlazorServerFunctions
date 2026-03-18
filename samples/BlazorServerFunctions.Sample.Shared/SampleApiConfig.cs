using BlazorServerFunctions.Abstractions;

namespace BlazorServerFunctions.Sample.Shared;

/// <summary>
/// Demonstrates a compile-time configuration override for a group of server functions.
/// Sets a custom base route and kebab-case route naming instead of the default
/// PascalCase method-name segments.
/// </summary>
public sealed class SampleApiConfig : ServerFunctionConfiguration
{
    public SampleApiConfig()
    {
        BaseRoute = "api/v1";
        RouteNaming = RouteNaming.KebabCase;
    }
}
