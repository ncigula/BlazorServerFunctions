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
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MissingServerFunctionCollectionAttribute,
                interfaceSymbol.Locations.FirstOrDefault(), interfaceSymbol.Name));

        if (!ValidateInterfaceDeclaration(context, interfaceSymbol))
            return new InterfaceInfo { Name = interfaceSymbol.Name };

        var (routePrefix, requireAuth) = ParseCollectionAttributeArgs(serverFunctionCollectionAttribute, interfaceSymbol);

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

        // BSF101: Empty interface warning (doesn't block generation)
        if (interfaceInfo.Methods.Count == 0)
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.EmptyInterface,
                interfaceSymbol.Locations.FirstOrDefault(), interfaceSymbol.Name));

        return interfaceInfo;
    }

    /// <summary>Validates BSF003-BSF006. Returns false if fatal (should abort parsing).</summary>
    private static bool ValidateInterfaceDeclaration(SourceProductionContextWrapper context, INamedTypeSymbol interfaceSymbol)
    {
        // BSF003: Interface must be public
        if (interfaceSymbol.DeclaredAccessibility != Accessibility.Public)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InterfaceMustBePublic,
                interfaceSymbol.Locations.FirstOrDefault(), interfaceSymbol.Name));
            return false;
        }

        // BSF004: Interface cannot be generic
        if (interfaceSymbol.TypeParameters.Length > 0)
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InterfaceCannotBeGeneric,
                interfaceSymbol.Locations.FirstOrDefault(), interfaceSymbol.Name));

        // BSF005: Interface cannot have properties
        if (interfaceSymbol.GetMembers().OfType<IPropertySymbol>().Any())
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InterfaceCannotHaveProperties,
                interfaceSymbol.Locations.FirstOrDefault(), interfaceSymbol.Name));

        // BSF006: Interface cannot have events
        if (interfaceSymbol.GetMembers().OfType<IEventSymbol>().Any())
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InterfaceCannotHaveEvents,
                interfaceSymbol.Locations.FirstOrDefault(), interfaceSymbol.Name));

        return true;
    }

    private static (string RoutePrefix, bool RequireAuth) ParseCollectionAttributeArgs(
        AttributeData? attribute,
        INamedTypeSymbol interfaceSymbol)
    {
        string? routePrefix = null;
        bool requireAuth = false;

        foreach (var namedArg in attribute?.NamedArguments ?? [])
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

        return (routePrefix, requireAuth);
    }

    private static IEnumerable<MethodInfo> ParseMethods(
        SourceProductionContextWrapper context,
        InterfaceInfo interfaceInfo,
        INamedTypeSymbol interfaceSymbol)
    {
        var seenRoutes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var member in interfaceSymbol.GetMembers())
        {
            if (member is not IMethodSymbol { MethodKind: MethodKind.Ordinary } methodSymbol)
                continue;

            var methodInfo = ParseMethod(context, interfaceInfo, methodSymbol);

            // BSF014: Duplicate route check
            var route = methodInfo.CustomRoute ?? methodInfo.Name;
            if (seenRoutes.TryGetValue(route, out var existingMethodName))
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.DuplicateRoute,
                    methodSymbol.Locations.First(), methodInfo.Name, route, existingMethodName));
            else
                seenRoutes[route] = methodInfo.Name;

            yield return methodInfo;
        }
    }

    private static MethodInfo ParseMethod(
        SourceProductionContextWrapper context,
        InterfaceInfo interfaceInfo,
        IMethodSymbol methodSymbol)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        // BSF008: Method cannot be generic
        if (methodSymbol.TypeParameters.Length > 0)
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MethodCannotBeGeneric,
                methodSymbol.Locations.First(), methodSymbol.Name));

        ValidateParameterModifiers(context, methodSymbol);

        // BSF102: Too many parameters warning (doesn't block generation)
        if (methodSymbol.Parameters.Length > 5)
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.TooManyParameters,
                methodSymbol.Locations.First(), methodSymbol.Name, methodSymbol.Parameters.Length));

        var methodInfo = new MethodInfo
        {
            Name = methodSymbol.Name,
            RequireAuthorization = interfaceInfo.RequireAuthorization
        };

        (methodInfo.AsyncType, methodInfo.ReturnType) = ParseReturnType(context, methodSymbol);
        methodInfo.HasCancellationToken = methodSymbol.Parameters.Any(p =>
            string.Equals(p.Type.ToDisplayString(), "System.Threading.CancellationToken", StringComparison.Ordinal));
        methodInfo.Parameters = ParseParameters(methodSymbol);

        var serverFunctionAttribute = GetServerFunctionAttribute(context, methodSymbol, methodInfo);
        ParseServerFunctionAttributes(context, methodSymbol, methodInfo, serverFunctionAttribute);

        return methodInfo;
    }

    /// <summary>Validates BSF009/BSF010/BSF011 parameter modifiers.</summary>
    private static void ValidateParameterModifiers(SourceProductionContextWrapper context, IMethodSymbol methodSymbol)
    {
        foreach (var param in methodSymbol.Parameters)
        {
            if (param.RefKind == RefKind.Out)
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.OutParametersNotSupported,
                    param.Locations.FirstOrDefault() ?? methodSymbol.Locations.First(),
                    methodSymbol.Name, param.Name));
            else if (param.RefKind == RefKind.Ref)
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.RefParametersNotSupported,
                    param.Locations.FirstOrDefault() ?? methodSymbol.Locations.First(),
                    methodSymbol.Name, param.Name));

            if (param.IsParams)
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ParamsNotSupported,
                    param.Locations.FirstOrDefault() ?? methodSymbol.Locations.First(),
                    methodSymbol.Name, param.Name));
        }
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
                    ValidateRouteFormat(context, methodSymbol, methodInfo);
                    break;

                case "HttpMethod":
                    if (attribute.Value.Kind is not TypedConstantKind.Error && attribute.Value.Value != null)
                    {
                        var httpMethodStr = attribute.Value.Value.ToString()!;
                        if (Enum.TryParse<HttpMethod>(httpMethodStr, ignoreCase: true, out var parsedMethod))
                        {
                            methodInfo.HttpMethod = parsedMethod;
                            break;
                        }
                    }

                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InvalidHttpMethod,
                        methodSymbol.Locations.First(), methodInfo.Name,
                        attribute.Value.Value?.ToString() ?? "null"));
                    break;

                case "RequireAuthorization":
                    methodInfo.RequireAuthorization = attribute.Value.Value is true;
                    break;
            }
        }
    }

    /// <summary>BSF015: Validates route format after setting CustomRoute.</summary>
    private static void ValidateRouteFormat(
        SourceProductionContextWrapper context,
        IMethodSymbol methodSymbol,
        MethodInfo methodInfo)
    {
        if (methodInfo.CustomRoute != null &&
            (string.IsNullOrWhiteSpace(methodInfo.CustomRoute) ||
             methodInfo.CustomRoute.Any(char.IsWhiteSpace)))
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InvalidRouteFormat,
                methodSymbol.Locations.First(), methodSymbol.Name, methodInfo.CustomRoute));
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
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MissingServerFunctionAttribute,
                methodSymbol.Locations.First(), methodInfo.Name, methodSymbol.Name));

        if (serverFunctionAttribute is not null && !serverFunctionAttribute.NamedArguments.Any(kvp => string.Equals(kvp.Key, "HttpMethod", StringComparison.Ordinal)))
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.HttpMethodRequired,
                methodSymbol.Locations.First(), methodInfo.Name));

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

    private static (AsyncType AsyncType, string ReturnType) ParseReturnType(
        SourceProductionContextWrapper context,
        IMethodSymbol methodSymbol)
    {
        var returnType = methodSymbol.ReturnType.ToDisplayString();
        bool isAsync = RegexExpressions.IsAsyncTypeRegex.IsMatch(returnType);

        // BSF007: Method must return Task or ValueTask
        if (!isAsync)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InvalidReturnType,
                methodSymbol.Locations.First(), methodSymbol.Name));
            return (AsyncType.Task, "void");
        }

        var asyncType = returnType.Contains("ValueTask", StringComparison.Ordinal)
            ? AsyncType.ValueTask
            : AsyncType.Task;

        if (methodSymbol.ReturnType is INamedTypeSymbol namedType)
            returnType = namedType.TypeArguments.Length > 0
                ? namedType.TypeArguments[0].ToDisplayString()
                : "void";

        return (asyncType, returnType);
    }
}
