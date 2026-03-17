namespace BlazorServerFunctions.EndToEndTests;

/// <summary>
/// Client path: resolves EchoServiceClient from DI → HTTP calls to the in-memory server.
/// Verifies GET query-string binding and POST body binding in generated endpoints.
/// </summary>
public sealed class EchoServiceClientTests(E2EFixture fixture) : IClassFixture<E2EFixture>
{
    private EchoServiceClient Client =>
        fixture.Factory.Services.GetRequiredService<EchoServiceClient>();

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
