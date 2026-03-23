using System.Net;
using System.Net.Http.Json;

namespace BlazorServerFunctions.EndToEndTests;

/// <summary>
/// Server path: verifies that the <c>.ValidateAntiforgery()</c> call generated for
/// <c>[ServerFunction(RequireAntiForgery = true)]</c> is actually enforced by ASP.NET Core
/// at runtime.
/// <para>
/// We cannot obtain a valid antiforgery token in a plain <see cref="HttpClient"/> test
/// (no browser cookie round-trip), so we validate the <em>rejection</em> path: a raw
/// POST without a token must be rejected with <c>400 Bad Request</c> — proving that
/// <c>.ValidateAntiforgery()</c> is wired up on the endpoint.
/// </para>
/// </summary>
[Collection("Server")]
public sealed class AntiForgeryServiceServerTests(WebApplicationFactory<Program> factory)
   
{
    [Fact]
    public async Task SubmitAsync_WithoutAntiForgeryToken_Returns400()
    {
        // The generated endpoint has .ValidateAntiforgery().
        // ASP.NET Core rejects requests that lack a valid RequestVerificationToken with 400.
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync(
            new Uri("/api/functions/antiforgery/SubmitAsync", UriKind.Relative), "test");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SubmitAsync_ServiceResolvesFromDi_WithoutHttpLayer()
    {
        // Confirms the service implementation is registered and the interface is
        // wired to AntiForgeryService — antiforgery only applies over HTTP.
        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IAntiForgeryService>();
        var result = await service.SubmitAsync("hello");
        Assert.Equal("submitted:hello", result);
    }
}
