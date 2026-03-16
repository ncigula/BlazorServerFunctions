using BlazorServerFunctions.Sample.Shared;

namespace BlazorServerFunctions.Sample.Components.Crud;

internal sealed class CrudService : ICrudService
{
    public Task<ComplexDto> GetAsync(int id) =>
        Task.FromResult(new ComplexDto { Id = id, Name = $"item-{id}" });

    public Task<ComplexDto> CreateAsync(ComplexDto item)
    {
        item.Id = 1;
        return Task.FromResult(item);
    }

    public Task<ComplexDto> UpdateAsync(int id, ComplexDto item)
    {
        item.Id = id;
        return Task.FromResult(item);
    }

    public Task<ComplexDto> PatchAsync(int id, string field, string value) =>
        Task.FromResult(new ComplexDto { Id = id, Name = value });

    public Task DeleteAsync(int id) => Task.CompletedTask;
}
