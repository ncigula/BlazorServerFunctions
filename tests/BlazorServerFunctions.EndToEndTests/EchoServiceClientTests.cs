namespace BlazorServerFunctions.EndToEndTests;

/// <summary>
/// Client path: resolves IEchoService from DI → EchoServiceClient (HTTP proxy) → in-memory server.
/// Verifies GET query-string binding and POST body binding in generated endpoints.
/// </summary>
[Collection("E2E")]
public sealed class EchoServiceClientTests(E2EFixture fixture)
{
    private IEchoService Client =>
        fixture.ClientServices.GetRequiredService<IEchoService>();

    [Fact]
    public async Task GetEchoAsync_QueryStringParameter_RoundTrips()
    {
        var result = await Client.GetEchoAsync("hello-world");
        Assert.Equal("hello-world", result);
    }

    [Fact]
    public async Task PostEchoAsync_BodyParameter_RoundTrips()
    {
        var result = await Client.PostEchoAsync("hello-post");
        Assert.Equal("hello-post", result);
    }

    [Fact]
    public async Task GetEchoAsync_WithSpecialCharacters_RoundTrips()
    {
        var result = await Client.GetEchoAsync("hello world & more");
        Assert.Equal("hello world & more", result);
    }
}
