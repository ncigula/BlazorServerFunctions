using BlazorServerFunctions.Abstractions;

namespace BlazorServerFunctions.Sample.Shared;

[ServerFunctionCollection(RequireAuthorization = true)]
public interface IAdminService
{
    [ServerFunction(HttpMethod = "GET")]
    Task<string> GetSecretAsync(CancellationToken cancellationToken = default);
}
