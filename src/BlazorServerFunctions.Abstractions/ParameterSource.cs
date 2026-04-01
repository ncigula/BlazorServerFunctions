namespace BlazorServerFunctions.Abstractions;

/// <summary>
/// Controls how a method parameter is bound when the generator cannot infer the correct binding,
/// or when you want to make the binding explicit and validated at compile time.
/// </summary>
/// <remarks>
/// Apply via <see cref="ServerFunctionParameterAttribute"/>:
/// <code>
/// [ServerFunction(HttpMethod = "POST")]
/// Task&lt;Order&gt; CreateOrderAsync(
///     [ServerFunctionParameter(From = ParameterSource.Header, Name = "X-Tenant-Id")] string tenantId,
///     string productId,
///     int quantity);
/// </code>
/// </remarks>
public enum ParameterSource
{
    /// <summary>
    /// Default. The generator infers the binding source:
    /// route-template match → route; GET/DELETE → query string; POST/PUT/PATCH → JSON body.
    /// </summary>
    Auto,

    /// <summary>
    /// Asserts that this parameter must appear as <c>{paramName}</c> in the route template.
    /// Generation is identical to the inferred route binding; this source is a compile-time
    /// validation marker. Diagnostic BSF031 fires if the template does not contain the parameter.
    /// </summary>
    Route,

    /// <summary>
    /// Binds from the URL query string (<c>[FromQuery]</c> on the server; query string on the client).
    /// Useful to force a query-string binding on a POST/PUT/PATCH method where the default would be <c>[FromBody]</c>.
    /// </summary>
    Query,

    /// <summary>
    /// Binds from the JSON request body (<c>[FromBody]</c> on the server; JSON body on the client).
    /// Useful to force a body binding on a GET/DELETE method.
    /// Diagnostic BSF032 (warning) fires because GET/DELETE requests should not have a body.
    /// </summary>
    Body,

    /// <summary>
    /// Binds from an HTTP request header (<c>[FromHeader(Name = "...")]</c> on the server;
    /// <c>requestMessage.Headers.Add("...", value)</c> on the client).
    /// Use the <see cref="ServerFunctionParameterAttribute.Name"/> property to specify the HTTP header name
    /// (e.g. <c>"X-Tenant-Id"</c>). When <c>Name</c> is omitted the C# parameter name is used as-is.
    /// </summary>
    Header,
}
