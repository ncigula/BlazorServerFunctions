using BlazorServerFunctions.Generator.Models;
using Microsoft.CodeAnalysis;

namespace BlazorServerFunctions.Generator.Helpers;

internal static class InterfaceParser
{
    private static readonly DiagnosticDescriptor MissingServerFunctionCollectionAttribute = new(
        id: "BSF001",
        title: "Missing server function collection attribute",
        messageFormat: "Method '{0}' must have a [ServerFunctionCollection] attribute",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    private static readonly DiagnosticDescriptor MissingServerFunctionAttribute = new(
        id: "BSF002",
        title: "Missing server function attribute",
        messageFormat: "Method '{0}' must have a [ServerFunction] attribute",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    internal static Result<InterfaceInfo> ParseInterface(
        INamedTypeSymbol interfaceSymbol,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var serverFunctionCollectionAttribute = interfaceSymbol.GetAttributes()
            .FirstOrDefault(a => string.Equals(a.AttributeClass?.Name, "ServerFunctionCollectionAttribute", StringComparison.OrdinalIgnoreCase));

        if (serverFunctionCollectionAttribute is null)
            return Result.Failure<InterfaceInfo>(
                Error.Diagnostic(MissingServerFunctionCollectionAttribute, interfaceSymbol.Name));

        // Extract attribute parameters
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
            }
        }

        // Default route prefix from interface name (remove leading 'I')
        routePrefix ??= interfaceSymbol.Name.TrimStart('I').ToLowerInvariant();

        // Get namespace
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

        foreach (var member in interfaceSymbol.GetMembers())
        {
            if (member is not IMethodSymbol { MethodKind: MethodKind.Ordinary } methodSymbol)
                continue;

            var methodInfo = ParseMethod(interfaceInfo, methodSymbol, cancellationToken);
            if (methodInfo.IsSuccess)
            {
                interfaceInfo.Methods.Add(methodInfo.Value);
                continue;
            }

            return Result.Failure<InterfaceInfo>(methodInfo.Error);
        }

        return interfaceInfo;
    }

    private static Result<MethodInfo> ParseMethod(
        InterfaceInfo interfaceInfo,
        IMethodSymbol methodSymbol,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var methodInfo = new MethodInfo();
        methodInfo.Name = methodSymbol.Name;
        var returnType = methodSymbol.ReturnType.ToDisplayString();
        bool isAsync = RegexExpressions.IsAsyncType().IsMatch(returnType);

        methodInfo.AsyncType = (isAsync, returnType) switch
        {
            (false, _) => AsyncType.None,
            (true, var type) when type.StartsWith("ValueTask", StringComparison.Ordinal) => AsyncType.ValueTask,
            (true, var type) when type.StartsWith("Task", StringComparison.Ordinal) => AsyncType.Task,
            _ => throw new ArgumentOutOfRangeException(returnType),
        };

        if (isAsync && methodSymbol.ReturnType is INamedTypeSymbol namedType)
            methodInfo.ReturnType = namedType.TypeArguments.Length > 0
                ? namedType.TypeArguments[0].ToDisplayString()
                : "void";

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
            return Result.Failure<MethodInfo>(
                Error.Diagnostic(MissingServerFunctionAttribute, methodInfo.Name));

        foreach (var attribute in serverFunctionAttribute.NamedArguments)
        {
            switch (attribute.Key)
            {
                case "Route":
                    methodInfo.CustomRoute = attribute.Value.Value?.ToString();
                    break;
                case "HttpMethod":
                    methodInfo.HttpMethod = attribute.Value.Value!.ToString()!;
                    break;
                case "RequireAuthorization":
                    methodInfo.RequireAuthorization = attribute.Value.Value is true;
                    break;
            }
        }

        return methodInfo;
    }
}