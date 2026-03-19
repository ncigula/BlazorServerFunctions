using System.Collections.Generic;
using System.Text.RegularExpressions;
using BlazorServerFunctions.Generator.Models;
using Microsoft.CodeAnalysis;
using HttpMethod = BlazorServerFunctions.Generator.Models.HttpMethod;

namespace BlazorServerFunctions.Generator.Helpers;

internal static class InterfaceParser
{
    internal static InterfaceInfo ParseInterface(
        SourceProductionContextWrapper context,
        INamedTypeSymbol interfaceSymbol,
        Compilation? compilation = null)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        var serverFunctionCollectionAttribute = interfaceSymbol.GetAttributes()
            .FirstOrDefault(a => string.Equals(a.AttributeClass?.Name, "ServerFunctionCollectionAttribute", StringComparison.OrdinalIgnoreCase));

        if (serverFunctionCollectionAttribute is null)
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MissingServerFunctionCollectionAttribute,
                interfaceSymbol.Locations.FirstOrDefault(), interfaceSymbol.Name));

        if (!ValidateInterfaceDeclaration(context, interfaceSymbol))
            return new InterfaceInfo { Name = interfaceSymbol.Name };

        var (routePrefix, requireAuth, configuration) = ParseCollectionAttributeArgs(serverFunctionCollectionAttribute, interfaceSymbol, compilation);

        var namespaceName = interfaceSymbol.ContainingNamespace.IsGlobalNamespace
            ? "Generated"
            : interfaceSymbol.ContainingNamespace.ToDisplayString();

        var interfaceInfo = new InterfaceInfo
        {
            Name = interfaceSymbol.Name,
            Namespace = namespaceName,
            RoutePrefix = routePrefix,
            RequireAuthorization = requireAuth,
            Configuration = configuration,
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

    private static (string RoutePrefix, bool RequireAuth, ConfigurationInfo Configuration) ParseCollectionAttributeArgs(
        AttributeData? attribute,
        INamedTypeSymbol interfaceSymbol,
        Compilation? compilation = null)
    {
        string? routePrefix = null;
        bool requireAuth = false;
        var configuration = ConfigurationInfo.Default;

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
                case "Configuration":
                    if (namedArg.Value.Value is INamedTypeSymbol configSymbol)
                        configuration = ConfigurationReader.ReadConfiguration(configSymbol, interfaceSymbol, compilation);
                    break;
            }
        }

        // Strip leading slash so route generation doesn't produce double-slashes
        // e.g. [ServerFunctionCollection(RoutePrefix = "/users")] → "users"
        routePrefix = routePrefix?.TrimStart('/');
        routePrefix ??= interfaceSymbol.Name.TrimStart('I').ToLowerInvariant();

        return (routePrefix, requireAuth, configuration);
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

            // BSF014: Duplicate route check — keyed on (httpMethod, route) so that
            // different verbs on the same path (GET /items/{id} vs DELETE /items/{id}) are valid.
            var route = methodInfo.CustomRoute ?? methodInfo.Name;
            var routeKey = $"{methodInfo.HttpMethod}:{route}";
            if (seenRoutes.TryGetValue(routeKey, out var existingMethodName))
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.DuplicateRoute,
                    methodSymbol.Locations.First(), methodInfo.Name, route, existingMethodName));
            else
                seenRoutes[routeKey] = methodInfo.Name;

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

        var serverFunctionAttribute = GetServerFunctionAttribute(context, methodSymbol, methodInfo, interfaceInfo);
        ParseServerFunctionAttributes(context, methodSymbol, methodInfo, serverFunctionAttribute, interfaceInfo);
        ValidateCacheSeconds(context, methodSymbol, methodInfo);

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
        AttributeData? serverFunctionAttribute,
        InterfaceInfo interfaceInfo)
    {
        bool hasExplicitHttpMethod = false;
        int methodCacheSeconds = -1; // -1 = not set on attribute → inherit from config
        string? rawMethodRateLimitPolicy = null; // null = not set on attribute → inherit from config

        foreach (var attribute in serverFunctionAttribute?.NamedArguments ?? [])
        {
            switch (attribute.Key)
            {
                case "Route":
                    methodInfo.CustomRoute = attribute.Value.Value?.ToString();
                    ValidateRouteFormat(context, methodSymbol, methodInfo);
                    ValidateAndMarkRouteParameters(context, methodSymbol, methodInfo);
                    break;

                case "HttpMethod":
                    hasExplicitHttpMethod = true;
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

                case "CacheSeconds":
                    methodCacheSeconds = attribute.Value.Value is int v ? v : -1;
                    break;

                case "RateLimitPolicy":
                    rawMethodRateLimitPolicy = attribute.Value.Value?.ToString();
                    break;
            }
        }

        ResolveFromConfig(methodInfo, interfaceInfo, hasExplicitHttpMethod, methodCacheSeconds, rawMethodRateLimitPolicy);
    }

    private static void ResolveFromConfig(
        MethodInfo methodInfo,
        InterfaceInfo interfaceInfo,
        bool hasExplicitHttpMethod,
        int methodCacheSeconds,
        string? rawMethodRateLimitPolicy)
    {
        // Apply DefaultHttpMethod from config when no explicit method was provided on this [ServerFunction]
        if (!hasExplicitHttpMethod
            && interfaceInfo.Configuration.DefaultHttpMethod is not null
            && Enum.TryParse<HttpMethod>(interfaceInfo.Configuration.DefaultHttpMethod, ignoreCase: true, out var defaultMethod))
        {
            methodInfo.HttpMethod = defaultMethod;
        }

        // Resolve CacheSeconds: -1 means "not set on attribute" → fall back to config default
        methodInfo.CacheSeconds = methodCacheSeconds == -1
            ? interfaceInfo.Configuration.CacheSeconds
            : methodCacheSeconds;

        // Resolve RateLimitPolicy: null = not set → inherit config; "" = explicit disable → null; non-empty = use it
        methodInfo.RateLimitPolicy = rawMethodRateLimitPolicy switch
        {
            null => interfaceInfo.Configuration.RateLimitPolicy,
            "" => null,
            _ => rawMethodRateLimitPolicy
        };
    }

    /// <summary>
    /// Validates the resolved CacheSeconds value.
    /// BSF019: streaming methods cannot be cached (warning, clamp to 0).
    /// BSF020: non-GET methods cannot be cached (error, clamp to 0).
    /// Must be called after both HttpMethod and AsyncType are resolved.
    /// </summary>
    private static void ValidateCacheSeconds(
        SourceProductionContextWrapper context,
        IMethodSymbol methodSymbol,
        MethodInfo methodInfo)
    {
        if (methodInfo.CacheSeconds <= 0)
            return;

        if (methodInfo.AsyncType is AsyncType.AsyncEnumerable)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.CacheOnStreamingMethod,
                methodSymbol.Locations.First(), methodInfo.Name));
            methodInfo.CacheSeconds = 0;
            return;
        }

        if (methodInfo.HttpMethod is not HttpMethod.Get)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.CacheOnNonGetMethod,
                methodSymbol.Locations.First(), methodInfo.Name,
                methodInfo.HttpMethod.ToString().ToUpperInvariant()));
            methodInfo.CacheSeconds = 0;
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
        MethodInfo methodInfo,
        InterfaceInfo interfaceInfo)
    {
        var serverFunctionAttribute = methodSymbol.GetAttributes()
            .FirstOrDefault(a => string.Equals(a.AttributeClass?.Name, "ServerFunctionAttribute", StringComparison.OrdinalIgnoreCase));

        if (serverFunctionAttribute is null)
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MissingServerFunctionAttribute,
                methodSymbol.Locations.First(), methodInfo.Name, methodSymbol.Name));

        // BSF013: HttpMethod is required unless a DefaultHttpMethod is set on the config
        bool hasExplicitHttpMethod = serverFunctionAttribute is not null
            && serverFunctionAttribute.NamedArguments.Any(kvp => string.Equals(kvp.Key, "HttpMethod", StringComparison.Ordinal));

        if (serverFunctionAttribute is not null
            && !hasExplicitHttpMethod
            && interfaceInfo.Configuration.DefaultHttpMethod is null)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.HttpMethodRequired,
                methodSymbol.Locations.First(), methodInfo.Name));
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

    /// <summary>
    /// Validates route parameters in CustomRoute against method parameters.
    /// Reports BSF017 for unmatched route params and BSF018 for complex-typed route params.
    /// Marks matched parameters with IsRouteParameter = true.
    /// </summary>
    private static void ValidateAndMarkRouteParameters(
        SourceProductionContextWrapper context,
        IMethodSymbol methodSymbol,
        MethodInfo methodInfo)
    {
        if (methodInfo.CustomRoute is null)
            return;

        var matches = RegexExpressions.RouteParameterRegex.Matches(methodInfo.CustomRoute);
        foreach (Match match in matches)
        {
            var paramName = match.Groups["name"].Value;
            var idx = methodInfo.Parameters.FindIndex(
                p => string.Equals(p.Name, paramName, StringComparison.OrdinalIgnoreCase));

            if (idx < 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.RouteParameterNotFound,
                    methodSymbol.Locations.First(),
                    methodInfo.Name,
                    paramName));
                continue;
            }

            var paramInfo = methodInfo.Parameters[idx];

            if (IsComplexRouteType(paramInfo.Type))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.RouteParameterComplexType,
                    methodSymbol.Locations.First(),
                    methodInfo.Name,
                    paramName,
                    paramInfo.Type));
            }

            methodInfo.Parameters[idx] = paramInfo with { IsRouteParameter = true };
        }
    }

    private static bool IsComplexRouteType(string typeName)
    {
        var bare = typeName.EndsWith("?", StringComparison.Ordinal)
            ? typeName.AsSpan(0, typeName.Length - 1).ToString()
            : typeName;

        // Fully qualified names (contain '.') are assumed bindable
        if (bare.IndexOf('.') >= 0)
            return false;

        // Generic or array types are complex
        if (bare.IndexOf('<') >= 0 || bare.IndexOf('[') >= 0)
            return true;

        return !s_primitiveRouteTypes.Contains(bare);
    }

    private static readonly HashSet<string> s_primitiveRouteTypes = new(StringComparer.Ordinal)
    {
        "int", "long", "short", "byte", "uint", "ulong", "ushort", "sbyte",
        "float", "double", "decimal", "bool", "char", "string",
        "Guid", "DateTime", "DateTimeOffset", "DateOnly", "TimeOnly", "TimeSpan",
    };

    private static (AsyncType AsyncType, string ReturnType) ParseReturnType(
        SourceProductionContextWrapper context,
        IMethodSymbol methodSymbol)
    {
        var returnType = methodSymbol.ReturnType.ToDisplayString();

        // IAsyncEnumerable<T> — checked before the Task/ValueTask guard so it isn't rejected by BSF007
        if (RegexExpressions.IsAsyncEnumerableRegex.IsMatch(returnType))
        {
            var innerType = methodSymbol.ReturnType is INamedTypeSymbol { TypeArguments.Length: > 0 } iae
                ? iae.TypeArguments[0].ToDisplayString()
                : "object";
            return (AsyncType.AsyncEnumerable, innerType);
        }

        bool isAsync = RegexExpressions.IsAsyncTypeRegex.IsMatch(returnType);

        // BSF007: Method must return Task, ValueTask, or IAsyncEnumerable<T>
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
