using System.Net;
using System.Net.Http.Json;
using BlazorServerFunctions.Sample.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorServerFunctions.EndToEndTests;

[Collection("E2E")]
public sealed class ExplicitBindingServiceClientTests(E2EFixture fixture)
{
    private IExplicitBindingService Client =>
        fixture.ClientServices.GetRequiredService<IExplicitBindingService>();

    // ── Auto ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetItemAsync_Auto_RouteAndQueryRoundTrip()
    {
        var result = await Client.GetItemAsync(42, "active");
        Assert.Equal("id=42 filter=active", result);
    }

    // ── ParameterSource.Route ─────────────────────────────────────────────────

    /// <summary>
    /// Sends a raw DELETE to verify the explicit Route marker produces a working endpoint.
    /// </summary>
    [Fact]
    public async Task DeleteItemAsync_ExplicitRoute_Returns200()
    {
        using var client = fixture.Factory.CreateClient();
        var response = await client.DeleteAsync(
            new Uri("/api/v1/explicit-binding/99", UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── ParameterSource.Header ────────────────────────────────────────────────

    [Fact]
    public async Task CreateOrderAsync_HeaderParam_RoundTrips()
    {
        var result = await Client.CreateOrderAsync("tenant-abc", "prod-1", 5);
        Assert.Equal("tenant=tenant-abc product=prod-1 qty=5", result);
    }

    /// <summary>
    /// Raw HTTP proof: the X-Tenant-Id header value is read by the server endpoint,
    /// confirming the generator emitted [FromHeader] on the server and
    /// requestMessage.Headers.Add on the client.
    /// </summary>
    [Fact]
    public async Task CreateOrderAsync_HeaderActuallyReadFromHttpHeader()
    {
        using var client = fixture.Factory.CreateClient();
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            new Uri("/api/v1/explicit-binding/create-order-async", UriKind.Relative));
        request.Headers.Add("X-Tenant-Id", "raw-tenant");
        request.Content = JsonContent.Create(new { ProductId = "prod-raw", Quantity = 3 });

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("raw-tenant", body, StringComparison.Ordinal);
    }

    // ── ParameterSource.Query on POST ─────────────────────────────────────────

    [Fact]
    public async Task SearchAsync_ExplicitQueryOnPost_RoundTrips()
    {
        var result = await Client.SearchAsync(page: 2, pageSize: 25, query: "hello");
        Assert.Equal("page=2 size=25 query=hello", result);
    }

    /// <summary>
    /// Raw HTTP proof: page/pageSize are read from the URL query string (not the body),
    /// confirming the [FromQuery] split on the server and correct URL construction on the client.
    /// </summary>
    [Fact]
    public async Task SearchAsync_PageParamsActuallyInQueryString()
    {
        using var client = fixture.Factory.CreateClient();
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            new Uri("/api/v1/explicit-binding/search-async?Page=3&PageSize=10", UriKind.Relative));
        request.Content = JsonContent.Create(new { Query = "test" });

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("page=3", body, StringComparison.Ordinal);
        Assert.Contains("size=10", body, StringComparison.Ordinal);
        Assert.Contains("test", body, StringComparison.Ordinal);
    }

}
