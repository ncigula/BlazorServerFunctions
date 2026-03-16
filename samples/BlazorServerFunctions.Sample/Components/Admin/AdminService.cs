using BlazorServerFunctions.Sample.Shared;

namespace BlazorServerFunctions.Sample.Components.Admin;

internal sealed class AdminService : IAdminService
{
    public Task<string> GetSecretAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult("top-secret");
}
