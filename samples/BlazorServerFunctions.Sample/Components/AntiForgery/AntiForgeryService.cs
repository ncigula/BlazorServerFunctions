using BlazorServerFunctions.Sample.Shared;

namespace BlazorServerFunctions.Sample.Components.AntiForgery;

internal sealed class AntiForgeryService : IAntiForgeryService
{
    public Task<string> SubmitAsync(string value) =>
        Task.FromResult($"submitted:{value}");
}
