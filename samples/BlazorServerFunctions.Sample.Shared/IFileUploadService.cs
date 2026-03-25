using System.IO;
using System.Threading.Tasks;
using BlazorServerFunctions.Abstractions;

namespace BlazorServerFunctions.Sample.Shared;

[ServerFunctionCollection(Configuration = typeof(SampleApiConfig))]
public interface IFileUploadService
{
    [ServerFunction(HttpMethod = "POST")]
    Task<long> UploadAsync(Stream file, string fileName);

    [ServerFunction(HttpMethod = "POST")]
    Task<long> UploadStreamOnlyAsync(Stream file);
}
