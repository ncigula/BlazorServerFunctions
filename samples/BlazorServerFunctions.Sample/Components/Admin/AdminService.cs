using BlazorServerFunctions.Sample.Shared;

namespace BlazorServerFunctions.Sample.Components.Admin;

internal sealed class AdminService : IAdminService
{
    public Task<string> GetSecretAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult("top-secret");

    public Task<string> GetRoleSecretAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult("admin-role-secret");

    public Task<string> GetPolicySecretAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult("policy-secret");

    public Task<string> GetRoleAndPolicySecretAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult("role-and-policy-secret");
}
