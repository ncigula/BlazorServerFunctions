using System.Collections.Immutable;
using BlazorServerFunctions.Generator.Helpers;
using BlazorServerFunctions.Generator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BlazorServerFunctions.Generator.Generators;

[Generator]
public sealed class ServerFunctionCollectionGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Step 1: Detect project type FIRST (only once)
        var projectInfo = context.CompilationProvider
            .Select(static (compilation, _) => GetProjectInfo(compilation));

        // Step 1a: Find local interfaces
        var interfaceDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsCandidateInterface(node),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);

        // Step 1b: Find referenced interfaces (only if needed)
        var referencedInterfaces = context.CompilationProvider
            .Combine(projectInfo)
            .Select(static (source, cancellationToken) =>
                GetReferencedInterfaces(source.Left, source.Right, cancellationToken));

        // Step 2: Combine everything
        var compilationAndInterfaces = context.CompilationProvider.Combine(interfaceDeclarations.Collect());
        var withReferenced = compilationAndInterfaces.Combine(referencedInterfaces);
        var compilationAndProject = withReferenced.Combine(projectInfo);

        // Step 3: Generate code
        context.RegisterSourceOutput(compilationAndProject,
            static (spc, data) =>
            {
                var compilation = data.Left.Left.Left;
                var localInterfaces = data.Left.Left.Right;
                var referencedInterfaces = data.Left.Right;
                var projectInfo = data.Right;

                Execute(spc, compilation, localInterfaces, referencedInterfaces, projectInfo);
            });
    }

    private static ImmutableArray<InterfaceInfo> GetReferencedInterfaces(
        Compilation compilation,
        ProjectInfo projectInfo,
        CancellationToken cancellationToken)
    {
        if (projectInfo is { GenerateEndpoints: false, GenerateClients: false })
            return ImmutableArray<InterfaceInfo>.Empty;

        var result = new List<InterfaceInfo>();

        foreach (var reference in compilation.SourceModule.ReferencedAssemblySymbols)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (ShouldSkipAssembly(reference))
                continue;

            var visitor = new InterfaceVisitor(result, cancellationToken);
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

        // Check if it has [ServerFunctionCollection] attribute
        foreach (var attributeList in interfaceDecl.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var name = attribute.Name.ToString();
                if (string.Equals(name, "ServerFunctionCollection", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(name, "ServerFunctionCollectionAttribute", StringComparison.OrdinalIgnoreCase))
                {
                    return interfaceDecl;
                }
            }
        }

        return null;
    }

    private static ProjectInfo GetProjectInfo(Compilation compilation)
    {
        // Detect Server by checking for IEndpointRouteBuilder (ASP.NET Core)
        bool isServer = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Routing.IEndpointRouteBuilder") != null;

        // Detect Client by checking for WebAssemblyHostBuilder (Blazor WASM)
        bool isClient = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.WebAssembly.Hosting.WebAssemblyHostBuilder") != null;

        // If it's a Server project, we DON'T want to generate clients for referenced interfaces
        // because those clients are already generated in the referenced libraries.
        // We only generate server endpoints for everything.

        // The user specifically wants Shared projects to NOT contain server code.
        bool isLibrary = !isServer && !isClient;

        return new ProjectInfo
        {
            GenerateEndpoints = isServer, // Only generate endpoints if it IS a server project
            GenerateClients = isClient || isLibrary // Generate clients for client projects OR libraries
        };
    }

    private static void Execute(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<InterfaceDeclarationSyntax?> interfaces,
        ImmutableArray<InterfaceInfo> referencedInterfaces,
        ProjectInfo projectInfo)
    {
        // Early exit if nothing to do
        if (interfaces.IsDefaultOrEmpty && referencedInterfaces.IsDefaultOrEmpty)
            return;

        var localInterfaceInfos = ParseLocalInterfaces(context, compilation, interfaces);

        // Another early exit
        if (localInterfaceInfos.Count == 0 && referencedInterfaces.Length == 0)
            return;

        // Generate endpoints if needed
        if (projectInfo.GenerateEndpoints)
        {
            GenerateEndpoints(context, localInterfaceInfos, referencedInterfaces);
        }

        // Generate clients if needed
        if (projectInfo.GenerateClients && localInterfaceInfos.Count > 0)
        {
            GenerateClients(context, localInterfaceInfos);
        }
    }

    private static List<InterfaceInfo> ParseLocalInterfaces(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<InterfaceDeclarationSyntax?> interfaces)
    {
        var result = new List<InterfaceInfo>(interfaces.Length);
        var semanticModelCache = new Dictionary<SyntaxTree, SemanticModel>(); // 👈 Cache

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

            var interfaceInfo = InterfaceParser.ParseInterface(interfaceSymbol, context.CancellationToken);
            if (interfaceInfo is not null)
            {
                result.Add(interfaceInfo);
            }
        }

        return result;
    }

    private static void GenerateEndpoints(
        SourceProductionContext context,
        List<InterfaceInfo> localInterfaces,
        ImmutableArray<InterfaceInfo> referencedInterfaces)
    {
        var allInterfaceInfos = new List<InterfaceInfo>(
            localInterfaces.Count + referencedInterfaces.Length); // 👈 Pre-size

        allInterfaceInfos.AddRange(localInterfaces);
        allInterfaceInfos.AddRange(referencedInterfaces);

        if (allInterfaceInfos.Count == 0)
            return;

        foreach (var interfaceInfo in allInterfaceInfos)
        {
            var serverCode = ServerEndpointGenerator.Generate(interfaceInfo);
            context.AddSource($"{interfaceInfo.Name}ServerExtensions.g.cs", serverCode);
        }

        var registrationCode = ServerRegistrationGenerator.Generate(allInterfaceInfos);
        context.AddSource("ServerFunctionEndpointsRegistration.g.cs", registrationCode);
    }

    private static void GenerateClients(
        SourceProductionContext context,
        List<InterfaceInfo> localInterfaces)
    {
        if (localInterfaces.Count == 0)
            return;
        
        foreach (var interfaceInfo in localInterfaces)
        {
            var clientCode = ClientProxyGenerator.Generate(interfaceInfo);
            context.AddSource($"{interfaceInfo.Name}Client.g.cs", clientCode);
        }

        var clientRegistrationCode = ClientRegistrationGenerator.Generate(localInterfaces);
        context.AddSource("ServerFunctionClientsRegistration.g.cs", clientRegistrationCode);
    }
}