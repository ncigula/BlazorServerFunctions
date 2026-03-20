using BlazorServerFunctions.Abstractions;

namespace BlazorServerFunctions.Sample.Shared;

/// <summary>
/// Demonstrates <c>RequireAntiForgery = true</c>: the generated minimal API endpoint
/// gets <c>.ValidateAntiforgery()</c>, so requests without a valid antiforgery token
/// are rejected with <c>400 Bad Request</c> before the handler is invoked.
/// </summary>
[ServerFunctionCollection(RoutePrefix = "antiforgery")]
public interface IAntiForgeryService
{
    /// <summary>
    /// POST endpoint protected by antiforgery validation.
    /// Callers must supply a valid <c>RequestVerificationToken</c> header/cookie pair.
    /// </summary>
    [ServerFunction(HttpMethod = "POST", RequireAntiForgery = true)]
    Task<string> SubmitAsync(string value);
}
