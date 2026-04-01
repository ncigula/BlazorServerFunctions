using BlazorServerFunctions.Generator.Models;
using Microsoft.CodeAnalysis;
using HttpMethod = BlazorServerFunctions.Generator.Models.HttpMethod;

namespace BlazorServerFunctions.Generator.Helpers;

internal sealed partial class InterfaceParser
{
    private IEnumerable<MethodInfo> ParseMethods(InterfaceInfo interfaceInfo, INamedTypeSymbol interfaceSymbol)
    {
        var seenRoutes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var member in interfaceSymbol.GetMembers())
        {
            if (member is not IMethodSymbol { MethodKind: MethodKind.Ordinary } methodSymbol)
                continue;

            var methodInfo = ParseMethod(interfaceInfo, methodSymbol);

            // BSF014: Duplicate route check — keyed on (httpMethod, route) so that
            // different verbs on the same path (GET /items/{id} vs DELETE /items/{id}) are valid.
            // gRPC uses method names for dispatch (not HTTP verbs + routes), so this check is REST-only.
            if (interfaceInfo.Configuration.ApiType != ApiType.GRPC)
            {
                var route = methodInfo.CustomRoute ?? methodInfo.Name;
                var routeKey = $"{methodInfo.HttpMethod}:{route}";
                if (seenRoutes.TryGetValue(routeKey, out var existingMethodName))
                    _context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.DuplicateRoute,
                        methodSymbol.Locations.First(), methodInfo.Name, route, existingMethodName));
                else
                    seenRoutes[routeKey] = methodInfo.Name;
            }

            yield return methodInfo;
        }
    }

    private MethodInfo ParseMethod(InterfaceInfo interfaceInfo, IMethodSymbol methodSymbol)
    {
        _context.CancellationToken.ThrowIfCancellationRequested();

        // BSF008: Method cannot be generic
        if (methodSymbol.TypeParameters.Length > 0)
            _context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MethodCannotBeGeneric,
                methodSymbol.Locations.First(), methodSymbol.Name));

        ValidateParameterModifiers(methodSymbol);

        // BSF102: Too many parameters warning (doesn't block generation)
        if (methodSymbol.Parameters.Length > 5)
            _context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.TooManyParameters,
                methodSymbol.Locations.First(), methodSymbol.Name, methodSymbol.Parameters.Length));

        var methodInfo = new MethodInfo
        {
            Name = methodSymbol.Name,
            RequireAuthorization = interfaceInfo.RequireAuthorization
        };

        (methodInfo.AsyncType, methodInfo.ReturnType) = ParseReturnType(methodSymbol);
        methodInfo.HasCancellationToken = methodSymbol.Parameters.Any(p =>
            string.Equals(p.Type.ToDisplayString(), "System.Threading.CancellationToken", StringComparison.Ordinal));
        methodInfo.Parameters = ParseParameters(methodSymbol);

        var serverFunctionAttribute = GetServerFunctionAttribute(methodSymbol, methodInfo, interfaceInfo);
        ParseServerFunctionAttributes(methodSymbol, methodInfo, serverFunctionAttribute, interfaceInfo);
        ValidateCacheSeconds(methodSymbol, methodInfo);

        if (interfaceInfo.Configuration.ApiType == ApiType.GRPC)
            ValidateGrpcConstraints(methodSymbol, methodInfo);

        ValidateFileParameters(methodSymbol, methodInfo, interfaceInfo.Configuration.ApiType);
        ValidateExplicitParameterBindings(methodSymbol, methodInfo);
        ParseOpenApiAttributes(serverFunctionAttribute, methodInfo);

        return methodInfo;
    }

    /// <summary>Validates BSF009/BSF010/BSF011 parameter modifiers.</summary>
    private void ValidateParameterModifiers(IMethodSymbol methodSymbol)
    {
        foreach (var param in methodSymbol.Parameters)
        {
            if (param.RefKind == RefKind.Out)
                _context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.OutParametersNotSupported,
                    param.Locations.FirstOrDefault() ?? methodSymbol.Locations.First(),
                    methodSymbol.Name, param.Name));
            else if (param.RefKind == RefKind.Ref)
                _context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.RefParametersNotSupported,
                    param.Locations.FirstOrDefault() ?? methodSymbol.Locations.First(),
                    methodSymbol.Name, param.Name));

            if (param.IsParams)
                _context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ParamsNotSupported,
                    param.Locations.FirstOrDefault() ?? methodSymbol.Locations.First(),
                    methodSymbol.Name, param.Name));
        }
    }

    private void ParseServerFunctionAttributes(
        IMethodSymbol methodSymbol,
        MethodInfo methodInfo,
        AttributeData? serverFunctionAttribute,
        InterfaceInfo interfaceInfo)
    {
        bool hasExplicitHttpMethod = false;
        int methodCacheSeconds = -1; // -1 = not set on attribute → inherit from config
        string? rawMethodRateLimitPolicy = null; // null = not set on attribute → inherit from config
        string? rawMethodPolicy = null; // null = not set on attribute → inherit from config
        string? rawMethodRoles = null; // null = not set on attribute → no roles requirement
        var filters = new List<string>();

        foreach (var attribute in serverFunctionAttribute?.NamedArguments ?? [])
        {
            switch (attribute.Key)
            {
                case "Route":
                    methodInfo.CustomRoute = attribute.Value.Value?.ToString();
                    ValidateRouteFormat(methodSymbol, methodInfo);
                    ValidateAndMarkRouteParameters(methodSymbol, methodInfo);
                    break;

                case "HttpMethod":
                    hasExplicitHttpMethod = ParseHttpMethodAttribute(methodSymbol, methodInfo, attribute.Value);
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

                case "Policy":
                    rawMethodPolicy = attribute.Value.Value?.ToString();
                    break;

                case "Roles":
                    rawMethodRoles = attribute.Value.Value?.ToString();
                    break;

                case "RequireAntiForgery":
                    ParseRequireAntiForgery(methodSymbol, methodInfo, attribute.Value, interfaceInfo.Configuration.ApiType);
                    break;

                case "Filters":
                    filters.AddRange(ParseFilterTypes(attribute.Value));
                    break;
            }
        }

        methodInfo.Filters = filters;
        rawMethodRoles = ValidateRoles(methodSymbol, methodInfo, rawMethodRoles);
        ResolveFromConfig(methodInfo, interfaceInfo, hasExplicitHttpMethod, methodCacheSeconds, rawMethodRateLimitPolicy, rawMethodPolicy, rawMethodRoles);
    }

    private static List<string> ParseFilterTypes(TypedConstant value)
    {
        var filters = new List<string>();
        foreach (var item in value.Values)
        {
            if (item.Value is INamedTypeSymbol filterSymbol)
                filters.Add(filterSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        }
        return filters;
    }

    private string? ValidateRoles(
        IMethodSymbol methodSymbol,
        MethodInfo methodInfo,
        string? rawMethodRoles)
    {
        if (rawMethodRoles is not null && rawMethodRoles.Length == 0)
        {
            _context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.EmptyRoles,
                methodSymbol.Locations.First(), methodInfo.Name));
            return null;
        }
        return rawMethodRoles;
    }

    private bool ParseHttpMethodAttribute(
        IMethodSymbol methodSymbol,
        MethodInfo methodInfo,
        TypedConstant value)
    {
        if (value.Kind is not TypedConstantKind.Error && value.Value != null)
        {
            var httpMethodStr = value.Value.ToString()!;
            if (Enum.TryParse<HttpMethod>(httpMethodStr, ignoreCase: true, out var parsedMethod))
            {
                methodInfo.HttpMethod = parsedMethod;
                return true;
            }
        }

        _context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InvalidHttpMethod,
            methodSymbol.Locations.First(), methodInfo.Name,
            value.Value?.ToString() ?? "null"));
        return true; // Still "explicit" even if invalid — prevents DefaultHttpMethod override
    }

    /// <summary>
    /// Handles the <c>RequireAntiForgery</c> attribute argument.
    /// BSF025: emits a warning and leaves the flag <c>false</c> when the interface uses gRPC.
    /// </summary>
    private void ParseRequireAntiForgery(
        IMethodSymbol methodSymbol,
        MethodInfo methodInfo,
        TypedConstant value,
        ApiType apiType)
    {
        if (apiType == ApiType.GRPC && value.Value is true)
        {
            // BSF025: RequireAntiForgery not supported for gRPC — leave methodInfo.RequireAntiForgery = false
            _context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.AntiForgeryIgnoredForGrpc,
                methodSymbol.Locations.First(), methodInfo.Name));
        }
        else
        {
            methodInfo.RequireAntiForgery = value.Value is true;
        }
    }

    private static void ResolveFromConfig(
        MethodInfo methodInfo,
        InterfaceInfo interfaceInfo,
        bool hasExplicitHttpMethod,
        int methodCacheSeconds,
        string? rawMethodRateLimitPolicy,
        string? rawMethodPolicy,
        string? rawMethodRoles)
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

        // Resolve Policy: null = not set → inherit config; "" = explicit disable → null; non-empty = use it
        methodInfo.Policy = rawMethodPolicy switch
        {
            null => interfaceInfo.Configuration.Policy,
            "" => null,
            _ => rawMethodPolicy
        };

        // Resolve Roles: no config default — null = not set → no roles requirement; non-null = use it
        // (empty string case already validated and nulled out before this call)
        methodInfo.Roles = rawMethodRoles;
    }

    /// <summary>
    /// Validates the resolved CacheSeconds value.
    /// BSF019: streaming methods cannot be cached (warning, clamp to 0).
    /// BSF020: non-GET methods cannot be cached (error, clamp to 0).
    /// Must be called after both HttpMethod and AsyncType are resolved.
    /// </summary>
    private void ValidateCacheSeconds(IMethodSymbol methodSymbol, MethodInfo methodInfo)
    {
        if (methodInfo.CacheSeconds <= 0)
            return;

        if (methodInfo.AsyncType is AsyncType.AsyncEnumerable)
        {
            _context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.CacheOnStreamingMethod,
                methodSymbol.Locations.First(), methodInfo.Name));
            methodInfo.CacheSeconds = 0;
            return;
        }

        if (methodInfo.HttpMethod is not HttpMethod.Get)
        {
            _context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.CacheOnNonGetMethod,
                methodSymbol.Locations.First(), methodInfo.Name,
                methodInfo.HttpMethod.ToString().ToUpperInvariant()));
            methodInfo.CacheSeconds = 0;
        }
    }

    /// <summary>
    /// BSF024: Output caching not supported for gRPC — clamps <see cref="MethodInfo.CacheSeconds"/> to 0.
    /// Called only when <see cref="ApiType.GRPC"/> is active, after <see cref="ValidateCacheSeconds"/>.
    /// </summary>
    private void ValidateGrpcConstraints(IMethodSymbol methodSymbol, MethodInfo methodInfo)
    {
        // BSF024: CacheSeconds not supported for gRPC (may come from attribute or config default)
        if (methodInfo.CacheSeconds > 0)
        {
            _context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.CacheSecondsIgnoredForGrpc,
                methodSymbol.Locations.First(), methodInfo.Name));
            methodInfo.CacheSeconds = 0;
        }
    }

    /// <summary>
    /// BSF026: File upload on GET/DELETE.
    /// BSF027: File upload with IAsyncEnumerable return.
    /// BSF028: File upload on gRPC interface.
    /// Must be called after both HttpMethod, AsyncType, and ApiType are resolved.
    /// </summary>
    private void ValidateFileParameters(IMethodSymbol methodSymbol, MethodInfo methodInfo, ApiType apiType)
    {
        var fileParams = methodInfo.Parameters
            .Where(static p => p.FileKind != FileKind.None)
            .ToList();

        if (fileParams.Count == 0)
            return;

        var httpMethodStr = methodInfo.HttpMethod.ToString().ToUpperInvariant();

        foreach (var fileParamName in fileParams.Select(static p => p.Name))
        {
            if (apiType == ApiType.GRPC)
            {
                _context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.FileUploadNotSupportedForGrpc,
                    methodSymbol.Locations.First(),
                    methodInfo.Name,
                    fileParamName));
            }
            else if (methodInfo.HttpMethod is HttpMethod.Get or HttpMethod.Delete)
            {
                _context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.FileUploadOnGetOrDelete,
                    methodSymbol.Locations.First(),
                    methodInfo.Name,
                    fileParamName,
                    httpMethodStr));
            }
        }

        if (apiType != ApiType.GRPC && methodInfo.AsyncType is AsyncType.AsyncEnumerable)
        {
            _context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.FileUploadWithStreamingReturn,
                methodSymbol.Locations.First(),
                methodInfo.Name));
        }
    }

    /// <summary>
    /// Reads the five OpenAPI customization named arguments from <c>[ServerFunction]</c>:
    /// <c>Summary</c>, <c>Description</c>, <c>Tags</c>, <c>ProducesStatusCodes</c>, <c>ExcludeFromOpenApi</c>.
    /// </summary>
    private static void ParseOpenApiAttributes(AttributeData? serverFunctionAttribute, MethodInfo methodInfo)
    {
        foreach (var attribute in serverFunctionAttribute?.NamedArguments ?? [])
        {
            switch (attribute.Key)
            {
                case "Summary":
                    methodInfo.Summary = attribute.Value.Value?.ToString();
                    break;
                case "Description":
                    methodInfo.Description = attribute.Value.Value?.ToString();
                    break;
                case "Tags":
                    methodInfo.Tags = attribute.Value.IsNull ? null
                        : [.. attribute.Value.Values.Select(v => v.Value?.ToString() ?? "")];
                    break;
                case "ProducesStatusCodes":
                    methodInfo.ProducesStatusCodes = attribute.Value.IsNull ? null
                        : [.. attribute.Value.Values.Select(v => v.Value is int i ? i : 0)];
                    break;
                case "ExcludeFromOpenApi":
                    methodInfo.ExcludeFromOpenApi = attribute.Value.Value is true;
                    break;
            }
        }
    }

    /// <summary>BSF015: Validates route format after setting CustomRoute.</summary>
    private void ValidateRouteFormat(IMethodSymbol methodSymbol, MethodInfo methodInfo)
    {
        if (methodInfo.CustomRoute != null &&
            (string.IsNullOrWhiteSpace(methodInfo.CustomRoute) ||
             methodInfo.CustomRoute.Any(char.IsWhiteSpace)))
        {
            _context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InvalidRouteFormat,
                methodSymbol.Locations.First(), methodSymbol.Name, methodInfo.CustomRoute));
        }
    }

    private AttributeData? GetServerFunctionAttribute(
        IMethodSymbol methodSymbol,
        MethodInfo methodInfo,
        InterfaceInfo interfaceInfo)
    {
        var serverFunctionAttribute = methodSymbol.GetAttributes()
            .FirstOrDefault(a => string.Equals(a.AttributeClass?.Name, "ServerFunctionAttribute", StringComparison.OrdinalIgnoreCase));

        if (serverFunctionAttribute is null)
            _context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MissingServerFunctionAttribute,
                methodSymbol.Locations.First(), methodInfo.Name, methodSymbol.Name));

        // BSF012: HttpMethod is required unless a DefaultHttpMethod is set on the config.
        // Not applicable for gRPC — gRPC uses HTTP POST exclusively at the transport layer.
        bool hasExplicitHttpMethod = serverFunctionAttribute is not null
            && serverFunctionAttribute.NamedArguments.Any(kvp => string.Equals(kvp.Key, "HttpMethod", StringComparison.Ordinal));

        if (interfaceInfo.Configuration.ApiType == ApiType.GRPC)
        {
            // BSF023: HttpMethod explicitly set on a gRPC method — must be removed
            if (hasExplicitHttpMethod)
                _context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.HttpMethodIgnoredForGrpc,
                    methodSymbol.Locations.First(), methodInfo.Name));
        }
        else if (serverFunctionAttribute is not null
            && !hasExplicitHttpMethod
            && interfaceInfo.Configuration.DefaultHttpMethod is null)
        {
            _context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.HttpMethodRequired,
                methodSymbol.Locations.First(), methodInfo.Name));
        }

        return serverFunctionAttribute;
    }

    private (AsyncType AsyncType, string ReturnType) ParseReturnType(IMethodSymbol methodSymbol)
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
            _context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InvalidReturnType,
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
