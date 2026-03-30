using BlazorServerFunctions.Generator.Models;
using Microsoft.CodeAnalysis;

namespace BlazorServerFunctions.Generator.Helpers;

internal sealed partial class InterfaceParser
{
    /// <summary>
    /// <see cref="SymbolDisplayFormat"/> that produces a fully-qualified type name
    /// with the <c>global::</c> prefix but without any generic type parameters.
    /// Used to store result mapper type names in the equatable model records.
    /// </summary>
    private static readonly SymbolDisplayFormat ResultMapperTypeNameFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.None);

    /// <summary>Validates BSF003-BSF006. Returns false if fatal (should abort parsing).</summary>
    private bool ValidateInterfaceDeclaration(INamedTypeSymbol interfaceSymbol)
    {
        // BSF003: Interface must be public
        if (interfaceSymbol.DeclaredAccessibility != Accessibility.Public)
        {
            _context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InterfaceMustBePublic,
                interfaceSymbol.Locations.FirstOrDefault(), interfaceSymbol.Name));
            return false;
        }

        // BSF004: Interface cannot be generic
        if (interfaceSymbol.TypeParameters.Length > 0)
            _context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InterfaceCannotBeGeneric,
                interfaceSymbol.Locations.FirstOrDefault(), interfaceSymbol.Name));

        // BSF005: Interface cannot have properties
        if (interfaceSymbol.GetMembers().OfType<IPropertySymbol>().Any())
            _context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InterfaceCannotHaveProperties,
                interfaceSymbol.Locations.FirstOrDefault(), interfaceSymbol.Name));

        // BSF006: Interface cannot have events
        if (interfaceSymbol.GetMembers().OfType<IEventSymbol>().Any())
            _context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InterfaceCannotHaveEvents,
                interfaceSymbol.Locations.FirstOrDefault(), interfaceSymbol.Name));

        return true;
    }

    private (string RoutePrefix, bool RequireAuth, ConfigurationInfo Configuration, string? CorsPolicy, string? ResultMapperTypeName) ParseCollectionAttributeArgs(
        AttributeData? attribute,
        INamedTypeSymbol interfaceSymbol)
    {
        var (routePrefix, requireAuth, configType, rawCorsPolicy, configuration, resultMapperTypeName)
            = ReadCollectionNamedArgs(attribute, interfaceSymbol);

        // If no Configuration type was specified, read ApiType directly from the attribute
        if (configType == null)
        {
            var apiTypeArg = attribute?.NamedArguments
                .FirstOrDefault(a => string.Equals(a.Key, "ApiType", StringComparison.Ordinal));
            if (apiTypeArg.HasValue && apiTypeArg.Value.Value.Value is int apiTypeInt)
                configuration = configuration with { ApiType = (ApiType)apiTypeInt };
        }

        // Strip leading slash so route generation doesn't produce double-slashes
        // e.g. [ServerFunctionCollection(RoutePrefix = "/users")] → "users"
        routePrefix = routePrefix?.TrimStart('/');
        routePrefix ??= interfaceSymbol.Name.TrimStart('I').ToLowerInvariant();

        if (rawCorsPolicy is not null && rawCorsPolicy.Length == 0)
        {
            _context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.EmptyCorsPolicy,
                interfaceSymbol.Locations.FirstOrDefault(), interfaceSymbol.Name));
            rawCorsPolicy = null;
        }

        // BSF029: ResultMapper on a gRPC interface is not supported.
        if (resultMapperTypeName is not null && configuration.ApiType == ApiType.GRPC)
        {
            _context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ResultMapperNotSupportedForGrpc,
                interfaceSymbol.Locations.FirstOrDefault(), interfaceSymbol.Name));
            resultMapperTypeName = null; // suppress, don't attempt to emit mapper code
        }

        var corsPolicy = rawCorsPolicy ?? configuration.CorsPolicy;

        return (routePrefix, requireAuth, configuration, corsPolicy, resultMapperTypeName);
    }

    private (string? RoutePrefix, bool RequireAuth, INamedTypeSymbol? ConfigType, string? RawCorsPolicy,
             ConfigurationInfo Configuration, string? ResultMapperTypeName)
        ReadCollectionNamedArgs(AttributeData? attribute, INamedTypeSymbol interfaceSymbol)
    {
        string? routePrefix = null;
        bool requireAuth = false;
        var configuration = ConfigurationInfo.Default;
        string? rawCorsPolicy = null;
        INamedTypeSymbol? configType = null;
        string? resultMapperTypeName = null;

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
                    {
                        configType = configSymbol;
                        configuration = ConfigurationReader.ReadConfiguration(configSymbol, interfaceSymbol, _compilation);
                    }
                    break;
                case "CorsPolicy":
                    rawCorsPolicy = namedArg.Value.Value?.ToString();
                    break;
                case "ResultMapper":
                    if (namedArg.Value.Value is INamedTypeSymbol mapperSymbol)
                    {
                        // Store as fully-qualified name without type parameters so the
                        // model stays equatable (no ITypeSymbol in records).
                        resultMapperTypeName = mapperSymbol.OriginalDefinition
                            .ToDisplayString(ResultMapperTypeNameFormat);
                    }
                    break;
            }
        }

        return (routePrefix, requireAuth, configType, rawCorsPolicy, configuration, resultMapperTypeName);
    }
}
