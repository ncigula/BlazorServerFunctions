using System.Collections.Generic;
using System.Net;

namespace BlazorServerFunctions.EndToEndTests;

/// <summary>
/// Verifies that IAsyncEnumerable&lt;T&gt; streaming works end-to-end:
/// the generated server endpoint streams items via chunked JSON and the generated
/// client proxy collects them via ReadFromJsonAsAsyncEnumerable.
/// </summary>
public sealed class WeatherServiceStreamingTests(E2EFixture fixture) : IClassFixture<E2EFixture>
{
    private IWeatherService Client =>
        fixture.ClientServices.GetRequiredService<IWeatherService>();

    // ── Proxy round-trips ────────────────────────────────────────────────────

    [Fact]
    public async Task StreamForecastsAsync_Returns5Items()
    {
        var items = new List<WeatherForecastDto>();
        await foreach (var item in Client.StreamForecastsAsync())
            items.Add(item);

        Assert.Equal(5, items.Count);
    }

    [Fact]
    public async Task StreamForecastsAsync_AllItemsHaveRequiredFields()
    {
        await foreach (var item in Client.StreamForecastsAsync())
        {
            Assert.NotNull(item.Summary);
            Assert.NotEqual(default, item.Date);
        }
    }

    [Fact]
    public async Task StreamForecastsAsync_TemperatureFMatchesFormula()
    {
        await foreach (var item in Client.StreamForecastsAsync())
            Assert.Equal(32 + (int)(item.TemperatureC / 0.5556), item.TemperatureF);
    }

    // ── Raw HTTP — verify endpoint is reachable ───────────────────────────────

    [Fact]
    public async Task Get_StreamForecastsAsync_Endpoint_Returns200()
    {
        using var client = fixture.Factory.CreateClient();
        var response = await client.GetAsync(new Uri("/api/functions/weatherservice/StreamForecastsAsync", UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // Both cancellation tests are skipped for the HTTP client path: the TestHost in .NET 10
    // has a recursive CancellationToken propagation issue that triggers for tokens passed to
    // HTTP requests. Cancellation behaviour is validated by the WeatherService implementation.
    [Fact(Skip = "TestHost recursive cancellation in .NET 10 preview — covered by server-path test")]
    public async Task StreamForecastsAsync_WithPreCancelledToken_Throws()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
#pragma warning disable S108 // body intentionally empty — pre-cancelled token throws before yielding any item
            await foreach (var _ in Client.StreamForecastsAsync(cts.Token).ConfigureAwait(false))
            {
            }
#pragma warning restore S108
        });
    }
}
