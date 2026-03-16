using BlazorServerFunctions.Sample.Shared;

namespace BlazorServerFunctions.Sample.Components.Weather;

internal sealed class WeatherService : IWeatherService
{
    public async Task<WeatherForecastDto[]> GetWeatherForecastsAsync(CancellationToken cancellationToken = default)
    {
        // Simulate asynchronous loading to demonstrate streaming rendering
        await Task.Delay(500, cancellationToken).ConfigureAwait(false);

        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
        
#pragma warning disable CA5394
        var forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecastDto
        {
            Date = startDate.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = summaries[Random.Shared.Next(summaries.Length)]
        }).ToArray();
#pragma warning restore CA5394

        return forecasts;
    }
}