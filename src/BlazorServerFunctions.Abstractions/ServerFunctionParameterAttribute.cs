namespace BlazorServerFunctions.Abstractions;

/// <summary>
/// Overrides the automatic parameter binding inference for a single method parameter.
/// Apply to parameters of methods marked with <see cref="ServerFunctionAttribute"/>.
/// </summary>
/// <remarks>
/// <para>
/// By default the generator infers binding:
/// parameters whose names match a <c>{token}</c> in the route template are route-bound;
/// remaining parameters on GET/DELETE go to the query string;
/// remaining parameters on POST/PUT/PATCH go to the JSON request body.
/// </para>
/// <para>
/// Use this attribute when you need a different binding, for example reading a tenant ID
/// from an HTTP header:
/// </para>
/// <code>
/// [ServerFunction(HttpMethod = "POST")]
/// Task&lt;Order&gt; CreateOrderAsync(
///     [ServerFunctionParameter(From = ParameterSource.Header, Name = "X-Tenant-Id")] string tenantId,
///     string productId,
///     int quantity);
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class ServerFunctionParameterAttribute : Attribute
{
    /// <summary>
    /// The explicit binding source. Defaults to <see cref="ParameterSource.Auto"/> (inferred).
    /// </summary>
    public ParameterSource From { get; set; } = ParameterSource.Auto;

    /// <summary>
    /// Optional custom name used for binding.
    /// <list type="bullet">
    /// <item><description><see cref="ParameterSource.Header"/>: the HTTP header name (e.g. <c>"X-Tenant-Id"</c>). Defaults to the C# parameter name.</description></item>
    /// <item><description><see cref="ParameterSource.Query"/>: the query string key. Defaults to the PascalCase C# parameter name.</description></item>
    /// <item><description>Other sources: ignored.</description></item>
    /// </list>
    /// </summary>
    public string? Name { get; set; }
}
