using BlazorServerFunctions.Abstractions;

namespace BlazorServerFunctions.Sample.Shared;

/// <summary>
/// Demonstrates §3.6 Rate Limiting: endpoints protected by a named rate-limiting policy.
/// Register the policy in the server pipeline via <c>builder.Services.AddRateLimiter(...)</c>
/// and enable the middleware with <c>app.UseRateLimiter()</c>.
/// </summary>
[ServerFunctionCollection]
public interface IRateLimitedService
{
    /// <summary>Increments the server-side counter and returns the new value. Rate-limited.</summary>
    [ServerFunction(HttpMethod = "POST", RateLimitPolicy = "fixed")]
    Task<int> IncrementAsync();

    /// <summary>Returns the current counter value. Rate-limited.</summary>
    [ServerFunction(HttpMethod = "GET", RateLimitPolicy = "fixed")]
    Task<int> GetCountAsync();
}
