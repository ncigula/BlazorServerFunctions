using System.Net.Http;
using BlazorServerFunctions.Sample.Client;

namespace BlazorServerFunctions.EndToEndTests;

/// <summary>
/// Verifies that the <c>configureClient</c> parameter of the generated
/// <c>AddServerFunctionClients</c> extension method actually wires delegating
/// handlers into the HTTP pipeline used by every registered client proxy.
/// </summary>
[Collection("E2E")]
public sealed class ConfigureClientTests(E2EFixture fixture)
{
    [Fact]
    public async Task ConfigureClient_DelegatingHandler_IsInvokedForEveryRequest()
    {
        // Arrange — build a fresh DI container using the generated AddServerFunctionClients,
        // wiring the test server handler and a call-counting delegating handler via configureClient.
        var counter = new CallCounter();
        var services = new ServiceCollection();

        services.AddServerFunctionClients(
            baseAddress: fixture.Factory.Server.BaseAddress,
            configureClient: b => b
                .ConfigurePrimaryHttpMessageHandler(() => fixture.Factory.Server.CreateHandler())
                .AddHttpMessageHandler(() => new CallCountingHandler(counter)));

        await using var provider = services.BuildServiceProvider();
        var weatherService = provider.GetRequiredService<IWeatherService>();

        // Act
        await weatherService.GetWeatherForecastsAsync();

        // Assert — the delegating handler was called exactly once
        Assert.Equal(1, counter.Count);
    }

    [Fact]
    public async Task ConfigureClient_DelegatingHandler_IsInvokedForEachInterface()
    {
        // Arrange — verifies that configureClient is applied to every AddHttpClient
        // registration, not just the first one.
        var counter = new CallCounter();
        var services = new ServiceCollection();

        services.AddServerFunctionClients(
            baseAddress: fixture.Factory.Server.BaseAddress,
            configureClient: b => b
                .ConfigurePrimaryHttpMessageHandler(() => fixture.Factory.Server.CreateHandler())
                .AddHttpMessageHandler(() => new CallCountingHandler(counter)));

        await using var provider = services.BuildServiceProvider();

        // Act — one request per registered interface
        await provider.GetRequiredService<IWeatherService>().GetWeatherForecastsAsync();
        await provider.GetRequiredService<IEchoService>().GetEchoAsync("ping");

        // Assert — both pipelines ran through the handler
        Assert.Equal(2, counter.Count);
    }

    private sealed class CallCounter
    {
        public int Count { get; private set; }
        public void Increment() => Count++;
    }

    private sealed class CallCountingHandler(CallCounter counter) : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            counter.Increment();
            return base.SendAsync(request, cancellationToken);
        }
    }
}
