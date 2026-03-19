namespace BlazorServerFunctions.Abstractions;

[AttributeUsage(AttributeTargets.Interface)]
public sealed class ServerFunctionCollectionAttribute : Attribute
{
    public string? RoutePrefix { get; set; }

    public bool RequireAuthorization { get; set; }

    /// <summary>
    /// Named CORS policy applied to all endpoints in this collection via
    /// <c>group.RequireCors("policyName")</c>.
    /// <c>null</c> (default) means no CORS policy.
    /// Setting this to an empty string is an error (BSF022).
    /// Requires <c>builder.Services.AddCors(...)</c> and <c>app.UseCors()</c> in the server pipeline.
    /// </summary>
    public string? CorsPolicy { get; set; }

    /// <summary>
    /// Compile-time configuration class. Must be a type that inherits from
    /// <see cref="ServerFunctionConfiguration"/>.
    /// </summary>
    /// <example><c>Configuration = typeof(MyApiConfig)</c></example>
    public Type? Configuration { get; set; }
}
