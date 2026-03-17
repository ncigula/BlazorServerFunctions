using BlazorServerFunctions.Abstractions;

namespace BlazorServerFunctions.Sample.Shared;

[ServerFunctionCollection(RoutePrefix = "crud")]
public interface ICrudService
{
    [ServerFunction(HttpMethod = "GET")]
    Task<ComplexDto[]> GetAllAsync();

    [ServerFunction(HttpMethod = "GET")]
    Task<ComplexDto> GetAsync(int id);

    [ServerFunction(HttpMethod = "POST")]
    Task<ComplexDto> CreateAsync(ComplexDto item);

    [ServerFunction(HttpMethod = "PUT")]
    Task<ComplexDto> UpdateAsync(int id, ComplexDto item);

    [ServerFunction(HttpMethod = "PATCH")]
    Task<ComplexDto> PatchAsync(int id, string field, string value);

    [ServerFunction(HttpMethod = "DELETE")]
    Task DeleteAsync(int id);
}
