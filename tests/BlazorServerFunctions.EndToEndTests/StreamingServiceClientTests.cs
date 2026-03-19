using System.Collections.Generic;

namespace BlazorServerFunctions.EndToEndTests;

/// <summary>
/// Verifies IAsyncEnumerable&lt;T&gt; streaming end-to-end via the generated client proxy:
/// IStreamingService → StreamingServiceClient (HTTP) → generated server endpoint → StreamingService.
/// </summary>
public sealed class StreamingServiceClientTests(E2EFixture fixture) : IClassFixture<E2EFixture>
{
    private IStreamingService Client =>
        fixture.ClientServices.GetRequiredService<IStreamingService>();

    [Fact]
    public async Task StreamItemsAsync_ReceivesAllItems()
    {
        var items = new List<StreamItemDto>();

        await foreach (var item in Client.StreamItemsAsync(count: 5))
            items.Add(item);

        Assert.Equal(5, items.Count);
        Assert.Equal([1, 2, 3, 4, 5], items.Select(i => i.Index));
        Assert.All(items, item => Assert.False(string.IsNullOrEmpty(item.Message)));
        Assert.All(items, item => Assert.NotEqual(default, item.Timestamp));
    }

    [Fact]
    public async Task StreamItemsAsync_ItemsArrivedInOrder()
    {
        var indices = new List<int>();

        await foreach (var item in Client.StreamItemsAsync(count: 5))
            indices.Add(item.Index);

        Assert.Equal(indices.Order().ToList(), indices);
    }

    [Fact]
    public async Task StreamItemsAsync_EarlyBreak_StopsConsuming()
    {
        // Break out of the foreach after 3 items — tests that the iterator is
        // cleanly disposed without requiring a CancellationToken.
        var items = new List<StreamItemDto>();

        await foreach (var item in Client.StreamItemsAsync(count: 20))
        {
            items.Add(item);
            if (items.Count >= 3)
                break;
        }

        Assert.Equal(3, items.Count);
    }

    [Fact]
    public async Task StreamItemsAsync_CountZero_YieldsNothing()
    {
        var items = new List<StreamItemDto>();

        await foreach (var item in Client.StreamItemsAsync(count: 0))
            items.Add(item);

        Assert.Empty(items);
    }
}
