namespace BlazorServerFunctions.EndToEndTests;

/// <summary>
/// Client path: resolves IWeatherService from DI → WeatherServiceClient (HTTP proxy) → in-memory server.
/// Exercises the generated client proxy + generated server endpoints together,
/// mirroring Blazor WASM/Auto components that inject IWeatherService.
/// </summary>
public sealed class WeatherServiceClientTests(E2EFixture fixture) : IClassFixture<E2EFixture>
{
    private IWeatherService Client =>
        fixture.ClientServices.GetRequiredService<IWeatherService>();

    [Fact]
    public async Task GetWeatherForecastsAsync_Returns5Forecasts()
    {
        var result = await Client.GetWeatherForecastsAsync();
        Assert.Equal(5, result.Length);
    }

    [Fact]
    public async Task GetWeatherForecastsAsync_AllForecastsHaveRequiredFields()
    {
        var result = await Client.GetWeatherForecastsAsync();
        Assert.All(result, f =>
        {
            Assert.NotNull(f.Summary);
            Assert.NotEqual(default, f.Date);
        });
    }

    [Fact]
    public async Task GetWeatherForecastsAsync_TemperatureFMatchesFormula()
    {
        var result = await Client.GetWeatherForecastsAsync();
        Assert.All(result, f =>
            Assert.Equal(32 + (int)(f.TemperatureC / 0.5556), f.TemperatureF));
    }

    // Both cancellation tests are skipped for the HTTP client path: the TestHost in .NET 10
    // preview has a recursive CancellationToken propagation bug (ClientInitiatedAbort ↔
    // RequestLifetimeFeature.Cancel loop → stack overflow) that triggers for both pre-
    // cancelled and mid-flight tokens passed to HTTP requests. Cancellation behaviour is
    // fully covered by the server-path equivalents in WeatherServiceServerTests.
    [Fact(Skip = "TestHost recursive cancellation in .NET 10 preview — covered by server-path test")]
    public async Task GetWeatherForecastsAsync_WithPreCancelledToken_Throws()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => Client.GetWeatherForecastsAsync(cts.Token));
    }

    [Fact(Skip = "TestHost recursive cancellation in .NET 10 preview — covered by server-path test")]
    public async Task GetWeatherForecastsAsync_CancelledMidFlight_Throws()
    {
        using var cts = new CancellationTokenSource(millisecondsDelay: 100);
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => Client.GetWeatherForecastsAsync(cts.Token));
    }
}
