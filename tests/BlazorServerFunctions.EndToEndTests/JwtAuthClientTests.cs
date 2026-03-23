using System.Net;
using System.Text.Json;
using Grpc.Core;
using Grpc.Net.Client;
using BlazorServerFunctions.Sample.Shared;

namespace BlazorServerFunctions.EndToEndTests;

/// <summary>
/// E2E tests for JWT Bearer authentication (§6.5).
/// Verifies that <c>Authorization: Bearer &lt;token&gt;</c> headers are accepted by both the
/// REST endpoints (<see cref="IAdminService"/>) and gRPC endpoints (<see cref="IGrpcDemoService"/>).
/// <para>
/// The same three user states are exercised for both transports:
/// no token / plain user token (no admin role) / admin token.
/// Both use <see cref="BearerTokenHandler"/> to inject the token, demonstrating that the
/// <c>innerGrpcHttpHandler</c> / <c>configureClient</c> injection points work symmetrically.
/// </para>
/// </summary>
public sealed class JwtAuthClientTests(E2EFixture fixture) : IClassFixture<E2EFixture>
{
    private async Task<string> GetTokenAsync(string loginPath)
    {
        var client = fixture.Factory.CreateClient();
        var response = await client
            .PostAsync(new Uri(loginPath, UriKind.Relative), content: null)
            .ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("token").GetString()!;
    }

    // ─── REST tests (IAdminService.GetPolicySecretAsync, Policy = "AdminOnly") ──

    [Fact]
    public async Task GetPolicySecretAsync_NoToken_Returns401()
    {
        using var http = new HttpClient(fixture.Factory.Server.CreateHandler())
        {
            BaseAddress = fixture.Factory.Server.BaseAddress,
        };
        var ex = await Assert.ThrowsAsync<HttpRequestException>(
            () => new AdminServiceClient(http).GetPolicySecretAsync());
        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
    }

    [Fact]
    public async Task GetPolicySecretAsync_PlainUserToken_Returns403()
    {
        var token = await GetTokenAsync("/demos/admin/login");
        using var handler = new BearerTokenHandler(token, fixture.Factory.Server.CreateHandler());
        using var http = new HttpClient(handler) { BaseAddress = fixture.Factory.Server.BaseAddress };
        var ex = await Assert.ThrowsAsync<HttpRequestException>(
            () => new AdminServiceClient(http).GetPolicySecretAsync());
        Assert.Equal(HttpStatusCode.Forbidden, ex.StatusCode);
    }

    [Fact]
    public async Task GetPolicySecretAsync_AdminToken_ReturnsSecret()
    {
        var token = await GetTokenAsync("/demos/admin/login/admin");
        using var handler = new BearerTokenHandler(token, fixture.Factory.Server.CreateHandler());
        using var http = new HttpClient(handler) { BaseAddress = fixture.Factory.Server.BaseAddress };
        var result = await new AdminServiceClient(http).GetPolicySecretAsync();
        Assert.Equal("policy-secret", result);
    }

    // ─── gRPC tests (IGrpcDemoService.GetSecretAsync, Policy = "AdminOnly") ──

    [Fact]
    public async Task GetSecretAsync_NoToken_ThrowsRpcUnauthenticated()
    {
        var channel = GrpcChannel.ForAddress(fixture.Factory.Server.BaseAddress,
            new GrpcChannelOptions { HttpHandler = fixture.Factory.Server.CreateHandler() });
        var ex = await Assert.ThrowsAsync<RpcException>(
            () => new GrpcDemoServiceGrpcClient(channel).GetSecretAsync());
        Assert.Equal(StatusCode.Unauthenticated, ex.StatusCode);
    }

    [Fact]
    public async Task GetSecretAsync_PlainUserToken_ThrowsRpcPermissionDenied()
    {
        var token = await GetTokenAsync("/demos/admin/login");
        using var handler = new BearerTokenHandler(token, fixture.Factory.Server.CreateHandler());
        var channel = GrpcChannel.ForAddress(fixture.Factory.Server.BaseAddress,
            new GrpcChannelOptions { HttpHandler = handler });
        var ex = await Assert.ThrowsAsync<RpcException>(
            () => new GrpcDemoServiceGrpcClient(channel).GetSecretAsync());
        Assert.Equal(StatusCode.PermissionDenied, ex.StatusCode);
    }

    [Fact]
    public async Task GetSecretAsync_AdminToken_ReturnsSecret()
    {
        var token = await GetTokenAsync("/demos/admin/login/admin");
        using var handler = new BearerTokenHandler(token, fixture.Factory.Server.CreateHandler());
        var channel = GrpcChannel.ForAddress(fixture.Factory.Server.BaseAddress,
            new GrpcChannelOptions { HttpHandler = handler });
        var result = await new GrpcDemoServiceGrpcClient(channel).GetSecretAsync();
        Assert.Equal("The secret is: gRPC auth works!", result);
    }
}
