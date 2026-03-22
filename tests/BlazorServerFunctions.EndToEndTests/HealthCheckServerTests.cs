using System.Net;

namespace BlazorServerFunctions.EndToEndTests;

/// <summary>
/// Server path: verifies that <c>AddServerFunctionHealthChecks()</c> registers a working health
/// check for every BSF-managed service, and that <c>MapServerFunctionHealthChecks()</c> exposes
/// them at <c>/health/server-functions</c>.
/// </summary>
public sealed class HealthCheckServerTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task HealthEndpoint_AllServicesResolvable_ReturnsHealthy()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync(
            new Uri("/health/server-functions", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HealthEndpoint_IsAvailableWithoutAuthentication()
    {
        // Health endpoints must be accessible by infrastructure probes without credentials.
        var client = factory.CreateClient(
            new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

        var response = await client.GetAsync(
            new Uri("/health/server-functions", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
