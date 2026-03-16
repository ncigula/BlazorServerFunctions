namespace BlazorServerFunctions.EndToEndTests;

public class CrudServiceE2ETests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly CrudServiceClient _client = new(factory.CreateClient());

    [Fact]
    public async Task GetAsync_ReturnsItemWithCorrectId()
    {
        var result = await _client.GetAsync(42);
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

        var result = await _client.CreateAsync(input);

        Assert.Equal(input.Name, result.Name);
        Assert.Equal(input.Description, result.Description);
        Assert.Equal(input.Tags, result.Tags);
        Assert.Equal(input.Price, result.Price);
    }

    [Fact]
    public async Task UpdateAsync_PutWithComplexDto_ReturnsUpdatedItem()
    {
        var update = new ComplexDto { Id = 5, Name = "Updated", Price = 1.0m };
        var result = await _client.UpdateAsync(5, update);
        Assert.Equal(5, result.Id);
        Assert.Equal("Updated", result.Name);
    }

    [Fact]
    public async Task PatchAsync_PartialUpdate_ReflectsChange()
    {
        var result = await _client.PatchAsync(7, "Name", "patched-value");
        Assert.Equal(7, result.Id);
        Assert.Equal("patched-value", result.Name);
    }

    [Fact]
    public async Task DeleteAsync_CompletesWithoutError()
    {
        var exception = await Record.ExceptionAsync(() => _client.DeleteAsync(10));
        Assert.Null(exception);
    }

    [Fact]
    public async Task CreateAsync_NullableFields_RoundTripsNullCorrectly()
    {
        var input = new ComplexDto { Name = "No Description", Description = null };
        var result = await _client.CreateAsync(input);
        Assert.Null(result.Description);
    }
}
