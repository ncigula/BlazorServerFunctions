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
        // Step 1: Find all interfaces with [ServerFunctionCollection] attribute in current project
        var interfaceDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsCandidateInterface(node),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);

        // Step 1b: Find all interfaces with [ServerFunctionCollection] attribute in referenced assemblies
        var referencedInterfaces = context.CompilationProvider
            .Select(static (compilation, cancellationToken) => GetReferencedInterfaces(compilation, cancellationToken));

        // Step 2: Detect project type (Server vs Client)
        var projectInfo = context.CompilationProvider
            .Select(static (compilation, _) => GetProjectInfo(compilation));

        // Step 3: Combine everything
        var compilationAndInterfaces = context.CompilationProvider.Combine(interfaceDeclarations.Collect());
        var withReferenced = compilationAndInterfaces.Combine(referencedInterfaces);
        var compilationAndProject = withReferenced.Combine(projectInfo);

        // Step 4: Generate code
        context.RegisterSourceOutput(compilationAndProject,
            static (spc, source) => Execute(spc, source.Left.Left.Left, source.Left.Left.Right, source.Left.Right, source.Right));
    }

    private static ImmutableArray<InterfaceInfo> GetReferencedInterfaces(Compilation compilation, CancellationToken cancellationToken)
    {
        var result = new List<InterfaceInfo>();

        bool isServer = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Routing.IEndpointRouteBuilder") != null;
        bool isClient = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.WebAssembly.Hosting.WebAssemblyHostBuilder") != null;

        // We only search referenced assemblies if this is a top-level project (Server or Client)
        if (!isServer && !isClient)
            return ImmutableArray<InterfaceInfo>.Empty;

        foreach (var reference in compilation.SourceModule.ReferencedAssemblySymbols)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Skip system assemblies
            if (reference.Name.StartsWith("System.", StringComparison.InvariantCulture)
                || reference.Name.StartsWith("Microsoft.", StringComparison.InvariantCulture))
                continue;

            // Search for interfaces with the attribute
            var visitor = new InterfaceVisitor(result, cancellationToken);
            visitor.Visit(reference.GlobalNamespace);
        }

        return result.ToImmutableArray();
    }
    
    private static bool IsCandidateInterface(SyntaxNode node) =>
        node is InterfaceDeclarationSyntax { AttributeLists.Count: > 0 };

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

    // Main execution method
    private static void Execute(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<InterfaceDeclarationSyntax?> interfaces,
        ImmutableArray<InterfaceInfo> referencedInterfaces,
        ProjectInfo projectInfo)
    {
        if (interfaces.IsDefaultOrEmpty && referencedInterfaces.IsDefaultOrEmpty)
            return;

        var localInterfaceInfos = new List<InterfaceInfo>();

        foreach (var interfaceDecl in interfaces)
        {
            if (interfaceDecl is null)
                continue;

            context.CancellationToken.ThrowIfCancellationRequested();

            var semanticModel = compilation.GetSemanticModel(interfaceDecl.SyntaxTree);
            var interfaceSymbol = semanticModel.GetDeclaredSymbol(interfaceDecl);

            if (interfaceSymbol is null)
                continue;

            var interfaceInfo = InterfaceParser.ParseInterface(interfaceSymbol, context.CancellationToken);
            if (interfaceInfo is not null)
            {
                localInterfaceInfos.Add(interfaceInfo);
            }
        }

        if (localInterfaceInfos.Count == 0 && referencedInterfaces.Length == 0)
            return;

        // Combine for endpoint generation (Server needs both local and referenced)
        var allInterfaceInfos = new List<InterfaceInfo>(localInterfaceInfos);
        allInterfaceInfos.AddRange(referencedInterfaces);

        // Generate server endpoints
        if (projectInfo.GenerateEndpoints && allInterfaceInfos.Count > 0)
        {
            foreach (var interfaceInfo in allInterfaceInfos)
            {
                var serverCode = ServerEndpointGenerator.Generate(interfaceInfo);
                context.AddSource($"{interfaceInfo.Name}ServerExtensions.g.cs", serverCode);
            }

            // Generate master registration
            var registrationCode = ServerRegistrationGenerator.Generate(allInterfaceInfos);
            context.AddSource("ServerFunctionEndpointsRegistration.g.cs", registrationCode);
        }

        // Generate client proxies (ONLY for local interfaces)
        if (projectInfo.GenerateClients && localInterfaceInfos.Count > 0)
        {
            foreach (var interfaceInfo in localInterfaceInfos)
            {
                var clientCode = ClientProxyGenerator.Generate(interfaceInfo);
                context.AddSource($"{interfaceInfo.Name}Client.g.cs", clientCode);
            }

            // Generate client registration
            var clientRegistrationCode = ClientRegistrationGenerator.Generate(localInterfaceInfos);
            context.AddSource("ServerFunctionClientsRegistration.g.cs", clientRegistrationCode);
        }
    }
}