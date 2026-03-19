using BlazorServerFunctions.Sample.Shared;

namespace BlazorServerFunctions.Sample.Components.Caching;

/// <summary>
/// In-memory singleton counter — demonstrates that GET responses are cached for 1 second
/// even while the counter keeps incrementing via POST.
/// </summary>
internal sealed class CacheableService : ICacheableService
{
    private int _count;

    public Task<int> IncrementAsync()
    {
        Interlocked.Increment(ref _count);
        return Task.FromResult(_count);
    }

    public Task<int> GetCountAsync() => Task.FromResult(_count);
}
