// CA1859: Intentionally using interface types to mirror how components inject IAdminService / IWeatherService
#pragma warning disable CA1859
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;

namespace BlazorServerFunctions.EndToEndTests;

/// <summary>
/// Client path: IAdminService (→ AdminServiceClient HTTP proxy) → generated endpoint with RequireAuthorization.
/// Verifies that interface-level auth is enforced on the HTTP endpoint and that
/// non-auth interfaces are unaffected by auth middleware.
/// </summary>
public sealed class AdminServiceClientTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private IAdminService UnauthenticatedClient => new AdminServiceClient(
        factory.WithWebHostBuilder(b =>
            b.ConfigureTestServices(services =>
                services.AddAuthentication(NoOpAuthHandler.SchemeName)
                    .AddScheme<AuthenticationSchemeOptions, NoOpAuthHandler>(
                        NoOpAuthHandler.SchemeName, _ => { })))
            .CreateClient());

    private IAdminService AuthenticatedClient => new AdminServiceClient(
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
        IWeatherService weatherClient = new WeatherServiceClient(
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
