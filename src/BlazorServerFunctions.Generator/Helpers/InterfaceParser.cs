using BlazorServerFunctions.Generator.Models;
using Microsoft.CodeAnalysis;
using HttpMethod = BlazorServerFunctions.Generator.Models.HttpMethod;

namespace BlazorServerFunctions.Generator.Helpers;

internal static class InterfaceParser
{
    internal static InterfaceInfo? ParseInterface(
        SourceProductionContext context,
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
            
            return null;
        }

        string? routePrefix = null;
        bool requireAuth = false;

        foreach (var namedArg in serverFunctionCollectionAttribute.NamedArguments)
        {
            switch (namedArg.Key)
            {
                case "RoutePrefix":
                    routePrefix = namedArg.Value.Value?.ToString();
                    break;
                case "RequireAuthorization":
                    requireAuth = namedArg.Value.Value is true;
                    break;
                default:
                    routePrefix = interfaceSymbol.Name.TrimStart('I').ToLowerInvariant();
                    break;
            }
        }

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
        SourceProductionContext context,
        InterfaceInfo interfaceInfo,
        INamedTypeSymbol interfaceSymbol)
    {
        foreach (var member in interfaceSymbol.GetMembers())
        {
            if (member is not IMethodSymbol { MethodKind: MethodKind.Ordinary } methodSymbol)
                continue;

            var methodInfo = ParseMethod(context, interfaceInfo, methodSymbol);
            
            if (methodInfo is null)
                continue;

            yield return methodInfo;
        }
    }

    private static MethodInfo? ParseMethod(
        SourceProductionContext context,
        InterfaceInfo interfaceInfo,
        IMethodSymbol methodSymbol)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        var methodInfo = new MethodInfo();
        methodInfo.Name = methodSymbol.Name;
        (methodInfo.AsyncType, methodInfo.ReturnType) = ParseReturnType(methodSymbol);

        methodInfo.Parameters = methodSymbol.Parameters
            .Select(parameter => new ParameterInfo
            {
                Name = parameter.Name,
                Type = parameter.Type.ToDisplayString(),
                HasDefaultValue = parameter.HasExplicitDefaultValue,
                DefaultValue = parameter.HasExplicitDefaultValue ? parameter.ExplicitDefaultValue?.ToString() : null,
            })
            .ToList();

        methodInfo.RequireAuthorization = interfaceInfo.RequireAuthorization;

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

            return null;
        }

        foreach (var attribute in serverFunctionAttribute.NamedArguments)
        {
            if (string.Equals(attribute.Key, "Route", StringComparison.Ordinal))
                methodInfo.CustomRoute = attribute.Value.Value?.ToString();
            
            if (string.Equals(attribute.Key, "HttpMethod", StringComparison.Ordinal))
                methodInfo.HttpMethod = Enum.Parse<HttpMethod>(attribute.Value.Value!.ToString()!);
            
            if (string.Equals(attribute.Key, "RequireAuthorization", StringComparison.Ordinal))
                methodInfo.RequireAuthorization = attribute.Value.Value is true;
        }

        return methodInfo;
    }

    private static (AsyncType AsyncType, string ReturnType) ParseReturnType(IMethodSymbol methodSymbol)
    {
        var returnType = methodSymbol.ReturnType.ToDisplayString();
        bool isAsync = RegexExpressions.IsAsyncType().IsMatch(returnType);

        var asyncType = (isAsync, returnType) switch
        {
            (false, _) => AsyncType.None,
            (true, var type) when type.StartsWith("ValueTask", StringComparison.Ordinal) => AsyncType.ValueTask,
            (true, var type) when type.StartsWith("Task", StringComparison.Ordinal) => AsyncType.Task,
            _ => throw new ArgumentOutOfRangeException(returnType),
        };

        if (isAsync && methodSymbol.ReturnType is INamedTypeSymbol namedType)
            returnType = namedType.TypeArguments.Length > 0
                ? namedType.TypeArguments[0].ToDisplayString()
                : "void";
        
        return (asyncType, returnType);
    }
}