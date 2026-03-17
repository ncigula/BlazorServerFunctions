namespace BlazorServerFunctions.EndToEndTests;

/// <summary>
/// Server path: resolves IWeatherService directly from DI → WeatherService.
/// Mirrors Blazor Server components which call services in-process without HTTP.
/// </summary>
public sealed class WeatherServiceServerTests(E2EFixture fixture) : IClassFixture<E2EFixture>
{
    [Fact]
    public async Task GetWeatherForecastsAsync_Returns5Forecasts()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var result = await scope.ServiceProvider.GetRequiredService<IWeatherService>()
            .GetWeatherForecastsAsync();
        Assert.Equal(5, result.Length);
    }

    [Fact]
    public async Task GetWeatherForecastsAsync_AllForecastsHaveRequiredFields()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var result = await scope.ServiceProvider.GetRequiredService<IWeatherService>()
            .GetWeatherForecastsAsync();
        Assert.All(result, f =>
        {
            Assert.NotNull(f.Summary);
            Assert.NotEqual(default, f.Date);
        });
    }

    [Fact]
    public async Task GetWeatherForecastsAsync_TemperatureFMatchesFormula()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var result = await scope.ServiceProvider.GetRequiredService<IWeatherService>()
            .GetWeatherForecastsAsync();
        Assert.All(result, f =>
            Assert.Equal(32 + (int)(f.TemperatureC / 0.5556), f.TemperatureF));
    }

    [Fact]
    public async Task GetWeatherForecastsAsync_WithPreCancelledToken_Throws()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => scope.ServiceProvider.GetRequiredService<IWeatherService>()
                .GetWeatherForecastsAsync(cts.Token));
    }

    [Fact]
    public async Task GetWeatherForecastsAsync_CancelledMidFlight_Throws()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        using var cts = new CancellationTokenSource(millisecondsDelay: 100);
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => scope.ServiceProvider.GetRequiredService<IWeatherService>()
                .GetWeatherForecastsAsync(cts.Token));
    }
}

/// <summary>
/// Client path: resolves WeatherServiceClient from DI → HTTP calls to the in-memory server.
/// Exercises the generated client proxy + generated server endpoints together,
/// mirroring Blazor WASM components.
/// </summary>
public sealed class WeatherServiceClientTests(E2EFixture fixture) : IClassFixture<E2EFixture>
{
    private WeatherServiceClient Client =>
        fixture.Factory.Services.GetRequiredService<WeatherServiceClient>();

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

    [Fact]
    public async Task GetWeatherForecastsAsync_WithPreCancelledToken_Throws()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => Client.GetWeatherForecastsAsync(cts.Token));
    }

    [Fact]
    public async Task GetWeatherForecastsAsync_CancelledMidFlight_Throws()
    {
        using var cts = new CancellationTokenSource(millisecondsDelay: 100);
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => Client.GetWeatherForecastsAsync(cts.Token));
    }
}
