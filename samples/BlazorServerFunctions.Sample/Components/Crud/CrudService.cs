using System.Collections.Concurrent;
using BlazorServerFunctions.Sample.Shared;

namespace BlazorServerFunctions.Sample.Components.Crud;

internal sealed class CrudService : ICrudService
{
    private readonly ConcurrentDictionary<int, ComplexDto> _items;
    private int _nextId = 3;

    public CrudService()
    {
        _items = new ConcurrentDictionary<int, ComplexDto>
        {
            [1] = new ComplexDto { Id = 1, Name = "Widget", Description = "A basic widget", Price = 9.99m, Tags = ["hardware"], CreatedAt = DateTimeOffset.UtcNow },
            [2] = new ComplexDto { Id = 2, Name = "Gadget", Description = "A fancy gadget", Price = 24.99m, Tags = ["electronics"], CreatedAt = DateTimeOffset.UtcNow },
            [3] = new ComplexDto { Id = 3, Name = "Doohickey", Description = "A simple doohickey", Price = 4.99m, Tags = ["misc"], CreatedAt = DateTimeOffset.UtcNow },
        };
    }

    public Task<ComplexDto[]> GetAllAsync() =>
        Task.FromResult(_items.Values.OrderBy(x => x.Id).ToArray());

    public Task<ComplexDto> GetAsync(int id) =>
        _items.TryGetValue(id, out var item)
            ? Task.FromResult(item)
            : Task.FromException<ComplexDto>(new KeyNotFoundException($"Item {id} not found."));

    public Task<ComplexDto> CreateAsync(ComplexDto item)
    {
        item.Id = Interlocked.Increment(ref _nextId);
        item.CreatedAt = DateTimeOffset.UtcNow;
        _items[item.Id] = item;
        return Task.FromResult(item);
    }

    public Task<ComplexDto> UpdateAsync(int id, ComplexDto item)
    {
        item.Id = id;
        _items[id] = item;
        return Task.FromResult(item);
    }

    public Task<ComplexDto> PatchAsync(int id, string field, string value)
    {
        if (!_items.TryGetValue(id, out var existing))
            return Task.FromException<ComplexDto>(new KeyNotFoundException($"Item {id} not found."));

        var patched = new ComplexDto
        {
            Id = existing.Id,
            Name = string.Equals(field, "Name", StringComparison.Ordinal) ? value : existing.Name,
            Description = string.Equals(field, "Description", StringComparison.Ordinal) ? value : existing.Description,
            Price = existing.Price,
            Tags = existing.Tags,
            CreatedAt = existing.CreatedAt,
        };
        _items[id] = patched;
        return Task.FromResult(patched);
    }

    public Task DeleteAsync(int id)
    {
        _items.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}
