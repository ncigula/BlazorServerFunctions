using BlazorServerFunctions.Sample.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorServerFunctions.EndToEndTests;

[Collection("E2E")]
public class GrpcServiceClientTests(E2EFixture fixture)
{
    private static readonly string[] s_countdownExpected = ["3", "2", "1", "0"];

    private IGrpcDemoService Client => fixture.ClientServices.GetRequiredService<IGrpcDemoService>();

    [Fact]
    public async Task Echo_ReturnsPrefixedMessage()
    {
        var result = await Client.EchoAsync("hello");
        Assert.Equal("gRPC echo: hello", result);
    }

    [Fact]
    public async Task Countdown_StreamsAllValues()
    {
        var items = new List<string>();
        await foreach (var item in Client.CountdownAsync(3))
            items.Add(item);
        Assert.Equal(s_countdownExpected, items);
    }
}
