using BlazorServerFunctions.Sample.Shared;
using Grpc.Net.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorServerFunctions.EndToEndTests;

/// <summary>
/// Shared fixture for client-path tests.
/// <para>
/// Spins up the in-memory test server (bare, no DI overrides) and builds a
/// <em>separate</em> <see cref="ClientServices"/> provider that holds the generated
/// HTTP-proxy classes wired back through the in-memory server's handler.
/// </para>
/// <para>
/// Keeping client registrations in a separate provider avoids the recursive-loop
/// failure that occurs when <c>ConfigureTestServices</c> replaces
/// <c>IWeatherService → WeatherService</c> with <c>IWeatherService → WeatherServiceClient</c>
/// in the test server itself: the server-side endpoint handler would resolve the
/// HTTP proxy, fire a request back into the same server, and loop indefinitely.
/// </para>
/// </summary>
public sealed class E2EFixture : IDisposable
{
    private readonly ServiceProvider _clientServices;

    public WebApplicationFactory<Program> Factory { get; }

    /// <summary>
    /// Client-side service provider: <c>IWeatherService → WeatherServiceClient</c>, etc.
    /// Use this in <em>Client</em> test classes; do not use <c>Factory.Services</c>
    /// (that provider has the real server-side implementations).
    /// </summary>
    public IServiceProvider ClientServices => _clientServices;

    public E2EFixture()
    {
        Factory = new WebApplicationFactory<Program>();

        // Touch the server so that Server.BaseAddress and CreateHandler() are ready.
        _ = Factory.CreateClient();

        _clientServices = BuildClientServices(Factory);
    }

    /// <summary>
    /// Builds a standalone client-side DI container wired to the given factory's
    /// in-memory server.  Reused by <see cref="AdminServiceFixture"/> so that
    /// all client registrations stay in one place.
    /// </summary>
    internal static ServiceProvider BuildClientServices(WebApplicationFactory<Program> factory)
    {
        var services = new ServiceCollection();

        services.AddHttpClient<IWeatherService, WeatherServiceClient>()
            .ConfigurePrimaryHttpMessageHandler(() => factory.Server.CreateHandler())
            .ConfigureHttpClient((_, c) => c.BaseAddress = factory.Server.BaseAddress);

        services.AddHttpClient<IAdminService, AdminServiceClient>()
            .ConfigurePrimaryHttpMessageHandler(() => factory.Server.CreateHandler())
            .ConfigureHttpClient((_, c) => c.BaseAddress = factory.Server.BaseAddress);

        services.AddHttpClient<IEchoService, EchoServiceClient>()
            .ConfigurePrimaryHttpMessageHandler(() => factory.Server.CreateHandler())
            .ConfigureHttpClient((_, c) => c.BaseAddress = factory.Server.BaseAddress);

        services.AddHttpClient<ICrudService, CrudServiceClient>()
            .ConfigurePrimaryHttpMessageHandler(() => factory.Server.CreateHandler())
            .ConfigureHttpClient((_, c) => c.BaseAddress = factory.Server.BaseAddress);

        services.AddHttpClient<IRouteParamService, RouteParamServiceClient>()
            .ConfigurePrimaryHttpMessageHandler(() => factory.Server.CreateHandler())
            .ConfigureHttpClient((_, c) => c.BaseAddress = factory.Server.BaseAddress);

        services.AddHttpClient<IStreamingService, StreamingServiceClient>()
            .ConfigurePrimaryHttpMessageHandler(() => factory.Server.CreateHandler())
            .ConfigureHttpClient((_, c) => c.BaseAddress = factory.Server.BaseAddress);

        services.AddHttpClient<ICacheableService, CacheableServiceClient>()
            .ConfigurePrimaryHttpMessageHandler(() => factory.Server.CreateHandler())
            .ConfigureHttpClient((_, c) => c.BaseAddress = factory.Server.BaseAddress);

        services.AddHttpClient<IRateLimitedService, RateLimitedServiceClient>()
            .ConfigurePrimaryHttpMessageHandler(() => factory.Server.CreateHandler())
            .ConfigureHttpClient((_, c) => c.BaseAddress = factory.Server.BaseAddress);

        services.AddHttpClient<IFileUploadService, FileUploadServiceClient>()
            .ConfigurePrimaryHttpMessageHandler(() => factory.Server.CreateHandler())
            .ConfigureHttpClient((_, c) => c.BaseAddress = factory.Server.BaseAddress);

        services.AddHttpClient<IResultDemoService, ResultDemoServiceClient>()
            .ConfigurePrimaryHttpMessageHandler(() => factory.Server.CreateHandler())
            .ConfigureHttpClient((_, c) => c.BaseAddress = factory.Server.BaseAddress);

        var grpcChannel = GrpcChannel.ForAddress(
            factory.Server.BaseAddress,
            new GrpcChannelOptions { HttpHandler = factory.Server.CreateHandler() });
        services.AddSingleton(grpcChannel);
        services.AddTransient<IGrpcDemoService, GrpcDemoServiceGrpcClient>();

        return services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _clientServices.Dispose();
        Factory.Dispose();
    }
}
