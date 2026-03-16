using BlazorServerFunctions.Sample.Shared;

namespace BlazorServerFunctions.Sample.Components.Echo;

internal sealed class EchoService : IEchoService
{
    public Task<string> GetEchoAsync(string message) => Task.FromResult(message);

    public Task<string> PostEchoAsync(string message) => Task.FromResult(message);
}
