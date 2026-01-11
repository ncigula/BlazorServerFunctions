using System.Net;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BlazorServerFunctions.IntegrationTests;

public sealed class IntegrationTest1
{
    [Fact]
    public async Task GetWebResourceRootReturnsOkStatusCode()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.BlazorServerFunctions_Sample_AppHost>(TestContext.Current.CancellationToken);
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder => { clientBuilder.AddStandardResilienceHandler(); });
        // To output logs to the xUnit.net ITestOutputHelper, consider adding a package from https://www.nuget.org/packages?q=xunit+logging

        await using var app = await appHost.BuildAsync(TestContext.Current.CancellationToken);
        var resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
        await app.StartAsync(TestContext.Current.CancellationToken);

        // Act
        using var httpClient = app.CreateHttpClient("sample");

        await resourceNotificationService.WaitForResourceAsync(
                "sample",
                KnownResourceStates.Running,
                TestContext.Current.CancellationToken)
            .WaitAsync(
                TimeSpan.FromSeconds(30),
                TestContext.Current.CancellationToken);

#pragma warning disable CA2234
        var response = await httpClient.GetAsync("/", TestContext.Current.CancellationToken);
#pragma warning restore CA2234

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}