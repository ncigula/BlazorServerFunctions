using BlazorServerFunctions.Sample.Shared;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace BlazorServerFunctions.Sample.Components.GrpcDemo;

internal sealed class GrpcDemoService : IGrpcDemoService
{
    public Task<string> EchoAsync(string message) =>
        Task.FromResult($"gRPC echo: {message}");

    public async IAsyncEnumerable<string> CountdownAsync(int from)
    {
        for (var i = from; i >= 0; i--)
        {
            yield return i.ToString(CultureInfo.InvariantCulture);
            await Task.Delay(200).ConfigureAwait(false);
        }
    }

    public Task<string> GetUserSecretAsync() =>
        Task.FromResult("User secret: you are authenticated!");

    public Task<string> GetSecretAsync() =>
        Task.FromResult("The secret is: gRPC auth works!");
}
