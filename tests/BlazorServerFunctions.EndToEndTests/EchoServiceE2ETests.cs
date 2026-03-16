namespace BlazorServerFunctions.EndToEndTests;

public class EchoServiceE2ETests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly EchoServiceClient _client = new(factory.CreateClient());

    [Fact]
    public async Task GetEchoAsync_QueryStringParameter_RoundTripsCorrectly()
    {
        var result = await _client.GetEchoAsync("hello-world");
        Assert.Equal("hello-world", result);
    }

    [Fact]
    public async Task PostEchoAsync_BodyParameter_RoundTripsCorrectly()
    {
        var result = await _client.PostEchoAsync("hello-post");
        Assert.Equal("hello-post", result);
    }

    [Fact]
    public async Task GetEchoAsync_WithSpecialCharacters_RoundTripsCorrectly()
    {
        var result = await _client.GetEchoAsync("hello world & more");
        Assert.Equal("hello world & more", result);
    }
}
