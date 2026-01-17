namespace BlazorServerFunctions.Generator.UnitTests;

public class WeatherServiceGeneratorTests
{
    [Fact]
    public Task Generates_Server_Functions_For_WeatherService()
    {
        var source = """
                     using BlazorServerFunctions.Abstractions;
                     
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