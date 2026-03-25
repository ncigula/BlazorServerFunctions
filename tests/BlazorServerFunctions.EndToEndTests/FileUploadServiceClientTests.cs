using System.Text;
using BlazorServerFunctions.Sample.Shared;

namespace BlazorServerFunctions.EndToEndTests;

/// <summary>
/// Client path: resolves IFileUploadService from DI → FileUploadServiceClient (HTTP proxy) → in-memory server.
/// Verifies that Stream parameters are sent as multipart/form-data and the server receives the bytes correctly.
/// </summary>
[Collection("E2E")]
public sealed class FileUploadServiceClientTests(E2EFixture fixture)
{
    private IFileUploadService Client =>
        fixture.ClientServices.GetRequiredService<IFileUploadService>();

    [Fact]
    public async Task UploadAsync_StreamContent_ReturnsByteCount()
    {
        const string content = "Hello, file upload!";
        var bytes = Encoding.UTF8.GetBytes(content);
        using var stream = new MemoryStream(bytes);

        var byteCount = await Client.UploadAsync(stream, "test.txt");

        Assert.Equal(bytes.Length, byteCount);
    }

    [Fact]
    public async Task UploadAsync_EmptyStream_ReturnsZero()
    {
        using var stream = new MemoryStream([]);

        var byteCount = await Client.UploadAsync(stream, "empty.txt");

        Assert.Equal(0, byteCount);
    }

    [Fact]
    public async Task UploadStreamOnlyAsync_StreamContent_ReturnsByteCount()
    {
        const string content = "Another upload test";
        var bytes = Encoding.UTF8.GetBytes(content);
        using var stream = new MemoryStream(bytes);

        var byteCount = await Client.UploadStreamOnlyAsync(stream);

        Assert.Equal(bytes.Length, byteCount);
    }

    [Fact]
    public async Task UploadAsync_LargeStream_ReturnsByteCount()
    {
        var bytes = new byte[100_000];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
        using var stream = new MemoryStream(bytes);

        var byteCount = await Client.UploadAsync(stream, "large.bin");

        Assert.Equal(bytes.Length, byteCount);
    }
}
