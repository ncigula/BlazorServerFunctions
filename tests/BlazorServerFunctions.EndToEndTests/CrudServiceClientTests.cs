using System.Net;
using System.Text.Json;

namespace BlazorServerFunctions.EndToEndTests;

/// <summary>
/// Client path: resolves ICrudService from DI → CrudServiceClient (HTTP proxy) → in-memory server.
/// Verifies all HTTP verbs and complex DTO JSON round-trip through generated endpoints.
/// </summary>
[Collection("E2E")]
public sealed class CrudServiceClientTests(E2EFixture fixture)
{
    private ICrudService Client =>
        fixture.ClientServices.GetRequiredService<ICrudService>();

    [Fact]
    public async Task GetAsync_ReturnsItemWithCorrectId()
    {
        var result = await Client.GetAsync(1);
        Assert.Equal(1, result.Id);
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
        var result = await Client.PatchAsync(2, "Name", "patched-value");
        Assert.Equal(2, result.Id);
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
    
    [Fact]
    public async Task GetAsync_NonExistentId_ThrowsWithProblemDetailsBody()
    {
        var ex = await Assert.ThrowsAsync<HttpRequestException>(
            () => Client.GetAsync(99));

        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
        AssertIsProblemDetailsWithDetail(ex.Message, "Item 99 not found.");
    }

    [Fact]
    public async Task PatchAsync_NonExistentId_ThrowsWithProblemDetailsBody()
    {
        var ex = await Assert.ThrowsAsync<HttpRequestException>(
            () => Client.PatchAsync(99, "Name", "x"));

        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
        AssertIsProblemDetailsWithDetail(ex.Message, "Item 99 not found.");
    }

    private static void AssertIsProblemDetailsWithDetail(string body, string expectedDetail)
    {
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("status", out var status),
            "Problem Details body missing 'status' field");
        Assert.Equal(500, status.GetInt32());

        Assert.True(root.TryGetProperty("detail", out var detail),
            "Problem Details body missing 'detail' field");
        Assert.Contains(expectedDetail, detail.GetString(), StringComparison.Ordinal);
    }
}
