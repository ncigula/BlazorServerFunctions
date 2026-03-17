using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;

namespace BlazorServerFunctions.EndToEndTests;

/// <summary>
/// Server path: resolves IAdminService directly from DI → AdminService.
/// No auth check applies — server-side components already run in an authenticated context.
/// </summary>
public sealed class AdminServiceServerTests(E2EFixture fixture) : IClassFixture<E2EFixture>
{
    [Fact]
    public async Task GetSecretAsync_ReturnsSecret_WithoutAuthCheck()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var result = await scope.ServiceProvider.GetRequiredService<IAdminService>()
            .GetSecretAsync();
        Assert.Equal("top-secret", result);
    }
}

/// <summary>
/// Client path: resolves AdminServiceClient via HTTP → generated endpoint with RequireAuthorization.
/// Verifies that interface-level auth is enforced on the HTTP endpoint and that
/// non-auth interfaces are unaffected by auth middleware.
/// </summary>
public sealed class AdminServiceClientTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private AdminServiceClient UnauthenticatedClient => new(
        factory.WithWebHostBuilder(b =>
            b.ConfigureTestServices(services =>
                services.AddAuthentication(NoOpAuthHandler.SchemeName)
                    .AddScheme<AuthenticationSchemeOptions, NoOpAuthHandler>(
                        NoOpAuthHandler.SchemeName, _ => { })))
            .CreateClient());

    private AdminServiceClient AuthenticatedClient => new(
        factory.WithWebHostBuilder(b =>
            b.ConfigureTestServices(services =>
                services.AddAuthentication(TestAuthHandler.SchemeName)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.SchemeName, _ => { })))
            .CreateClient());

    [Fact]
    public async Task GetSecretAsync_WithoutAuthentication_Returns401()
    {
        var ex = await Assert.ThrowsAsync<HttpRequestException>(
            () => UnauthenticatedClient.GetSecretAsync());
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, ex.StatusCode);
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
        var weatherClient = new WeatherServiceClient(
            factory.WithWebHostBuilder(b =>
                b.ConfigureTestServices(services =>
                    services.AddAuthentication(NoOpAuthHandler.SchemeName)
                        .AddScheme<AuthenticationSchemeOptions, NoOpAuthHandler>(
                            NoOpAuthHandler.SchemeName, _ => { })))
                .CreateClient());
        var result = await weatherClient.GetWeatherForecastsAsync();
        Assert.NotEmpty(result);
    }
}
