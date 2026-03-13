namespace BlazorServerFunctions.Generator.IntegrationTests._old;

public class WeatherServiceServerSnapshotTests
{
    [Fact]
    public void WeatherService_Generates_Server_Functions()
    {
        var host = IntegrationTestHost.Create(
            ["BlazorServerFunctions.Sample.Shared"],
            ["IWeatherService.cs"]);

        // Verify no generator errors, expected files are produced, and types exist in compilation.
        // Note: Full emit compilation is skipped because IWeatherService.cs depends on
        // WeatherForecastDto which is defined in a separate file not loaded here.
        host.AssertNoGeneratorErrors();
        host.AssertFilesAreGenerated(
            "WeatherServiceClient.g.cs",
            "ServerFunctionClientsRegistration.g.cs");
        host.AssertTypesExist(
            "BlazorServerFunctions.Sample.Shared.WeatherServiceClient",
            "BlazorServerFunctions.Sample.Shared.ServerFunctionClientsRegistration");
    }
}