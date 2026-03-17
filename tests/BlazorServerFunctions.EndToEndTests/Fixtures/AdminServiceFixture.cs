using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace BlazorServerFunctions.EndToEndTests;

/// <summary>
/// Fixture for <see cref="AdminServiceClientTests"/>.
/// <para>
/// Spins up two in-memory servers that differ only in their authentication scheme,
/// then builds a client-side DI provider for each — mirroring how a real WASM app
/// would resolve <c>IAdminService</c> from the generated
/// <c>AddServerFunctionClients</c> registration.
/// </para>
/// <list type="bullet">
///   <item><description>
///     <see cref="UnauthClientServices"/> — server uses <see cref="NoOpAuthHandler"/>
///     (returns <c>NoResult</c>), so <c>RequireAuthorization</c> endpoints challenge
///     with a direct 401 rather than redirecting to the login page.
///   </description></item>
///   <item><description>
///     <see cref="AuthClientServices"/> — server uses <see cref="TestAuthHandler"/>
///     (always succeeds), so authorized endpoints are reachable.
///   </description></item>
/// </list>
/// </summary>
public sealed class AdminServiceFixture : IDisposable
{
    private readonly UnauthenticatedServerFactory _unauthFactory;
    private readonly AuthenticatedServerFactory _authFactory;
    private readonly ServiceProvider _unauthClientServices;
    private readonly ServiceProvider _authClientServices;

    /// <summary>Client-side DI provider backed by an unauthenticated server (→ 401 on protected endpoints).</summary>
    public IServiceProvider UnauthClientServices => _unauthClientServices;

    /// <summary>Client-side DI provider backed by an authenticated server (→ 200 on protected endpoints).</summary>
    public IServiceProvider AuthClientServices => _authClientServices;

    public AdminServiceFixture()
    {
        _unauthFactory = new UnauthenticatedServerFactory();
        _ = _unauthFactory.CreateClient();
        _unauthClientServices = E2EFixture.BuildClientServices(_unauthFactory);

        _authFactory = new AuthenticatedServerFactory();
        _ = _authFactory.CreateClient();
        _authClientServices = E2EFixture.BuildClientServices(_authFactory);
    }

    public void Dispose()
    {
        _unauthClientServices.Dispose();
        _authClientServices.Dispose();
        _unauthFactory.Dispose();
        _authFactory.Dispose();
    }

    private sealed class UnauthenticatedServerFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder) =>
            builder.ConfigureTestServices(services =>
                services.AddAuthentication(NoOpAuthHandler.SchemeName)
                    .AddScheme<AuthenticationSchemeOptions, NoOpAuthHandler>(
                        NoOpAuthHandler.SchemeName, _ => { }));
    }

    private sealed class AuthenticatedServerFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder) =>
            builder.ConfigureTestServices(services =>
                services.AddAuthentication(TestAuthHandler.SchemeName)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.SchemeName, _ => { }));
    }
}
