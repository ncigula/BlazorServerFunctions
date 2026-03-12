using BlazorServerFunctions.Abstractions;

namespace BlazorServerFunctions.Sample.Shared;

[ServerFunctionCollection]
public interface IWeatherService
{
    [ServerFunction( HttpMethod = "GET")]
    Task<WeatherForecastDto[]> GetWeatherForecastsAsync();
}