using Microsoft.AspNetCore.Http;

namespace BlazorServerFunctions.Sample.Components.Filter;

/// <summary>
/// Demonstrates an <c>IEndpointFilter</c> used with <c>[ServerFunction(Filters = ...)]</c>.
/// Adds an <c>X-Correlation-Id</c> response header to every request that passes through it.
/// </summary>
internal sealed class AddCorrelationHeaderFilter : IEndpointFilter
{
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        context.HttpContext.Response.Headers["X-Correlation-Id"] = Guid.CreateVersion7().ToString();
        return next(context);
    }
}
