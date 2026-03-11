namespace BlazorServerFunctions.Generator.UnitTests.Helpers;

internal sealed class MethodInfoBuilder
{
    private readonly MethodInfo _method = new()
    {
        Name = "",
        ReturnType = "void",
        CustomRoute = null,
        RequireAuthorization = false,
        HttpMethod = "POST",
        AsyncType = AsyncType.Task,
        Parameters = new List<ParameterInfo>()
    };

    internal MethodInfoBuilder WithName(string name)
    {
        _method.Name = name;
        return this;
    }

    internal MethodInfoBuilder Returning(string returnType)
    {
        _method.ReturnType = returnType;
        return this;
    }

    internal MethodInfoBuilder WithCustomRoute(string? route)
    {
        _method.CustomRoute = route;
        return this;
    }

    internal MethodInfoBuilder RequiresAuthorization(bool required = true)
    {
        _method.RequireAuthorization = required;
        return this;
    }

    internal MethodInfoBuilder UsingHttp(string httpMethod)
    {
        _method.HttpMethod = httpMethod;
        return this;
    }

    internal MethodInfoBuilder IsAsyncMethod(AsyncType asyncType = AsyncType.Task)
    {
        _method.AsyncType = asyncType;
        return this;
    }

    internal MethodInfoBuilder WithParameter(ParameterInfo parameter)
    {
        _method.Parameters.Add(parameter);
        return this;
    }

    internal MethodInfoBuilder WithParameters(params ParameterInfo[] parameters)
    {
        _method.Parameters.AddRange(parameters);
        return this;
    }

    internal MethodInfoBuilder WithCancellationToken(bool value = true)
    {
        _method.HasCancellationToken = value;
        return this;
    }

    internal MethodInfo Build() =>
        new MethodInfo
        {
            Name = _method.Name,
            ReturnType = _method.ReturnType,
            CustomRoute = _method.CustomRoute,
            RequireAuthorization = _method.RequireAuthorization,
            HttpMethod = _method.HttpMethod,
            AsyncType = _method.AsyncType,
            HasCancellationToken = _method.HasCancellationToken,
            Parameters = _method.Parameters.ToList()
        };
}