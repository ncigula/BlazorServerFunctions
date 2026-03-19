using System.Net;
using System.Text.Json;

namespace BlazorServerFunctions.EndToEndTests;

/// <summary>
/// E2E tests for role-based auth (<c>Roles = "Admin"</c>), named authorization policies
/// (<c>Policy = "AdminOnly"</c>), and their combination — verifying that the generated
/// <c>.RequireAuthorization(...)</c> calls are correctly enforced by ASP.NET Core at runtime.
/// <para>
/// Auth state is controlled via two cookie-login endpoints in the sample server:
/// <list type="bullet">
///   <item><c>POST /demos/admin/login</c> — plain authenticated user (no roles)</item>
///   <item><c>POST /demos/admin/login/admin</c> — authenticated user with the "Admin" role</item>
/// </list>
/// </para>
/// <para>
/// ProblemDetails verification: the sample server is configured to return
/// <c>application/problem+json</c> bodies on 401 and 403 responses so that the generated
/// client can forward the detail and tests can assert on the body content.
/// </para>
/// </summary>
public sealed class RolesAndPolicyClientTests(E2EFixture fixture) : IClassFixture<E2EFixture>
{
    // Unauthenticated client — no cookie jar interaction.
    private IAdminService UnauthenticatedClient =>
        fixture.ClientServices.GetRequiredService<IAdminService>();

    // Signs in as a plain user (no roles) and returns an AdminServiceClient backed by that session.
    private async Task<IAdminService> PlainUserClientAsync()
    {
        var http = fixture.Factory.CreateClient();
        await http.PostAsync(new Uri("/demos/admin/login", UriKind.Relative), content: null).ConfigureAwait(false);
        return new AdminServiceClient(http);
    }

    // Signs in as a user with the "Admin" role and returns an AdminServiceClient backed by that session.
    private async Task<IAdminService> AdminUserClientAsync()
    {
        var http = fixture.Factory.CreateClient();
        await http.PostAsync(new Uri("/demos/admin/login/admin", UriKind.Relative), content: null).ConfigureAwait(false);
        return new AdminServiceClient(http);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static void AssertProblemDetails(HttpRequestException ex, HttpStatusCode expectedStatus)
    {
        Assert.Equal(expectedStatus, ex.StatusCode);
        // The generated client puts the raw error body in ex.Message — verify it is
        // valid ProblemDetails JSON with the correct status field.
        using var doc = JsonDocument.Parse(ex.Message);
        Assert.Equal((int)expectedStatus, doc.RootElement.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(
            doc.RootElement.GetProperty("title").GetString()));
    }

    // ── Role-based auth: [ServerFunction(Roles = "Admin")] ───────────────────

    [Fact]
    public async Task GetRoleSecretAsync_Unauthenticated_Returns401WithProblemDetails()
    {
        var ex = await Assert.ThrowsAsync<HttpRequestException>(
            () => UnauthenticatedClient.GetRoleSecretAsync());
        AssertProblemDetails(ex, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetRoleSecretAsync_AuthenticatedWithoutRole_Returns403WithProblemDetails()
    {
        var client = await PlainUserClientAsync();
        var ex = await Assert.ThrowsAsync<HttpRequestException>(
            () => client.GetRoleSecretAsync());
        AssertProblemDetails(ex, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetRoleSecretAsync_AuthenticatedWithAdminRole_ReturnsData()
    {
        var client = await AdminUserClientAsync();
        var result = await client.GetRoleSecretAsync();
        Assert.Equal("admin-role-secret", result);
    }

    // ── Named policy: [ServerFunction(Policy = "AdminOnly")] ─────────────────

    [Fact]
    public async Task GetPolicySecretAsync_Unauthenticated_Returns401WithProblemDetails()
    {
        var ex = await Assert.ThrowsAsync<HttpRequestException>(
            () => UnauthenticatedClient.GetPolicySecretAsync());
        AssertProblemDetails(ex, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPolicySecretAsync_AuthenticatedPolicyNotSatisfied_Returns403WithProblemDetails()
    {
        // "AdminOnly" policy requires the "Admin" role — plain user does not have it.
        var client = await PlainUserClientAsync();
        var ex = await Assert.ThrowsAsync<HttpRequestException>(
            () => client.GetPolicySecretAsync());
        AssertProblemDetails(ex, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPolicySecretAsync_AuthenticatedPolicySatisfied_ReturnsData()
    {
        var client = await AdminUserClientAsync();
        var result = await client.GetPolicySecretAsync();
        Assert.Equal("policy-secret", result);
    }

    // ── Role + Policy combined: [ServerFunction(Roles = "Admin", Policy = "AdminOnly")] ──

    [Fact]
    public async Task GetRoleAndPolicySecretAsync_Unauthenticated_Returns401WithProblemDetails()
    {
        var ex = await Assert.ThrowsAsync<HttpRequestException>(
            () => UnauthenticatedClient.GetRoleAndPolicySecretAsync());
        AssertProblemDetails(ex, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetRoleAndPolicySecretAsync_AuthenticatedWithoutRole_Returns403WithProblemDetails()
    {
        // Both Roles and Policy require "Admin" — plain user satisfies neither.
        var client = await PlainUserClientAsync();
        var ex = await Assert.ThrowsAsync<HttpRequestException>(
            () => client.GetRoleAndPolicySecretAsync());
        AssertProblemDetails(ex, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetRoleAndPolicySecretAsync_AuthenticatedWithAdminRole_ReturnsData()
    {
        // Admin user satisfies both the Roles constraint and the "AdminOnly" policy.
        var client = await AdminUserClientAsync();
        var result = await client.GetRoleAndPolicySecretAsync();
        Assert.Equal("role-and-policy-secret", result);
    }
}
