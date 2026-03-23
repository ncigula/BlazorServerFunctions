using System.Net;
using System.Net.Http.Json;

namespace BlazorServerFunctions.EndToEndTests;

/// <summary>
/// Verifies that <see cref="SampleApiConfig"/> is applied correctly at compile time,
/// producing routes under <c>/api/v1/echoservice/</c> with kebab-case method segments
/// instead of the default <c>/api/functions/echoservice/</c> PascalCase segments.
/// </summary>
[Collection("E2E")]
public sealed class EchoServiceConfigTests(E2EFixture fixture)
{
    // ── Client proxy (via generated EchoServiceClient) ──────────────────────

    [Fact]
    public async Task GetEchoAsync_ConfiguredProxy_RoundTripsViaKebabCaseRoute()
    {
        // The generated EchoServiceClient should now target /api/v1/echoservice/get-echo-async
        var echoService = fixture.ClientServices.GetRequiredService<IEchoService>();
        var result = await echoService.GetEchoAsync("config-works");
        Assert.Equal("config-works", result);
    }

    [Fact]
    public async Task PostEchoAsync_ConfiguredProxy_RoundTripsViaKebabCaseRoute()
    {
        var echoService = fixture.ClientServices.GetRequiredService<IEchoService>();
        var result = await echoService.PostEchoAsync("post-config-works");
        Assert.Equal("post-config-works", result);
    }

    // ── Raw HTTP — new routes are reachable ──────────────────────────────────

    [Fact]
    public async Task Get_KebabCaseRoute_Returns200()
    {
        using var client = fixture.Factory.CreateClient();
        var response = await client.GetAsync(new Uri("/api/v1/echoservice/get-echo-async?message=hello", UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<string>();
        Assert.Equal("hello", body);
    }

    [Fact]
    public async Task Post_KebabCaseRoute_Returns200()
    {
        using var client = fixture.Factory.CreateClient();
        // The generated server endpoint expects { Message: "..." } — same format the client proxy sends.
        var response = await client.PostAsJsonAsync("/api/v1/echoservice/post-echo-async", new { Message = "hello-post" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<string>();
        Assert.Equal("hello-post", body);
    }

    // ── Raw HTTP — old (default) routes are no longer registered ────────────

    [Fact]
    public async Task Get_DefaultPascalCaseRoute_Returns404()
    {
        using var client = fixture.Factory.CreateClient();
        var response = await client.GetAsync(new Uri("/api/functions/echoservice/GetEchoAsync?message=hello", UriKind.Relative));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
