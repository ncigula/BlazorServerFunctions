using BlazorServerFunctions.Generator.Generators;

namespace BlazorServerFunctions.Generator.UnitTests;

public class ServerFunctionGeneratorCollectionTest
{
    [Fact]
    public Task Generates_Server_Functions_For_WeatherService()
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
            MetadataReference.CreateFromFile(typeof(ServerFunctionCollectionAttribute).Assembly.Location));

        return Verify(result);
    }
}