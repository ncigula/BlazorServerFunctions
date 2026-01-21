using BlazorServerFunctions.Abstractions;

namespace BlazorServerFunctions.Sample.Shared;

[ServerFunctionCollection]
public interface IWeatherService
{
    [ServerFunction]
    Task<WeatherForecastDto[]> GetWeatherForecastsAsync();
}