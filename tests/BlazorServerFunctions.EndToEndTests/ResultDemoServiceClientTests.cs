using BlazorServerFunctions.Sample.Shared;

namespace BlazorServerFunctions.EndToEndTests;

/// <summary>
/// End-to-end tests for the ResultMapper feature using <see cref="IResultDemoService"/>.
/// Verifies the full round-trip:
///   client proxy → generated REST endpoint → service implementation → mapper → HTTP response → mapper → client result
///
/// Success path: server calls <c>GetValue</c>, returns 200 OK with the inner DTO;
///   client calls <c>WrapValue</c> to reconstruct <c>Result&lt;T&gt;.Ok(dto)</c>.
/// Failure path: server calls <c>GetError</c>, returns a ProblemDetails body;
///   client parses the body and calls <c>WrapFailure</c> to reconstruct <c>Result&lt;T&gt;.NotFound/Conflict/…</c>.
/// </summary>
[Collection("E2E")]
public sealed class ResultDemoServiceClientTests(E2EFixture fixture)
{
    private IResultDemoService Client =>
        fixture.ClientServices.GetRequiredService<IResultDemoService>();

    // ── GetProductAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetProductAsync_ExistingId_ReturnsSuccess()
    {
        var result = await Client.GetProductAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.Id);
        Assert.Equal("Widget", result.Value.Name);
        Assert.Equal(9.99m, result.Value.Price);
    }

    [Fact]
    public async Task GetProductAsync_MissingId_ReturnsNotFound()
    {
        var result = await Client.GetProductAsync(999);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Status);
        Assert.Contains("999", result.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetProductAsync_SecondSeededProduct_ReturnsSuccess()
    {
        var result = await Client.GetProductAsync(2);

        Assert.True(result.IsSuccess);
        Assert.Equal("Gadget", result.Value!.Name);
    }

    // ── CreateProductAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task CreateProductAsync_UniqueName_ReturnsCreatedProduct()
    {
        var result = await Client.CreateProductAsync("E2EProduct_Create", 14.99m);

        Assert.True(result.IsSuccess);
        Assert.Equal("E2EProduct_Create", result.Value!.Name);
        Assert.Equal(14.99m, result.Value.Price);
        Assert.True(result.Value.Id > 0);
    }

    [Fact]
    public async Task CreateProductAsync_DuplicateName_ReturnsConflict()
    {
        // "Widget" is pre-seeded — creating a second one must return 409 Conflict.
        var result = await Client.CreateProductAsync("Widget", 1.00m);

        Assert.False(result.IsSuccess);
        Assert.Equal(409, result.Status);
        Assert.Contains("Widget", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    // ── DeleteProductAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteProductAsync_ExistingProduct_ReturnsDeletedItem()
    {
        // Create a product first so this test is self-contained and doesn't interfere
        // with the seeded data that other tests depend on.
        var created = await Client.CreateProductAsync("E2EProduct_Delete", 5.00m);
        Assert.True(created.IsSuccess, "pre-condition: create must succeed");

        var deleted = await Client.DeleteProductAsync(created.Value!.Id);

        Assert.True(deleted.IsSuccess);
        Assert.Equal(created.Value.Id, deleted.Value!.Id);
        Assert.Equal("E2EProduct_Delete", deleted.Value.Name);
    }

    [Fact]
    public async Task DeleteProductAsync_MissingId_ReturnsNotFound()
    {
        var result = await Client.DeleteProductAsync(999);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Status);
        Assert.Contains("999", result.ErrorMessage, StringComparison.Ordinal);
    }

    // ── Round-trip fidelity ───────────────────────────────────────────────────

    [Fact]
    public async Task GetProductAsync_AfterCreate_ReturnsCreatedValues()
    {
        // Verifies that the created DTO is stored and can be retrieved — the mapper
        // correctly unwraps on create and re-wraps on the subsequent get.
        var created = await Client.CreateProductAsync("E2EProduct_RoundTrip", 42.00m);
        Assert.True(created.IsSuccess, "pre-condition: create must succeed");

        var fetched = await Client.GetProductAsync(created.Value!.Id);

        Assert.True(fetched.IsSuccess);
        Assert.Equal(created.Value.Id, fetched.Value!.Id);
        Assert.Equal("E2EProduct_RoundTrip", fetched.Value.Name);
        Assert.Equal(42.00m, fetched.Value.Price);
    }
}
