using System.Collections.Generic;
using BlazorServerFunctions.Abstractions;

namespace BlazorServerFunctions.Sample.Shared;

[ServerFunctionCollection(RoutePrefix = "streaming")]
public interface IStreamingService
{
    /// <summary>
    /// Streams exactly <paramref name="count"/> items, yielding one every ~50 ms.
    /// </summary>
    [ServerFunction(HttpMethod = "GET")]
    IAsyncEnumerable<StreamItemDto> StreamItemsAsync(int count, CancellationToken cancellationToken = default);
}
