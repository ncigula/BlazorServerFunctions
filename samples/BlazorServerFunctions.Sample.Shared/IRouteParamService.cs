using BlazorServerFunctions.Abstractions;

namespace BlazorServerFunctions.Sample.Shared;

[ServerFunctionCollection(RoutePrefix = "route-params")]
public interface IRouteParamService
{
    /// <summary>GET with a single route param — no query string or body.</summary>
    [ServerFunction(HttpMethod = "GET", Route = "{id}")]
    Task<string> GetByIdAsync(int id);

    /// <summary>DELETE with a route param — no body at all.</summary>
    [ServerFunction(HttpMethod = "DELETE", Route = "{id}")]
    Task DeleteByIdAsync(int id);

    /// <summary>PUT with a route param + a body param.</summary>
    [ServerFunction(HttpMethod = "PUT", Route = "{id}")]
    Task<string> UpdateAsync(int id, string value);

    /// <summary>GET with a route param and an additional query-string param.</summary>
    [ServerFunction(HttpMethod = "GET", Route = "{id}/tags")]
    Task<string> GetTagsAsync(int id, int page);
}
