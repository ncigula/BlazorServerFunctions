using BlazorServerFunctions.Abstractions;
using BlazorServerFunctions.Sample.Components.Filter;

namespace BlazorServerFunctions.Sample.Components.Filter;

/// <summary>
/// Demonstrates <c>Filters = new[] { typeof(T) }</c>: the generated minimal API endpoint
/// gets <c>.AddEndpointFilter&lt;AddCorrelationHeaderFilter&gt;()</c>, so every response
/// from this endpoint includes an <c>X-Correlation-Id</c> header added by the filter.
/// </summary>
[ServerFunctionCollection(RoutePrefix = "filtered")]
public interface IFilteredEchoService
{
    /// <summary>
    /// GET endpoint with an endpoint filter that injects a correlation-id response header.
    /// </summary>
    [ServerFunction(HttpMethod = "GET", Filters = new[] { typeof(AddCorrelationHeaderFilter) })]
    Task<string> EchoAsync(string message);
}
