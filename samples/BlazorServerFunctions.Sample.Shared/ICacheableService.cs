using BlazorServerFunctions.Abstractions;

namespace BlazorServerFunctions.Sample.Shared;

/// <summary>
/// Demonstrates §3.5 Output Caching: a POST that mutates state and a cached GET that reads it.
/// The GET is cached for 1 second so clients see stale data until the cache expires.
/// </summary>
[ServerFunctionCollection]
public interface ICacheableService
{
    /// <summary>Increments the server-side counter and returns the new value.</summary>
    [ServerFunction(HttpMethod = "POST")]
    Task<int> IncrementAsync();

    /// <summary>Returns the current counter value. Response is cached for 1 second.</summary>
    [ServerFunction(HttpMethod = "GET", CacheSeconds = 1)]
    Task<int> GetCountAsync();
}
