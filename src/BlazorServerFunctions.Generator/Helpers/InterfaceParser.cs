using BlazorServerFunctions.Generator.Models;
using Microsoft.CodeAnalysis;
using HttpMethod = BlazorServerFunctions.Generator.Models.HttpMethod;

namespace BlazorServerFunctions.Generator.Helpers;

internal static class InterfaceParser
{
    internal static InterfaceInfo ParseInterface(
        SourceProductionContextWrapper context,
        INamedTypeSymbol interfaceSymbol)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        var serverFunctionCollectionAttribute = interfaceSymbol.GetAttributes()
            .FirstOrDefault(a => string.Equals(a.AttributeClass?.Name, "ServerFunctionCollectionAttribute", StringComparison.OrdinalIgnoreCase));

        if (serverFunctionCollectionAttribute is null)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.MissingServerFunctionCollectionAttribute,
                    interfaceSymbol.Locations.FirstOrDefault(),
                    interfaceSymbol.Name));
        }

        string? routePrefix = null;
        bool requireAuth = false;

        foreach (var namedArg in serverFunctionCollectionAttribute?.NamedArguments ?? [])
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

        // Strip leading slash so route generation doesn't produce double-slashes
        // e.g. [ServerFunctionCollection(RoutePrefix = "/users")] → "users"
        routePrefix = routePrefix?.TrimStart('/');

        routePrefix ??= interfaceSymbol.Name.TrimStart('I').ToLowerInvariant();

        var namespaceName = interfaceSymbol.ContainingNamespace.IsGlobalNamespace
            ? "Generated"
            : interfaceSymbol.ContainingNamespace.ToDisplayString();

        var interfaceInfo = new InterfaceInfo
        {
            Name = interfaceSymbol.Name,
            Namespace = namespaceName,
            RoutePrefix = routePrefix,
            RequireAuthorization = requireAuth,
        };

        interfaceInfo.Methods.AddRange(ParseMethods(context, interfaceInfo, interfaceSymbol));

        return interfaceInfo;
    }

    private static IEnumerable<MethodInfo> ParseMethods(
        SourceProductionContextWrapper context,
        InterfaceInfo interfaceInfo,
        INamedTypeSymbol interfaceSymbol)
    {
        foreach (var member in interfaceSymbol.GetMembers())
        {
            if (member is not IMethodSymbol { MethodKind: MethodKind.Ordinary } methodSymbol)
                continue;

            var methodInfo = ParseMethod(context, interfaceInfo, methodSymbol);

            yield return methodInfo;
        }
    }

    private static MethodInfo ParseMethod(
        SourceProductionContextWrapper context,
        InterfaceInfo interfaceInfo,
        IMethodSymbol methodSymbol)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        var methodInfo = new MethodInfo
        {
            Name = methodSymbol.Name,
            RequireAuthorization = interfaceInfo.RequireAuthorization
        };

        (methodInfo.AsyncType, methodInfo.ReturnType) = ParseReturnType(methodSymbol);
        methodInfo.HasCancellationToken = methodSymbol.Parameters.Any(p =>
            string.Equals(p.Type.ToDisplayString(), "System.Threading.CancellationToken", StringComparison.Ordinal));
        methodInfo.Parameters = ParseParameters(methodSymbol);

        var serverFunctionAttribute = GetServerFunctionAttribute(context, methodSymbol, methodInfo);
        ParseServerFunctionAttributes(context, methodSymbol, methodInfo, serverFunctionAttribute);

        return methodInfo;
    }

    private static void ParseServerFunctionAttributes(
        SourceProductionContextWrapper context,
        IMethodSymbol methodSymbol,
        MethodInfo methodInfo,
        AttributeData? serverFunctionAttribute)
    {
        foreach (var attribute in serverFunctionAttribute?.NamedArguments ?? [])
        {
            switch (attribute.Key)
            {
                case "Route":
                    methodInfo.CustomRoute = attribute.Value.Value?.ToString();
                    break;

                case "HttpMethod":
                    if (attribute.Value.Kind is not TypedConstantKind.Error && attribute.Value.Value != null)
                    {
                        methodInfo.HttpMethod = Enum.Parse<HttpMethod>(attribute.Value.Value!.ToString()!, ignoreCase: true);
                        break;
                    }

                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.InvalidHttpMethod,
                            methodSymbol.Locations.First(),
                            methodInfo.Name,
                            attribute.Value.Value?.ToString() ?? "null"));
                    break;

                case "RequireAuthorization":
                    methodInfo.RequireAuthorization = attribute.Value.Value is true;
                    break;
            }
        }
    }
    
    private static AttributeData? GetServerFunctionAttribute(
        SourceProductionContextWrapper context,
        IMethodSymbol methodSymbol,
        MethodInfo methodInfo)
    {
        var serverFunctionAttribute = methodSymbol.GetAttributes()
            .FirstOrDefault(a => string.Equals(a.AttributeClass?.Name, "ServerFunctionAttribute", StringComparison.OrdinalIgnoreCase));

        if (serverFunctionAttribute is null)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.MissingServerFunctionAttribute,
                    methodSymbol.Locations.First(),
                    methodInfo.Name,
                    methodSymbol.Name));
        }

        if (serverFunctionAttribute is not null && !serverFunctionAttribute.NamedArguments.Any(kvp => string.Equals(kvp.Key, "HttpMethod", StringComparison.Ordinal)))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.HttpMethodRequired,
                    methodSymbol.Locations.First(),
                    methodInfo.Name));
        }

        return serverFunctionAttribute;
    }

    private static List<ParameterInfo> ParseParameters(IMethodSymbol methodSymbol)
    {
        return methodSymbol.Parameters
            .Where(p => !string.Equals(p.Type.ToDisplayString(), "System.Threading.CancellationToken", StringComparison.Ordinal))
            .Select(parameter => new ParameterInfo
            {
                Name = parameter.Name,
                Type = parameter.Type.ToDisplayString(),
                HasDefaultValue = parameter.HasExplicitDefaultValue,
                DefaultValue = parameter.HasExplicitDefaultValue && parameter.ExplicitDefaultValue is not null
                    ? Convert.ToString(parameter.ExplicitDefaultValue, System.Globalization.CultureInfo.InvariantCulture)
                    : null,
            })
            .ToList();
    }

    private static (AsyncType AsyncType, string ReturnType) ParseReturnType(IMethodSymbol methodSymbol)
    {
        var returnType = methodSymbol.ReturnType.ToDisplayString();
        bool isAsync = RegexExpressions.IsAsyncType().IsMatch(returnType);

        var asyncType = (isAsync, returnType) switch
        {
            (false, _) => AsyncType.None,
            (true, var type) when type.Contains("ValueTask", StringComparison.Ordinal) => AsyncType.ValueTask,
            (true, var type) when type.Contains("Task", StringComparison.Ordinal) => AsyncType.Task,
            _ => throw new ArgumentOutOfRangeException(returnType),
        };

        if (isAsync && methodSymbol.ReturnType is INamedTypeSymbol namedType)
            returnType = namedType.TypeArguments.Length > 0
                ? namedType.TypeArguments[0].ToDisplayString()
                : "void";

        return (asyncType, returnType);
    }
}