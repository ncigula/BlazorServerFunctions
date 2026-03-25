using BlazorServerFunctions.Sample.Shared;

namespace BlazorServerFunctions.Sample.Components.FileUpload;

internal sealed class FileUploadService : IFileUploadService
{
    public async Task<long> UploadAsync(Stream file, string fileName)
    {
        var buffer = new byte[4096];
        long totalBytes = 0;
        int bytesRead;
        while ((bytesRead = await file.ReadAsync(buffer).ConfigureAwait(false)) > 0)
            totalBytes += bytesRead;
        return totalBytes;
    }

    public async Task<long> UploadStreamOnlyAsync(Stream file)
    {
        var buffer = new byte[4096];
        long totalBytes = 0;
        int bytesRead;
        while ((bytesRead = await file.ReadAsync(buffer).ConfigureAwait(false)) > 0)
            totalBytes += bytesRead;
        return totalBytes;
    }
}
