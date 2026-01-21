using BlazorServerFunctions.Generator.Generators;
using BlazorServerFunctions.Generator.UnitTests.Helpers;

namespace BlazorServerFunctions.Generator.UnitTests;

public class ServerFunctionCollectionTests
{
    [Theory]
    [InlineData(ProjectType.Server)]
    [InlineData(ProjectType.Client)]
    [InlineData(ProjectType.Library)]
    public Task ServerFunctionCollectionAttribute_On_Interface_Generates_Files(ProjectType projectType)
    {
        var source = """
                     using BlazorServerFunctions.Abstractions;

                     namespace BlazorServerFunctions.Sample.Shared;

                     [ServerFunctionCollection]
                     public interface IWeatherService
                     {
                         [ServerFunction]
                         Task<WeatherForecastDto[]> GetWeatherForecastsAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunServerFunctionCollectionGenerator(
            source,
            projectType);

        return VerifyExtensions.VerifyNoDiagnostics(result)
            .UseParameters(projectType);
    }
}