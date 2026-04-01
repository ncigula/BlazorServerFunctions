using BlazorServerFunctions.Sample.Shared;

namespace BlazorServerFunctions.Sample.Components.ExplicitBinding;

internal sealed class ExplicitBindingService : IExplicitBindingService
{
    public Task<string> GetItemAsync(int id, string? filter) =>
        Task.FromResult($"id={id} filter={filter}");

    public Task DeleteItemAsync(int id) => Task.CompletedTask;

    public Task<string> CreateOrderAsync(string tenantId, string productId, int quantity) =>
        Task.FromResult($"tenant={tenantId} product={productId} qty={quantity}");

    public Task<string> SearchAsync(int page, int pageSize, string query) =>
        Task.FromResult($"page={page} size={pageSize} query={query}");
}
