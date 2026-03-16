using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorServerFunctions.EndToEndTests;

public class AuthorizationE2ETests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly AdminServiceClient _unauthenticatedClient = new(
        factory.WithWebHostBuilder(b =>
            b.ConfigureTestServices(services =>
                services.AddAuthentication(NoOpAuthHandler.SchemeName)
                    .AddScheme<AuthenticationSchemeOptions, NoOpAuthHandler>(
                        NoOpAuthHandler.SchemeName, _ => { })))
            .CreateClient());

    private readonly AdminServiceClient _authenticatedClient = new(
        factory.WithWebHostBuilder(b =>
            b.ConfigureTestServices(services =>
                services.AddAuthentication(TestAuthHandler.SchemeName)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.SchemeName, _ => { })))
            .CreateClient());

    [Fact]
    public async Task GetSecretAsync_WithoutAuthentication_Throws401()
    {
        var ex = await Assert.ThrowsAsync<HttpRequestException>(
            () => _unauthenticatedClient.GetSecretAsync());
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, ex.StatusCode);
    }

    [Fact]
    public async Task GetSecretAsync_WithAuthentication_ReturnsSecret()
    {
        var result = await _authenticatedClient.GetSecretAsync();
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
