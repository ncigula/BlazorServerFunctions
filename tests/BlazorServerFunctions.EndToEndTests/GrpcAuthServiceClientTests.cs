using Grpc.Core;
using Grpc.Net.Client;

namespace BlazorServerFunctions.EndToEndTests;

/// <summary>
/// E2E tests for gRPC auth enforcement (§6.5).
/// Verifies that <c>[Authorize(Policy = "AdminOnly")]</c> on the generated
/// <c>GrpcDemoServiceGrpcService</c> method is enforced at runtime.
/// <para>
/// Mirrors <see cref="AdminServiceClientTests"/>: <c>Factory.CreateClient()</c> gives an
/// <see cref="System.Net.Http.HttpClient"/> with a cookie jar; after posting to the login
/// endpoint the cookie is stored and forwarded on every subsequent request — including gRPC
/// calls, since <see cref="Grpc.Net.Client.GrpcChannelOptions.HttpClient"/> reuses that
/// same client for the underlying HTTP transport.
/// </para>
/// </summary>
public sealed class GrpcAuthServiceClientTests(E2EFixture fixture) : IClassFixture<E2EFixture>
{
    private IGrpcDemoService UnauthenticatedClient =>
        fixture.ClientServices.GetRequiredService<IGrpcDemoService>();

    private async Task<IGrpcDemoService> PlainUserClientAsync()
    {
        var http = fixture.Factory.CreateClient();
        await http.PostAsync(new Uri("/demos/admin/login", UriKind.Relative), content: null).ConfigureAwait(false);
        return new GrpcDemoServiceGrpcClient(
            GrpcChannel.ForAddress(fixture.Factory.Server.BaseAddress, new GrpcChannelOptions { HttpClient = http }));
    }

    private async Task<IGrpcDemoService> AdminUserClientAsync()
    {
        var http = fixture.Factory.CreateClient();
        await http.PostAsync(new Uri("/demos/admin/login/admin", UriKind.Relative), content: null).ConfigureAwait(false);
        return new GrpcDemoServiceGrpcClient(
            GrpcChannel.ForAddress(fixture.Factory.Server.BaseAddress, new GrpcChannelOptions { HttpClient = http }));
    }

    [Fact]
    public async Task GetSecretAsync_Unauthenticated_ThrowsRpcExceptionUnauthenticated()
    {
        var ex = await Assert.ThrowsAsync<RpcException>(
            () => UnauthenticatedClient.GetSecretAsync());
        Assert.Equal(StatusCode.Unauthenticated, ex.StatusCode);
    }

    [Fact]
    public async Task GetSecretAsync_AuthenticatedWithoutAdminRole_ThrowsRpcExceptionPermissionDenied()
    {
        var client = await PlainUserClientAsync();
        var ex = await Assert.ThrowsAsync<RpcException>(
            () => client.GetSecretAsync());
        Assert.Equal(StatusCode.PermissionDenied, ex.StatusCode);
    }

    [Fact]
    public async Task GetSecretAsync_AuthenticatedAsAdmin_ReturnsSecret()
    {
        var client = await AdminUserClientAsync();
        var result = await client.GetSecretAsync();
        Assert.Equal("The secret is: gRPC auth works!", result);
    }
}
