namespace BlazorServerFunctions.EndToEndTests;

public class WeatherServiceE2ETests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WeatherServiceClient _client = new(factory.CreateClient());

    [Fact]
    public async Task GetWeatherForecastsAsync_ReturnsExpectedCount()
    {
        var result = await _client.GetWeatherForecastsAsync();
        Assert.Equal(5, result.Length);
    }

    [Fact]
    public async Task GetWeatherForecastsAsync_AllForecastsHaveRequiredFields()
    {
        var result = await _client.GetWeatherForecastsAsync();
        Assert.All(result, f =>
        {
            Assert.NotNull(f.Summary);
            Assert.NotEqual(default, f.Date);
        });
    }

    [Fact]
    public async Task GetWeatherForecastsAsync_TemperatureFMatchesFormula()
    {
        var result = await _client.GetWeatherForecastsAsync();
        Assert.All(result, f =>
            Assert.Equal(32 + (int)(f.TemperatureC / 0.5556), f.TemperatureF));
    }

    [Fact]
    public async Task GetWeatherForecastsAsync_WithDefaultCancellationToken_Succeeds()
    {
        var result = await _client.GetWeatherForecastsAsync(CancellationToken.None);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetWeatherForecastsAsync_WithPreCancelledToken_ThrowsImmediately()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => _client.GetWeatherForecastsAsync(cts.Token));
    }

    [Fact]
    public async Task GetWeatherForecastsAsync_CancelledMidFlight_ThrowsTaskCanceledException()
    {
        using var cts = new CancellationTokenSource(millisecondsDelay: 100);
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => _client.GetWeatherForecastsAsync(cts.Token));
    }
}
