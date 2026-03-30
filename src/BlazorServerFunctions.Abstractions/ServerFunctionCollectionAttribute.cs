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
    /// Transport type for this service collection.
    /// Shortcut for specifying <see cref="ServerFunctionConfiguration.ApiType"/>
    /// without defining a full configuration class.
    /// When <see cref="Configuration"/> is also specified, the config class's <c>ApiType</c> takes priority.
    /// </summary>
    public ApiType ApiType { get; set; } = ApiType.REST;

    /// <summary>
    /// Compile-time configuration class. Must be a type that inherits from
    /// <see cref="ServerFunctionConfiguration"/>.
    /// </summary>
    /// <example><c>Configuration = typeof(MyApiConfig)</c></example>
    public Type? Configuration { get; set; }

    /// <summary>
    /// Open-generic type that implements
    /// <see cref="IServerFunctionResultMapper{TResult,TValue}"/> for the result wrapper
    /// types used by methods in this collection.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Pass the open generic definition, e.g. <c>typeof(MyResultMapper&lt;&gt;)</c>.
    /// The generator closes it with the type arguments of each method's return type.
    /// </para>
    /// <para>
    /// When set, the generated server endpoint calls
    /// <c>mapper.IsSuccess / mapper.GetValue / mapper.GetProblem</c>
    /// instead of emitting a plain <c>Results.Ok(result)</c>.
    /// The generated client proxy calls <c>mapper.WrapValue / mapper.WrapProblem</c>
    /// instead of directly deserialising the return type.
    /// </para>
    /// <para>
    /// Methods that return <c>void</c>, or whose return type is not generic
    /// (no type arguments to extract the value type from), fall back to the
    /// default <c>Results.Ok(result)</c> / direct-deserialise behaviour and
    /// emit a BSF030 warning.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// [ServerFunctionCollection(ResultMapper = typeof(ResultMapper&lt;&gt;))]
    /// public interface IOrderService
    /// {
    ///     [ServerFunction(HttpMethod = "GET")]
    ///     Task&lt;Result&lt;OrderDto&gt;&gt; GetOrderAsync(Guid id);
    /// }
    /// </code>
    /// </example>
    public Type? ResultMapper { get; set; }
}
