namespace BlazorServerFunctions.Generator.UnitTests.Helpers;

internal sealed class InterfaceInfoBuilder
{
    private readonly InterfaceInfo _info = new()
    {
        Name = "",
        Namespace = "",
        RoutePrefix = null,
        RequireAuthorization = false,
        Methods = new List<MethodInfo>()
    };

    internal InterfaceInfoBuilder WithName(string name)
    {
        _info.Name = name;
        return this;
    }

    internal InterfaceInfoBuilder WithNamespace(string ns)
    {
        _info.Namespace = ns;
        return this;
    }

    internal InterfaceInfoBuilder WithRoutePrefix(string? prefix)
    {
        _info.RoutePrefix = prefix;
        return this;
    }

    internal InterfaceInfoBuilder RequiresAuthorization(bool required = true)
    {
        _info.RequireAuthorization = required;
        return this;
    }

    internal InterfaceInfoBuilder WithMethod(MethodInfo method)
    {
        _info.Methods.Add(method);
        return this;
    }

    internal InterfaceInfoBuilder WithMethods(params MethodInfo[] methods)
    {
        _info.Methods.AddRange(methods);
        return this;
    }

    internal InterfaceInfo Build() =>
        new InterfaceInfo
        {
            Name = _info.Name,
            Namespace = _info.Namespace,
            RoutePrefix = _info.RoutePrefix,
            RequireAuthorization = _info.RequireAuthorization,
            Methods = _info.Methods.ToList()
        };
}