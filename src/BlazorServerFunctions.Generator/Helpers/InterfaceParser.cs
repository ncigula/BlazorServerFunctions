using BlazorServerFunctions.Generator.Models;
using Microsoft.CodeAnalysis;

namespace BlazorServerFunctions.Generator.Helpers;

public static class InterfaceParser
{
    internal static InterfaceInfo? ParseInterface(
        INamedTypeSymbol interfaceSymbol,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Get [ServerFunctionCollection] attribute data
        var attribute = interfaceSymbol.GetAttributes()
            .FirstOrDefault(a => string.Equals(a.AttributeClass?.Name, "ServerFunctionCollectionAttribute", StringComparison.OrdinalIgnoreCase));

        if (attribute is null)
            return null;

        // Extract attribute parameters
        string? routePrefix = null;
        bool requireAuth = false;

        foreach (var namedArg in attribute.NamedArguments)
        {
            switch (namedArg.Key)
            {
                case "RoutePrefix":
                    routePrefix = namedArg.Value.Value?.ToString();
                    break;
                case "RequireAuthorization":
                    requireAuth = namedArg.Value.Value is true;
                    break;
            }
        }

        // Default route prefix from interface name (remove leading 'I')
        routePrefix ??= interfaceSymbol.Name.TrimStart('I').ToLowerInvariant();

        // Get namespace
        var namespaceName = interfaceSymbol.ContainingNamespace.IsGlobalNamespace
            ? "Generated"
            : interfaceSymbol.ContainingNamespace.ToDisplayString();

        // Parse methods
        var methods = new List<MethodInfo>();
        foreach (var member in interfaceSymbol.GetMembers())
        {
            if (member is not IMethodSymbol methodSymbol)
                continue;

            var methodInfo = ParseMethod(methodSymbol, cancellationToken);
            if (methodInfo is not null)
            {
                methods.Add(methodInfo);
            }
        }

        return new InterfaceInfo
        {
            Name = interfaceSymbol.Name,
            Namespace = namespaceName,
            RoutePrefix = routePrefix,
            RequireAuthorization = requireAuth,
            Methods = methods
        };
    }

    private static MethodInfo? ParseMethod(
        IMethodSymbol methodSymbol,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (methodSymbol.MethodKind != MethodKind.Ordinary)
            return null;

        var returnType = methodSymbol.ReturnType.ToDisplayString();
        bool isAsync = returnType.StartsWith("System.Threading.Tasks.Task", StringComparison.InvariantCulture);

        if (isAsync && methodSymbol.ReturnType is INamedTypeSymbol namedType)
            returnType = namedType.TypeArguments.Length > 0
                ? namedType.TypeArguments[0].ToDisplayString()
                : "void"; // Task with no result

        var methodAttribute = methodSymbol.GetAttributes()
            .FirstOrDefault(a => string.Equals(a.AttributeClass?.Name, "ServerFunctionAttribute", StringComparison.OrdinalIgnoreCase));

        string? customRoute = null;
        bool requireAuthorization = false;
        string httpMethod = "POST";

        if (methodAttribute is not null)
        {
            foreach (var namedArg in methodAttribute.NamedArguments)
            {
                switch (namedArg.Key)
                {
                    case "Route":
                        customRoute = namedArg.Value.Value?.ToString();
                        break;
                    case "HttpMethod":
                        httpMethod = namedArg.Value.Value?.ToString() ?? "POST";
                        break;
                    case "RequireAuthorization":
                        requireAuthorization = namedArg.Value.Value is true;
                        break;
                }
            }
        }

        var parameters = methodSymbol.Parameters
            .Select(parameter => new ParameterInfo
            {
                Name = parameter.Name,
                Type = parameter.Type.ToDisplayString(),
                HasDefaultValue = parameter.HasExplicitDefaultValue,
                DefaultValue = parameter.HasExplicitDefaultValue ? parameter.ExplicitDefaultValue?.ToString() : null,
            })
            .ToList();

        return new MethodInfo
        {
            Name = methodSymbol.Name,
            ReturnType = returnType,
            IsAsync = isAsync,
            RequireAuthorization = requireAuthorization,
            Parameters = parameters,
            CustomRoute = customRoute,
            HttpMethod = httpMethod,
        };
    }
}