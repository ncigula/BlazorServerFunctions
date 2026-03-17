namespace BlazorServerFunctions.EndToEndTests;

/// <summary>
/// Server path: resolves ICrudService directly from DI → CrudService.
/// </summary>
public sealed class CrudServiceServerTests(E2EFixture fixture) : IClassFixture<E2EFixture>
{
    [Fact]
    public async Task GetAsync_ReturnsItemWithCorrectId()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var result = await scope.ServiceProvider.GetRequiredService<ICrudService>().GetAsync(42);
        Assert.Equal(42, result.Id);
    }

    [Fact]
    public async Task CreateAsync_ComplexDto_RoundTripsAllFields()
    {
        var input = new ComplexDto
        {
            Name = "Test Item",
            Description = "A complex object",
            Tags = ["tag1", "tag2"],
            CreatedAt = DateTimeOffset.UtcNow,
            Price = 99.99m
        };

        using var scope = fixture.Factory.Services.CreateScope();
        var result = await scope.ServiceProvider.GetRequiredService<ICrudService>().CreateAsync(input);

        Assert.Equal(input.Name, result.Name);
        Assert.Equal(input.Description, result.Description);
        Assert.Equal(input.Price, result.Price);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsItemWithCorrectId()
    {
        var update = new ComplexDto { Id = 5, Name = "Updated", Price = 1.0m };
        using var scope = fixture.Factory.Services.CreateScope();
        var result = await scope.ServiceProvider.GetRequiredService<ICrudService>().UpdateAsync(5, update);
        Assert.Equal(5, result.Id);
        Assert.Equal("Updated", result.Name);
    }

    [Fact]
    public async Task PatchAsync_ReflectsChange()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var result = await scope.ServiceProvider.GetRequiredService<ICrudService>()
            .PatchAsync(7, "Name", "patched-value");
        Assert.Equal(7, result.Id);
        Assert.Equal("patched-value", result.Name);
    }

    [Fact]
    public async Task DeleteAsync_CompletesWithoutError()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var exception = await Record.ExceptionAsync(
            () => scope.ServiceProvider.GetRequiredService<ICrudService>().DeleteAsync(10));
        Assert.Null(exception);
    }

    [Fact]
    public async Task CreateAsync_NullDescription_RoundTripsNull()
    {
        var input = new ComplexDto { Name = "No Description", Description = null };
        using var scope = fixture.Factory.Services.CreateScope();
        var result = await scope.ServiceProvider.GetRequiredService<ICrudService>().CreateAsync(input);
        Assert.Null(result.Description);
    }
}

/// <summary>
/// Client path: resolves CrudServiceClient from DI → HTTP calls to the in-memory server.
/// Verifies all HTTP verbs and complex DTO JSON round-trip through generated endpoints.
/// </summary>
public sealed class CrudServiceClientTests(E2EFixture fixture) : IClassFixture<E2EFixture>
{
    private CrudServiceClient Client =>
        fixture.Factory.Services.GetRequiredService<CrudServiceClient>();

    [Fact]
    public async Task GetAsync_ReturnsItemWithCorrectId()
    {
        var result = await Client.GetAsync(42);
        Assert.Equal(42, result.Id);
    }

    [Fact]
    public async Task CreateAsync_ComplexDto_RoundTripsAllFields()
    {
        var input = new ComplexDto
        {
            Name = "Test Item",
            Description = "A complex object",
            Tags = ["tag1", "tag2"],
            CreatedAt = DateTimeOffset.UtcNow,
            Price = 99.99m
        };

        var result = await Client.CreateAsync(input);

        Assert.Equal(input.Name, result.Name);
        Assert.Equal(input.Description, result.Description);
        Assert.Equal(input.Tags, result.Tags);
        Assert.Equal(input.Price, result.Price);
    }

    [Fact]
    public async Task UpdateAsync_PutWithComplexDto_ReturnsUpdatedItem()
    {
        var update = new ComplexDto { Id = 5, Name = "Updated", Price = 1.0m };
        var result = await Client.UpdateAsync(5, update);
        Assert.Equal(5, result.Id);
        Assert.Equal("Updated", result.Name);
    }

    [Fact]
    public async Task PatchAsync_PartialUpdate_ReflectsChange()
    {
        var result = await Client.PatchAsync(7, "Name", "patched-value");
        Assert.Equal(7, result.Id);
        Assert.Equal("patched-value", result.Name);
    }

    [Fact]
    public async Task DeleteAsync_CompletesWithoutError()
    {
        var exception = await Record.ExceptionAsync(() => Client.DeleteAsync(10));
        Assert.Null(exception);
    }

    [Fact]
    public async Task CreateAsync_NullDescription_RoundTripsNull()
    {
        var input = new ComplexDto { Name = "No Description", Description = null };
        var result = await Client.CreateAsync(input);
        Assert.Null(result.Description);
    }
}
