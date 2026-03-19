using BlazorServerFunctions.Sample.Shared;

namespace BlazorServerFunctions.Sample.Components.Weather;

internal sealed class WeatherService : IWeatherService
{
    private static readonly string[] s_summaries =
        ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

    public async Task<WeatherForecastDto[]> GetWeatherForecastsAsync(CancellationToken cancellationToken = default)
    {
        // Simulate asynchronous loading to demonstrate streaming rendering
        await Task.Delay(500, cancellationToken).ConfigureAwait(false);

        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);

#pragma warning disable CA5394
        var forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecastDto
        {
            Date = startDate.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = s_summaries[Random.Shared.Next(s_summaries.Length)]
        }).ToArray();
#pragma warning restore CA5394

        return forecasts;
    }
}