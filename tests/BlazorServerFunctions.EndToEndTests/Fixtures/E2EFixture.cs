using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorServerFunctions.EndToEndTests;

/// <summary>
/// Shared fixture that adds the generated client proxy classes as typed HttpClients
/// wired back to the same in-memory server, so both server-path and client-path
/// tests can resolve from the same <see cref="WebApplicationFactory{TEntryPoint}"/>.
/// </summary>
public sealed class E2EFixture : IDisposable
{
    public WebApplicationFactory<Program> Factory { get; }

    public E2EFixture()
    {
        // Capture self so the lambdas below can lazily reference Server after it starts.
        // CA2000 suppressed: factory ownership is transferred to Factory and disposed in Dispose().
#pragma warning disable CA2000
        WebApplicationFactory<Program>? self = null;
        self = new WebApplicationFactory<Program>()
#pragma warning restore CA2000
            .WithWebHostBuilder(b => b.ConfigureTestServices(services =>
            {
                services.AddHttpClient<WeatherServiceClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => self!.Server.CreateHandler())
                    .ConfigureHttpClient((_, c) => c.BaseAddress = self!.Server.BaseAddress);

                services.AddHttpClient<AdminServiceClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => self!.Server.CreateHandler())
                    .ConfigureHttpClient((_, c) => c.BaseAddress = self!.Server.BaseAddress);

                services.AddHttpClient<EchoServiceClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => self!.Server.CreateHandler())
                    .ConfigureHttpClient((_, c) => c.BaseAddress = self!.Server.BaseAddress);

                services.AddHttpClient<CrudServiceClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => self!.Server.CreateHandler())
                    .ConfigureHttpClient((_, c) => c.BaseAddress = self!.Server.BaseAddress);
            }));

        Factory = self;
    }

    public void Dispose() => Factory.Dispose();
}
