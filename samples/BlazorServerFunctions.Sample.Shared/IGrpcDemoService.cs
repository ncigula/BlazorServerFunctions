using BlazorServerFunctions.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorServerFunctions.Sample.Shared;

[ServerFunctionCollection(ApiType = ApiType.GRPC)]
public interface IGrpcDemoService
{
    [ServerFunction]
    Task<string> EchoAsync(string message);

    [ServerFunction]
    IAsyncEnumerable<string> CountdownAsync(int from);

    [ServerFunction(RequireAuthorization = true, Policy = "AdminOnly")]
    Task<string> GetSecretAsync();
}
