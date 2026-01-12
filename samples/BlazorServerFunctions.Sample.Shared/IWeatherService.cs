using System;
using System.Threading.Tasks;
using BlazorServerFunctions.Abstractions;

namespace BlazorServerFunctions.Sample.Shared;

[ServerFunctionCollection]
public interface IWeatherService
{
    Task<WeatherForecastDto[]> GetWeatherForecastsAsync();
}