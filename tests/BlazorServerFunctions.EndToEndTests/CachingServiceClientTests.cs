namespace BlazorServerFunctions.EndToEndTests;

/// <summary>
/// §3.5 Output Caching — verifies that GET endpoints annotated with CacheSeconds
/// return stale responses while the cache is valid, and fresh responses once it expires.
/// </summary>
[Collection("E2E")]
public sealed class CachingServiceClientTests(E2EFixture fixture)
{
    private ICacheableService Client =>
        fixture.ClientServices.GetRequiredService<ICacheableService>();

    [Fact]
    public async Task IncrementAsync_ReturnsIncrementedCount()
    {
        var before = await Client.GetCountAsync();
        var after = await Client.IncrementAsync();
        Assert.True(after > before, "IncrementAsync should return a value greater than the previous count.");
    }

    [Fact]
    public async Task GetCountAsync_ReturnsStaleCachedValue_WhileCacheIsValid()
    {
        // Read once to populate the cache, then increment and read again within the 1-second window.
        // The second GET should return the cached (pre-increment) value.
        var initial = await Client.GetCountAsync();
        await Client.IncrementAsync();
        var cachedRead = await Client.GetCountAsync();

        Assert.Equal(initial, cachedRead);
    }

    [Fact]
    public async Task GetCountAsync_ReturnsFreshValue_AfterCacheExpires()
    {
        // Increment to set a new server value, wait for the 1-second cache to expire,
        // then verify the GET reflects the updated count.
        var afterIncrement = await Client.IncrementAsync();

        await Task.Delay(TimeSpan.FromSeconds(1.5));

        var freshRead = await Client.GetCountAsync();
        Assert.Equal(afterIncrement, freshRead);
    }
}
