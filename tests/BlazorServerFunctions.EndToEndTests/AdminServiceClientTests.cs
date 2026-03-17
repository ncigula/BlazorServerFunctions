using System.Net;

namespace BlazorServerFunctions.EndToEndTests;

/// <summary>
/// Client path: IAdminService → AdminServiceClient (HTTP proxy) → generated endpoint
/// with RequireAuthorization.
/// <para>
/// Uses a single <see cref="E2EFixture"/> (one server, real cookie auth). Auth state is
/// determined purely by what credentials the HttpClient sends — mirroring real WASM usage
/// where a delegating handler attaches a JWT token or cookie to each request.
/// </para>
/// </summary>
public sealed class AdminServiceClientTests(E2EFixture fixture) : IClassFixture<E2EFixture>
{
    // No credentials → server's OnRedirectToLogin returns 401 directly (not a redirect).
    private IAdminService UnauthenticatedClient =>
        fixture.ClientServices.GetRequiredService<IAdminService>();

    [Fact]
    public async Task GetSecretAsync_WithoutAuthentication_Returns401()
    {
        var ex = await Assert.ThrowsAsync<HttpRequestException>(
            () => UnauthenticatedClient.GetSecretAsync());
        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
    }

    [Fact]
    public async Task GetSecretAsync_WithAuthentication_ReturnsSecret()
    {
        // CreateClient() produces an HttpClient with HandleCookies = true by default.
        // POST /demos/admin/login signs the user in; the cookie is stored in the
        // client's cookie jar and sent automatically on all subsequent requests —
        // exactly how a browser (or Blazor WASM runtime) handles auth cookies.
        var client = fixture.Factory.CreateClient();
        await client.PostAsync(new Uri("/demos/admin/login", UriKind.Relative), content: null);
        var result = await new AdminServiceClient(client).GetSecretAsync();
        Assert.Equal("top-secret", result);
    }

    [Fact]
    public async Task WeatherEndpoint_WithoutAuthentication_StillAccessible()
    {
        // Verifies that auth on IAdminService does not bleed onto unprotected interfaces.
        var weatherClient = fixture.ClientServices.GetRequiredService<IWeatherService>();
        var result = await weatherClient.GetWeatherForecastsAsync();
        Assert.NotEmpty(result);
    }
}
