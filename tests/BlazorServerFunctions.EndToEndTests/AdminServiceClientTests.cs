using System.Net;

namespace BlazorServerFunctions.EndToEndTests;

/// <summary>
/// Client path: resolves IAdminService from DI → AdminServiceClient (HTTP proxy) →
/// generated endpoint with RequireAuthorization.
/// <para>
/// Uses <see cref="AdminServiceFixture"/> which spins up two in-memory servers — one
/// unauthenticated and one authenticated — and exposes a client-side
/// <see cref="IServiceProvider"/> for each, mirroring how a real WASM component
/// injects <c>IAdminService</c> via the generated <c>AddServerFunctionClients</c>.
/// </para>
/// </summary>
public sealed class AdminServiceClientTests(AdminServiceFixture fixture) : IClassFixture<AdminServiceFixture>
{
    private IAdminService UnauthenticatedClient =>
        fixture.UnauthClientServices.GetRequiredService<IAdminService>();

    private IAdminService AuthenticatedClient =>
        fixture.AuthClientServices.GetRequiredService<IAdminService>();

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
        var result = await AuthenticatedClient.GetSecretAsync();
        Assert.Equal("top-secret", result);
    }

    [Fact]
    public async Task WeatherEndpoint_WithoutAuthentication_StillAccessible()
    {
        // Verifies that auth on IAdminService does not bleed onto unprotected interfaces.
        var weatherClient = fixture.UnauthClientServices.GetRequiredService<IWeatherService>();
        var result = await weatherClient.GetWeatherForecastsAsync();
        Assert.NotEmpty(result);
    }
}
