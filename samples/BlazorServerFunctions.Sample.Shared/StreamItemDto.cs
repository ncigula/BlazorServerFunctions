namespace BlazorServerFunctions.Sample.Shared;

public sealed class StreamItemDto
{
    public int Index { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
}
