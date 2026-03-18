using System.Net;

namespace BlazorServerFunctions.EndToEndTests;

/// <summary>
/// Verifies that route parameters are correctly bound on the server and interpolated
/// in the client URL for all HTTP verbs that use them.
/// </summary>
public sealed class RouteParamServiceClientTests(E2EFixture fixture) : IClassFixture<E2EFixture>
{
    private IRouteParamService Client =>
        fixture.ClientServices.GetRequiredService<IRouteParamService>();

    // ── Proxy round-trips ────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_RouteParam_RoundTrips()
    {
        var result = await Client.GetByIdAsync(42);
        Assert.Equal("item-42", result);
    }

    [Fact]
    public async Task DeleteByIdAsync_RouteParam_Succeeds()
    {
        var exception = await Record.ExceptionAsync(() => Client.DeleteByIdAsync(7));
        Assert.Null(exception);
    }

    [Fact]
    public async Task UpdateAsync_RouteParamPlusBody_RoundTrips()
    {
        var result = await Client.UpdateAsync(3, "hello");
        Assert.Equal("updated-3-hello", result);
    }

    [Fact]
    public async Task GetTagsAsync_RouteParamPlusQueryString_RoundTrips()
    {
        var result = await Client.GetTagsAsync(id: 5, page: 2);
        Assert.Equal("tags-5-page-2", result);
    }

    // ── Raw HTTP — verify routes are reachable ────────────────────────────────

    [Fact]
    public async Task Get_RouteParam_Returns200()
    {
        using var client = fixture.Factory.CreateClient();
        var response = await client.GetAsync(new Uri("/api/functions/route-params/99", UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Delete_RouteParam_Returns200()
    {
        using var client = fixture.Factory.CreateClient();
        var response = await client.DeleteAsync(new Uri("/api/functions/route-params/1", UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_RouteParamWithQueryString_Returns200()
    {
        using var client = fixture.Factory.CreateClient();
        var response = await client.GetAsync(new Uri("/api/functions/route-params/5/tags?Page=1", UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
