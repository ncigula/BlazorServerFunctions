using System.Collections.Immutable;
using BlazorServerFunctions.Abstractions;
using BlazorServerFunctions.Generator.Helpers;
using BlazorServerFunctions.Generator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BlazorServerFunctions.Generator.Generators;

[Generator]
public sealed class ServerFunctionCollectionGenerator : IIncrementalGenerator
{
    private const string ServerTypeMarker = "Microsoft.AspNetCore.Routing.IEndpointRouteBuilder";
    private const string ClientTypeMarker = "Microsoft.AspNetCore.Components.WebAssembly.Hosting.WebAssemblyHostBuilder";
    private const string OpenApiMarker = "Microsoft.AspNetCore.Builder.OpenApiEndpointConventionBuilderExtensions";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var projectInfo = context.CompilationProvider
            .Select(static (compilation, _) => GetProjectInfo(compilation));

        var interfaceDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsCandidateInterface(node),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);

        // Just collect SYMBOLS, don't parse yet
        var referencedInterfaceSymbols = context.CompilationProvider
            .Combine(projectInfo)
            .Select(static (source, cancellationToken) =>
                GetReferencedInterfaceSymbols(source.Left, source.Right, cancellationToken));

        var compilationAndInterfaces = context.CompilationProvider.Combine(interfaceDeclarations.Collect());
        var withReferenced = compilationAndInterfaces.Combine(referencedInterfaceSymbols);
        var compilationAndProject = withReferenced.Combine(projectInfo);

        context.RegisterSourceOutput(compilationAndProject,
            static (spc, data) =>
            {
                var compilation = data.Left.Left.Left;
                var localInterfaces = data.Left.Left.Right;
                var referencedSymbols = data.Left.Right;
                var projectInfo = data.Right;

                Execute(spc, compilation, localInterfaces, referencedSymbols, projectInfo);
            });

        // BSF001: detect [ServerFunction] methods on interfaces missing [ServerFunctionCollection]
        RegisterBsf001Pipeline(context);
    }

    private static void RegisterBsf001Pipeline(IncrementalGeneratorInitializationContext context)
    {
        var orphaned = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsInterfaceWithServerFunctionMethods(node),
                transform: static (ctx, _) => GetInterfaceSymbolIfMissingCollectionAttr(ctx))
            .Where(static m => m is not null)
            .Collect();

        context.RegisterSourceOutput(orphaned, static (spc, symbols) =>
        {
            var reported = new HashSet<string>(StringComparer.Ordinal);
            foreach (var symbol in symbols)
            {
                if (symbol is null) continue;
                var key = symbol.ToDisplayString();
                if (!reported.Add(key)) continue;
                spc.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.MissingServerFunctionCollectionAttribute,
                    symbol.Locations.FirstOrDefault(),
                    symbol.Name));
            }
        });
    }

    private static bool IsInterfaceWithServerFunctionMethods(SyntaxNode node)
    {
        if (node is not InterfaceDeclarationSyntax interfaceDecl)
            return false;

        return interfaceDecl.Members.OfType<MethodDeclarationSyntax>().Any(
            m => m.AttributeLists.Any(al => al.Attributes.Any(
                a => a.Name.ToString().Contains("ServerFunction", StringComparison.Ordinal)
                     && !a.Name.ToString().Contains("ServerFunctionCollection", StringComparison.Ordinal))));
    }

    private static INamedTypeSymbol? GetInterfaceSymbolIfMissingCollectionAttr(GeneratorSyntaxContext ctx)
    {
        var interfaceDecl = (InterfaceDeclarationSyntax)ctx.Node;
        if (ctx.SemanticModel.GetDeclaredSymbol(interfaceDecl) is not INamedTypeSymbol symbol)
            return null;

        var hasCollectionAttr = symbol.GetAttributes().Any(a =>
            string.Equals(a.AttributeClass?.Name, "ServerFunctionCollectionAttribute",
                StringComparison.OrdinalIgnoreCase));

        return hasCollectionAttr ? null : symbol;
    }

    /// <summary>
    /// Collects interface SYMBOLS from referenced assemblies (doesn't parse yet).
    /// Parsing happens later in Execute() where we have SourceProductionContext.
    /// </summary>
    private static ImmutableArray<INamedTypeSymbol> GetReferencedInterfaceSymbols(
        Compilation compilation,
        ProjectInfo projectInfo,
        CancellationToken cancellationToken)
    {
        // Only search referenced assemblies for top-level projects
        if (!projectInfo.GenerateEndpoints && !projectInfo.GenerateClients)
            return ImmutableArray<INamedTypeSymbol>.Empty;

        var result = new List<INamedTypeSymbol>();

        foreach (var reference in compilation.SourceModule.ReferencedAssemblySymbols)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (ShouldSkipAssembly(reference))
                continue;

            // Collect symbols only - don't parse
            var visitor = new InterfaceSymbolCollector(result, cancellationToken);
            visitor.Visit(reference.GlobalNamespace);
        }

        return result.ToImmutableArray();
    }

    private static bool IsCandidateInterface(SyntaxNode node) =>
        node is InterfaceDeclarationSyntax { AttributeLists.Count: > 0 };

    private static bool ShouldSkipAssembly(IAssemblySymbol assembly)
    {
        var name = assembly.Name;
        return name.StartsWith("System.", StringComparison.Ordinal)
               || name.StartsWith("Microsoft.", StringComparison.Ordinal)
               || name.Equals("mscorlib", StringComparison.Ordinal)
               || name.Equals("netstandard", StringComparison.Ordinal);
    }

    private static InterfaceDeclarationSyntax? GetSemanticTargetForGeneration(
        GeneratorSyntaxContext context)
    {
        var interfaceDecl = (InterfaceDeclarationSyntax)context.Node;

        foreach (var attributeList in interfaceDecl.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var attributeName = attribute.Name.ToString();
                var requiredAttributeName = nameof(ServerFunctionCollectionAttribute).Replace("Attribute", "");
                if (string.Equals(attributeName, requiredAttributeName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return interfaceDecl;
                }
            }
        }

        return null;
    }

    private static ProjectInfo GetProjectInfo(Compilation compilation)
    {
        bool isServer = compilation.GetTypeByMetadataName(ServerTypeMarker) != null;
        bool isClient = compilation.GetTypeByMetadataName(ClientTypeMarker) != null;
        bool isLibrary = !isServer && !isClient;
        bool hasOpenApi = compilation.GetTypeByMetadataName(OpenApiMarker) is not null;

        return new ProjectInfo
        {
            GenerateEndpoints = isServer,
            GenerateClients = isClient || isLibrary,
            IsLibrary = isLibrary,
            HasOpenApiPackage = hasOpenApi
        };
    }

    private static void Execute(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<InterfaceDeclarationSyntax?> interfaces,
        ImmutableArray<INamedTypeSymbol> referencedSymbols,
        ProjectInfo projectInfo)
    {
        if (interfaces.IsDefaultOrEmpty && referencedSymbols.IsDefaultOrEmpty)
            return;

        // Parse local interfaces (from this project's source code)
        var localInterfaceInfos = ParseLocalInterfaces(context, compilation, interfaces);

        // Parse referenced interfaces (from referenced assemblies)
        // Pass compilation so referenced interfaces can use the cross-compilation config manifest fallback.
        var referencedInterfaceInfos = ParseReferencedInterfaces(context, compilation, referencedSymbols);

        if (localInterfaceInfos.Count == 0 && referencedInterfaceInfos.Count == 0)
            return;

        // ── Emit config manifests (library projects only) ────────────────────
        if (projectInfo.IsLibrary)
            EmitConfigManifests(context, localInterfaceInfos);

        // ── Generate Server Endpoints ────────────────────────────────────────
        if (projectInfo.GenerateEndpoints)
        {
            var allInterfaces = new List<InterfaceInfo>(
                localInterfaceInfos.Count + referencedInterfaceInfos.Count);

            allInterfaces.AddRange(localInterfaceInfos);
            allInterfaces.AddRange(referencedInterfaceInfos);

            if (allInterfaces.Count > 0)
                GenerateEndpoints(context, allInterfaces, compilation.AssemblyName, projectInfo.HasOpenApiPackage);
        }

        // ── Generate Client Proxies ───────────────────────────────────────────
        if (projectInfo.GenerateClients)
            GenerateClients(context, compilation, projectInfo, localInterfaceInfos, referencedInterfaceInfos);
    }

    private static void GenerateClients(
        SourceProductionContext context,
        Compilation compilation,
        ProjectInfo projectInfo,
        List<InterfaceInfo> localInterfaceInfos,
        List<InterfaceInfo> referencedInterfaceInfos)
    {
        // Proxy files only for LOCAL interfaces.
        // Referenced interfaces already have proxies generated in their source project.
        // Regenerating them here causes CS0436 conflicts when the reference assembly is added.
        foreach (var interfaceInfo in localInterfaceInfos)
        {
            var clientCode = ClientProxyGenerator.Generate(interfaceInfo);
            context.AddSource($"{interfaceInfo.Name.TrimStart('I')}Client.g.cs", clientCode);
        }

        // Registration is generated for Client/Server projects and for Library projects
        // that consume interfaces from referenced assemblies.
        // Source libraries (Library mode with only local interfaces) skip registration —
        // the consuming Client/Server project generates it when referencing this library.
        bool isSourceLibrary = projectInfo.IsLibrary && referencedInterfaceInfos.Count == 0;

        if (!isSourceLibrary)
        {
            var allForRegistration = new List<InterfaceInfo>(
                localInterfaceInfos.Count + referencedInterfaceInfos.Count);
            allForRegistration.AddRange(localInterfaceInfos);
            allForRegistration.AddRange(referencedInterfaceInfos);

            if (allForRegistration.Count > 0)
            {
                var registrationCode = ClientRegistrationGenerator.Generate(allForRegistration, compilation.AssemblyName);
                context.AddSource("ServerFunctionClientsRegistration.g.cs", registrationCode);
            }
        }
    }

    private static List<InterfaceInfo> ParseLocalInterfaces(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<InterfaceDeclarationSyntax?> interfaces)
    {
        var result = new List<InterfaceInfo>(interfaces.Length);
        var semanticModelCache = new Dictionary<SyntaxTree, SemanticModel>();

        foreach (var interfaceDecl in interfaces)
        {
            if (interfaceDecl is null)
                continue;

            context.CancellationToken.ThrowIfCancellationRequested();

            if (!semanticModelCache.TryGetValue(interfaceDecl.SyntaxTree, out var semanticModel))
            {
                semanticModel = compilation.GetSemanticModel(interfaceDecl.SyntaxTree);
                semanticModelCache[interfaceDecl.SyntaxTree] = semanticModel;
            }

            var interfaceSymbol = semanticModel.GetDeclaredSymbol(interfaceDecl);
            if (interfaceSymbol is null)
                continue;

            var sourceProductionContextWrapper = new SourceProductionContextWrapper(context);
            
            // Parse with context - emits diagnostics
            var interfaceInfo = InterfaceParser.ParseInterface(
                sourceProductionContextWrapper,
                interfaceSymbol,
                compilation);

            if (!sourceProductionContextWrapper.HasErrors && interfaceInfo.Methods.Count > 0)
                result.Add(interfaceInfo);
        }

        return result;
    }

    /// <summary>
    /// Parse referenced interface symbols.
    /// We have context here so we CAN emit diagnostics if needed.
    /// But typically we silently skip invalid ones since they're not our code.
    /// </summary>
    private static List<InterfaceInfo> ParseReferencedInterfaces(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<INamedTypeSymbol> symbols)
    {
        var result = new List<InterfaceInfo>(symbols.Length);

        foreach (var symbol in symbols)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                var sourceProductionContextWrapper = new SourceProductionContextWrapper(context);

                var interfaceInfo = InterfaceParser.ParseInterface(
                    sourceProductionContextWrapper,
                    symbol,
                    compilation);

                if (!sourceProductionContextWrapper.HasErrors && interfaceInfo.Methods.Count > 0)
                    result.Add(interfaceInfo);
            }
            catch (Exception)
            {
                // BSF016: Failed to parse a referenced interface — report and skip
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.ReferencedInterfaceParseFailure,
                        Location.None,
                        symbol.Name,
                        symbol.ContainingAssembly.Name));
            }
        }

        return result;
    }

    /// <summary>
    /// Emits <c>__BsfConfig_{InterfaceName}.g.cs</c> manifest files for library projects.
    /// These encode resolved configuration as const fields so server/client generators can
    /// recover the values from compiled metadata (cross-compilation fallback).
    /// </summary>
    private static void EmitConfigManifests(
        SourceProductionContext context,
        List<InterfaceInfo> localInterfaceInfos)
    {
        foreach (var interfaceInfo in localInterfaceInfos)
        {
            var manifestCode = ConfigManifestGenerator.Generate(interfaceInfo);
            if (manifestCode is not null)
                context.AddSource($"__BsfConfig_{interfaceInfo.Name}.g.cs", manifestCode);
        }
    }

    private static void GenerateEndpoints(
        SourceProductionContext context,
        List<InterfaceInfo> allInterfaces,
        string? targetNamespace,
        bool hasOpenApiPackage)
    {
        foreach (var interfaceInfo in allInterfaces)
        {
            var serverCode = ServerEndpointGenerator.Generate(interfaceInfo, targetNamespace, hasOpenApiPackage);
            context.AddSource($"{interfaceInfo.Name}ServerExtensions.g.cs", serverCode);
        }

        var registrationCode = ServerRegistrationGenerator.Generate(allInterfaces, targetNamespace);
        context.AddSource("ServerFunctionEndpointsRegistration.g.cs", registrationCode);
    }

}