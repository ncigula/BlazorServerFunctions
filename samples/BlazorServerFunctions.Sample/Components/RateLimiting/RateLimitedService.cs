using BlazorServerFunctions.Sample.Shared;

namespace BlazorServerFunctions.Sample.Components.RateLimiting;

/// <summary>
/// In-memory singleton counter — demonstrates that endpoints are protected by rate limiting.
/// </summary>
internal sealed class RateLimitedService : IRateLimitedService
{
    private int _count;

    public Task<int> IncrementAsync()
    {
        Interlocked.Increment(ref _count);
        return Task.FromResult(_count);
    }

    public Task<int> GetCountAsync() => Task.FromResult(_count);
}
