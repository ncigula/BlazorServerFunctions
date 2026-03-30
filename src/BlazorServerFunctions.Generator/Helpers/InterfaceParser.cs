using BlazorServerFunctions.Generator.Models;
using Microsoft.CodeAnalysis;

namespace BlazorServerFunctions.Generator.Helpers;

internal sealed partial class InterfaceParser
{
    private readonly SourceProductionContextWrapper _context;
    private readonly Compilation? _compilation;

    private InterfaceParser(SourceProductionContextWrapper context, Compilation? compilation)
    {
        _context = context;
        _compilation = compilation;
    }

    /// <summary>
    /// BSF030: warn when a non-void, non-generic return type is found in a ResultMapper collection.
    /// The mapper cannot be applied to plain types like <c>string</c> or <c>int</c> because there
    /// is no inner type argument to extract for deserialisation — those methods fall back to
    /// <c>Results.Ok(result)</c>.
    /// </summary>
    private void EmitBsf030Warnings(
        IReadOnlyList<MethodInfo> methods,
        INamedTypeSymbol interfaceSymbol)
    {
        foreach (var method in methods)
        {
            if (string.Equals(method.ReturnType, "void", StringComparison.Ordinal))
                continue; // void methods never use the mapper — no warning needed

            if (method.AsyncType is AsyncType.AsyncEnumerable)
                continue; // streaming methods are excluded — documented behaviour

            // ReturnType is already the unwrapped Task<> inner type.
            // If it has no '<' it is non-generic and the mapper cannot be applied.
            if (!method.ReturnType.Contains('<'))
            {
                _context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.ResultMapperReturnTypeNotGeneric,
                    interfaceSymbol.Locations.FirstOrDefault(),
                    method.Name,
                    method.ReturnType));
            }
        }
    }

    internal static InterfaceInfo ParseInterface(
        SourceProductionContextWrapper context,
        INamedTypeSymbol interfaceSymbol,
        Compilation? compilation = null)
        => new InterfaceParser(context, compilation).ParseCore(interfaceSymbol);

    private InterfaceInfo ParseCore(INamedTypeSymbol interfaceSymbol)
    {
        _context.CancellationToken.ThrowIfCancellationRequested();

        var serverFunctionCollectionAttribute = interfaceSymbol.GetAttributes()
            .FirstOrDefault(a => string.Equals(a.AttributeClass?.Name, "ServerFunctionCollectionAttribute", StringComparison.OrdinalIgnoreCase));

        if (serverFunctionCollectionAttribute is null)
            _context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MissingServerFunctionCollectionAttribute,
                interfaceSymbol.Locations.FirstOrDefault(), interfaceSymbol.Name));

        if (!ValidateInterfaceDeclaration(interfaceSymbol))
            return new InterfaceInfo { Name = interfaceSymbol.Name };

        var (routePrefix, requireAuth, configuration, corsPolicy, resultMapperTypeName) = ParseCollectionAttributeArgs(serverFunctionCollectionAttribute, interfaceSymbol);

        var namespaceName = interfaceSymbol.ContainingNamespace.IsGlobalNamespace
            ? "Generated"
            : interfaceSymbol.ContainingNamespace.ToDisplayString();

        var interfaceInfo = new InterfaceInfo
        {
            Name = interfaceSymbol.Name,
            Namespace = namespaceName,
            RoutePrefix = routePrefix,
            RequireAuthorization = requireAuth,
            CorsPolicy = corsPolicy,
            Configuration = configuration,
            ResultMapperTypeName = resultMapperTypeName,
        };

        interfaceInfo.Methods.AddRange(ParseMethods(interfaceInfo, interfaceSymbol));

        if (resultMapperTypeName is not null)
            EmitBsf030Warnings(interfaceInfo.Methods, interfaceSymbol);

        // BSF101: Empty interface warning (doesn't block generation)
        if (interfaceInfo.Methods.Count == 0)
            _context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.EmptyInterface,
                interfaceSymbol.Locations.FirstOrDefault(), interfaceSymbol.Name));

        return interfaceInfo;
    }
}
