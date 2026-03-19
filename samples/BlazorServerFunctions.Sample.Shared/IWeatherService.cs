using System.Collections.Generic;
using BlazorServerFunctions.Abstractions;

namespace BlazorServerFunctions.Sample.Shared;

[ServerFunctionCollection]
public interface IWeatherService
{
    [ServerFunction(HttpMethod = "GET")]
    Task<WeatherForecastDto[]> GetWeatherForecastsAsync(CancellationToken cancellationToken = default);

    [ServerFunction(HttpMethod = "GET")]
    IAsyncEnumerable<WeatherForecastDto> StreamForecastsAsync(CancellationToken cancellationToken = default);
}