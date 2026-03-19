using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BlazorServerFunctions.Sample.Shared;

namespace BlazorServerFunctions.Sample.Components.Streaming;

internal sealed class StreamingService : IStreamingService
{
    public async IAsyncEnumerable<StreamItemDto> StreamItemsAsync(
        int count,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        for (var i = 1; i <= count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(50, cancellationToken).ConfigureAwait(false);

            yield return new StreamItemDto
            {
                Index = i,
                Message = $"Item {i} of {count}",
                Timestamp = DateTimeOffset.UtcNow
            };
        }
    }
}
