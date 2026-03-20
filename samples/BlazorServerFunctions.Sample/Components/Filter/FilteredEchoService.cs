namespace BlazorServerFunctions.Sample.Components.Filter;

internal sealed class FilteredEchoService : IFilteredEchoService
{
    public Task<string> EchoAsync(string message) => Task.FromResult(message);
}
