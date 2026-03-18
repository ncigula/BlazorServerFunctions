using BlazorServerFunctions.Sample.Shared;

namespace BlazorServerFunctions.Sample.Components.RouteParams;

internal sealed class RouteParamService : IRouteParamService
{
    public Task<string> GetByIdAsync(int id) =>
        Task.FromResult($"item-{id}");

    public Task DeleteByIdAsync(int id) =>
        Task.CompletedTask;

    public Task<string> UpdateAsync(int id, string value) =>
        Task.FromResult($"updated-{id}-{value}");

    public Task<string> GetTagsAsync(int id, int page) =>
        Task.FromResult($"tags-{id}-page-{page}");
}
