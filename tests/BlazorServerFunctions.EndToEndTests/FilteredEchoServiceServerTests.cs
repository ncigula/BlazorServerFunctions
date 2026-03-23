using System.Net;
using BlazorServerFunctions.Sample.Components.Filter;

namespace BlazorServerFunctions.EndToEndTests;

/// <summary>
/// Server path: verifies that the <c>.AddEndpointFilter&lt;T&gt;()</c> call generated for
/// <c>[ServerFunction(Filters = new[] { typeof(T) })]</c> is actually wired up and executed
/// by ASP.NET Core at runtime.
/// <para>
/// <see cref="AddCorrelationHeaderFilter"/> adds an <c>X-Correlation-Id</c> response header —
/// a directly observable side-effect we can assert on without any browser or token ceremony.
/// </para>
/// </summary>
[Collection("Server")]
public sealed class FilteredEchoServiceServerTests(WebApplicationFactory<Program> factory)
   
{
    [Fact]
    public async Task EchoAsync_ResponseContainsCorrelationHeader()
    {
        // The generated endpoint has .AddEndpointFilter<AddCorrelationHeaderFilter>().
        // The filter runs and adds X-Correlation-Id to every response.
        var client = factory.CreateClient();
        var response = await client.GetAsync(
            new Uri("/api/functions/filtered/EchoAsync?message=hello", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Correlation-Id"),
            "Expected X-Correlation-Id header set by AddCorrelationHeaderFilter.");
    }

    [Fact]
    public async Task EchoAsync_ServiceResolvesFromDi_WithoutHttpLayer()
    {
        // Confirms the service implementation is registered and the interface is
        // wired to FilteredEchoService — endpoint filters only run over HTTP.
        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IFilteredEchoService>();
        var result = await service.EchoAsync("hello");
        Assert.Equal("hello", result);
    }
}
