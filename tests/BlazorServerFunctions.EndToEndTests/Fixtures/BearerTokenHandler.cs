using System.Net.Http.Headers;

namespace BlazorServerFunctions.EndToEndTests;

/// <summary>
/// A delegating handler that injects an <c>Authorization: Bearer &lt;token&gt;</c> header
/// on every outgoing request. Used in JWT auth E2E tests to simulate API clients that
/// authenticate with tokens rather than cookies.
/// </summary>
internal sealed class BearerTokenHandler(string token, HttpMessageHandler inner)
    : DelegatingHandler(inner)
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return base.SendAsync(request, cancellationToken);
    }
}
