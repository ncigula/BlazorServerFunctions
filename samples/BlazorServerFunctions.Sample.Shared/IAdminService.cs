using BlazorServerFunctions.Abstractions;

namespace BlazorServerFunctions.Sample.Shared;

[ServerFunctionCollection(RequireAuthorization = true)]
public interface IAdminService
{
    [ServerFunction(HttpMethod = "GET")]
    Task<string> GetSecretAsync(CancellationToken cancellationToken = default);

    [ServerFunction(HttpMethod = "GET", Roles = "Admin")]
    Task<string> GetRoleSecretAsync(CancellationToken cancellationToken = default);

    [ServerFunction(HttpMethod = "GET", Policy = "AdminOnly")]
    Task<string> GetPolicySecretAsync(CancellationToken cancellationToken = default);

    [ServerFunction(HttpMethod = "GET", Roles = "Admin", Policy = "AdminOnly")]
    Task<string> GetRoleAndPolicySecretAsync(CancellationToken cancellationToken = default);
}
