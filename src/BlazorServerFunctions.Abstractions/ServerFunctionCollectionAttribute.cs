namespace BlazorServerFunctions.Abstractions;

[AttributeUsage(AttributeTargets.Interface)]
public sealed class ServerFunctionCollectionAttribute : Attribute
{
    public string? RoutePrefix { get; set; }

    public bool RequireAuthorization { get; set; }

    /// <summary>
    /// Compile-time configuration class. Must be a type that inherits from
    /// <see cref="ServerFunctionConfiguration"/>.
    /// </summary>
    /// <example><c>Configuration = typeof(MyApiConfig)</c></example>
    public Type? Configuration { get; set; }
}
