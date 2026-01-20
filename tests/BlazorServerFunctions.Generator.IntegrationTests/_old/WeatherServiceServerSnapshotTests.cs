namespace BlazorServerFunctions.Generator.IntegrationTests._old;

public class WeatherServiceServerSnapshotTests
{
    [Fact]
    public void WeatherService_Generates_Server_Functions()
    {
        var host = IntegrationTestHost.Create(
            ["BlazorServerFunctions.Sample.Shared"],
            ["IWeatherService.cs"]);

        host.AssertTypesExistInCompilation(
            ["IWeatherServiceClient.g.cs", "ServerFunctionClientsRegistration.g.cs"],
            ["BlazorServerFunctions.Sample.Shared.WeatherServiceClient", "BlazorServerFunctions.Sample.Shared.ServerFunctionClientsRegistration"]);
    }
}