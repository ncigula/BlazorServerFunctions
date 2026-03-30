using BlazorServerFunctions.Sample.Shared;

namespace BlazorServerFunctions.Sample.Components.ResultDemo;

/// <summary>
/// Server-side implementation of <see cref="IResultDemoService"/>.
/// Demonstrates how the server implementation works with the ResultMapper feature:
/// the service returns <c>Result&lt;T&gt;</c> (a domain result type), and the generated
/// endpoint unwraps it into either 200 OK or a ProblemDetails error response.
///
/// In a real application this service would typically delegate to a MediatR handler
/// or a repository, both of which naturally return result types.
/// </summary>
public sealed class ResultDemoService : IResultDemoService
{
    private int _nextId = 3; // IDs 1 and 2 are pre-seeded

    // In-memory store — shared across requests via singleton registration.
    private readonly Dictionary<int, ProductDto> _products = new()
    {
        [1] = new ProductDto(1, "Widget", 9.99m),
        [2] = new ProductDto(2, "Gadget", 24.99m),
    };

    /// <inheritdoc/>
    public Task<Result<ProductDto>> GetProductAsync(int id, CancellationToken cancellationToken = default)
    {
        if (_products.TryGetValue(id, out var product))
            return Task.FromResult(Result<ProductDto>.Ok(product));

        return Task.FromResult(Result<ProductDto>.NotFound($"Product #{id} was not found."));
    }

    /// <inheritdoc/>
    public Task<Result<ProductDto>> CreateProductAsync(string name, decimal price, CancellationToken cancellationToken = default)
    {
        if (_products.Values.Any(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)))
            return Task.FromResult(Result<ProductDto>.Conflict($"A product named '{name}' already exists."));

        var id = _nextId++;
        var dto = new ProductDto(id, name, price);
        _products[id] = dto;
        return Task.FromResult(Result<ProductDto>.Ok(dto));
    }

    /// <inheritdoc/>
    public Task<Result<ProductDto>> DeleteProductAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!_products.TryGetValue(id, out var product))
            return Task.FromResult(Result<ProductDto>.NotFound($"Product #{id} was not found."));

        _products.Remove(id);
        return Task.FromResult(Result<ProductDto>.Ok(product));
    }
}
