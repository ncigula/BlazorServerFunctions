using BlazorServerFunctions.Generator.Generators;

namespace BlazorServerFunctions.Generator.UnitTests;

public class ServerFunctionGeneratorCollectionTest
{
    [Theory]
    [InlineData(ProjectType.Server)]
    [InlineData(ProjectType.Client)]
    [InlineData(ProjectType.Library)]
    public Task Generates_Server_Files_For_WeatherService(ProjectType projectType)
    {
        var source = """
                     using BlazorServerFunctions.Abstractions;

                     namespace BlazorServerFunctions.Sample.Shared;

                     [ServerFunctionCollection]
                     public interface IWeatherService
                     {
                         Task<WeatherForecastDto[]> GetWeatherForecastsAsync();
                     }
                     """;

        var generator = new ServerFunctionCollectionGenerator();

        var result = GeneratorTestHelper.RunGenerator(
            source,
            generator,
            projectType);

        return Verify(result).UseParameters(projectType);
    }
}