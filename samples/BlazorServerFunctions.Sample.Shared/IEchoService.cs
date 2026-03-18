using BlazorServerFunctions.Abstractions;

namespace BlazorServerFunctions.Sample.Shared;

[ServerFunctionCollection(Configuration = typeof(SampleApiConfig))]
public interface IEchoService
{
    [ServerFunction(HttpMethod = "GET")]
    Task<string> GetEchoAsync(string message);

    [ServerFunction(HttpMethod = "POST")]
    Task<string> PostEchoAsync(string message);
}
