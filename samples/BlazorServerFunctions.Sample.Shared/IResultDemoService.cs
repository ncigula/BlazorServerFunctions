using BlazorServerFunctions.Abstractions;

namespace BlazorServerFunctions.Sample.Shared;

/// <summary>
/// Demonstrates the <c>ResultMapper</c> feature: methods return <c>Result&lt;T&gt;</c>
/// instead of plain <c>T</c>. The generated server endpoint unwraps the result and
/// returns either 200 OK or a ProblemDetails error response. The generated client proxy
/// reconstructs the <c>Result&lt;T&gt;</c> from the HTTP response automatically.
/// </summary>
[ServerFunctionCollection(
    Configuration = typeof(SampleApiConfig),
    ResultMapper = typeof(ResultMapper<>))]
public interface IResultDemoService
{
    /// <summary>
    /// Looks up a product by ID.
    /// Returns <c>Result.NotFound</c> when the ID does not exist, triggering a 404 response.
    /// </summary>
    [ServerFunction(HttpMethod = "GET", Route = "{id}")]
    Task<Result<ProductDto>> GetProductAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a product.
    /// Returns <c>Result.Conflict</c> when a product with the same name already exists.
    /// </summary>
    [ServerFunction(HttpMethod = "POST")]
    Task<Result<ProductDto>> CreateProductAsync(string name, decimal price, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a product.
    /// Returns <c>Result.NotFound</c> if the product does not exist.
    /// Returns <c>Result.Ok(deleted)</c> with the removed item otherwise.
    /// </summary>
    [ServerFunction(HttpMethod = "DELETE", Route = "{id}")]
    Task<Result<ProductDto>> DeleteProductAsync(int id, CancellationToken cancellationToken = default);
}

