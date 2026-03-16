namespace BlazorServerFunctions.Sample.Shared;

public sealed class ComplexDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public IReadOnlyCollection<string> Tags { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
    public decimal Price { get; set; }
}
