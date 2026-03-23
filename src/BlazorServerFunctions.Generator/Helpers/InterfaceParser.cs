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

        var (routePrefix, requireAuth, configuration, corsPolicy) = ParseCollectionAttributeArgs(serverFunctionCollectionAttribute, interfaceSymbol);

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
        };

        interfaceInfo.Methods.AddRange(ParseMethods(interfaceInfo, interfaceSymbol));

        // BSF101: Empty interface warning (doesn't block generation)
        if (interfaceInfo.Methods.Count == 0)
            _context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.EmptyInterface,
                interfaceSymbol.Locations.FirstOrDefault(), interfaceSymbol.Name));

        return interfaceInfo;
    }
}
