namespace BlazorServerFunctions.EndToEndTests;

/// <summary>
/// Server path: resolves IAdminService directly from DI → AdminService.
/// No auth check applies — server-side components already run in an authenticated context.
/// </summary>
[Collection("Server")]
public sealed class AdminServiceServerTests(WebApplicationFactory<Program> factory)
{
    [Fact]
    public async Task GetSecretAsync_ReturnsSecret_WithoutAuthCheck()
    {
        using var scope = factory.Services.CreateScope();
        var result = await scope.ServiceProvider.GetRequiredService<IAdminService>()
            .GetSecretAsync();
        Assert.Equal("top-secret", result);
    }
}
