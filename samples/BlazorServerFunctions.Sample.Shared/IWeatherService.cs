namespace BlazorServerFunctions.Sample.Shared;

public interface IWeatherService
{
    Task<WeatherForecastDto[]> GetWeatherForecastsAsync();
}