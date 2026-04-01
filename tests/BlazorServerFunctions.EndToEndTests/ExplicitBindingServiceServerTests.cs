using BlazorServerFunctions.Sample.Shared;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorServerFunctions.EndToEndTests;

[Collection("Server")]
public sealed class ExplicitBindingServiceServerTests(WebApplicationFactory<Program> factory)
{
    [Fact]
    public async Task GetItemAsync_DirectCall_ReturnsIdAndFilter()
    {
        using var scope = factory.Services.CreateScope();
        var result = await scope.ServiceProvider
            .GetRequiredService<IExplicitBindingService>()
            .GetItemAsync(7, "pending");
        Assert.Equal("id=7 filter=pending", result);
    }

    [Fact]
    public async Task DeleteItemAsync_DirectCall_CompletesWithoutError()
    {
        using var scope = factory.Services.CreateScope();
        var exception = await Record.ExceptionAsync(
            () => scope.ServiceProvider
                .GetRequiredService<IExplicitBindingService>()
                .DeleteItemAsync(1));
        Assert.Null(exception);
    }

    [Fact]
    public async Task CreateOrderAsync_DirectCall_ReturnsExpectedResult()
    {
        using var scope = factory.Services.CreateScope();
        var result = await scope.ServiceProvider
            .GetRequiredService<IExplicitBindingService>()
            .CreateOrderAsync("tenant-x", "product-y", 7);
        Assert.Equal("tenant=tenant-x product=product-y qty=7", result);
    }

    [Fact]
    public async Task SearchAsync_DirectCall_ReturnsExpectedResult()
    {
        using var scope = factory.Services.CreateScope();
        var result = await scope.ServiceProvider
            .GetRequiredService<IExplicitBindingService>()
            .SearchAsync(1, 20, "blazor");
        Assert.Equal("page=1 size=20 query=blazor", result);
    }

}
