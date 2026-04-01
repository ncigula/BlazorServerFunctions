using BlazorServerFunctions.Abstractions;

namespace BlazorServerFunctions.Sample.Shared;

[ServerFunctionCollection(RoutePrefix = "explicit-binding", Configuration = typeof(SampleApiConfig))]
public interface IExplicitBindingService
{
    // ── Auto (default inference) ──────────────────────────────────────────────
    // GET /api/v1/explicit-binding/{id}?Filter=<val>
    // id → route (inferred from {id}), filter → query string (inferred: GET non-route)
    [ServerFunction(HttpMethod = "GET", Route = "{id}")]
    Task<string> GetItemAsync(int id, string? filter);

    // ── ParameterSource.Route (compile-time validation marker) ───────────────
    // DELETE /api/v1/explicit-binding/{id}
    // Explicit assertion that id must appear as {id} in the route template; BSF031 if absent
    [ServerFunction(HttpMethod = "DELETE", Route = "{id}")]
    Task DeleteItemAsync(
        [ServerFunctionParameter(From = ParameterSource.Route)] int id);

    // ── ParameterSource.Header ────────────────────────────────────────────────
    // POST /api/v1/explicit-binding/create-order-async
    // tenantId read from X-Tenant-Id header; productId/quantity in JSON body (auto)
    [ServerFunction(HttpMethod = "POST")]
    Task<string> CreateOrderAsync(
        [ServerFunctionParameter(From = ParameterSource.Header, Name = "X-Tenant-Id")] string tenantId,
        string productId,
        int quantity);

    // ── ParameterSource.Query on POST ─────────────────────────────────────────
    // POST /api/v1/explicit-binding/search-async?Page=<p>&PageSize=<ps>
    // page/pageSize forced to URL query string; query stays in JSON body (auto)
    [ServerFunction(HttpMethod = "POST")]
    Task<string> SearchAsync(
        [ServerFunctionParameter(From = ParameterSource.Query)] int page,
        [ServerFunctionParameter(From = ParameterSource.Query)] int pageSize,
        string query);

}
