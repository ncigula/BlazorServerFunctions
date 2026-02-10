namespace BlazorServerFunctions.Generator.UnitTests;

/// <summary>
/// Unit tests for ClientProxyGenerator - testing string output generation
/// </summary>
public class ClientProxyGeneratorTests
{
    [Fact]
    public Task Generate_BasicInterface_ProducesCorrectCode()
    {
        var interfaceInfo = TestDataFactory.BasicGetInterface();

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    [Fact]
    public Task Generate_BasicInterface_ProducesCorrectCode2()
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

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());
        
        return result.VerifyNoDiagnostics();

    }
}