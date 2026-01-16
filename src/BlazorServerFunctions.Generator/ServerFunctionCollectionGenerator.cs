using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace BlazorServerFunctions.Generator;

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
            if (reference.Name.StartsWith("System.") || reference.Name.StartsWith("Microsoft."))
                continue;

            // Search for interfaces with the attribute
            var visitor = new InterfaceVisitor(result, cancellationToken);
            visitor.Visit(reference.GlobalNamespace);
        }

        return result.ToImmutableArray();
    }

    private sealed class InterfaceVisitor : SymbolVisitor
    {
        private readonly List<InterfaceInfo> _result;
        private readonly CancellationToken _cancellationToken;

        public InterfaceVisitor(List<InterfaceInfo> result, CancellationToken cancellationToken)
        {
            _result = result;
            _cancellationToken = cancellationToken;
        }

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            foreach (var member in symbol.GetMembers())
            {
                member.Accept(this);
            }
        }

        public override void VisitNamedType(INamedTypeSymbol symbol)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            if (symbol.TypeKind == TypeKind.Interface)
            {
                var interfaceInfo = ParseInterface(symbol, _cancellationToken);
                if (interfaceInfo != null)
                {
                    _result.Add(interfaceInfo);
                }
            }

            foreach (var member in symbol.GetTypeMembers())
            {
                member.Accept(this);
            }
        }
    }

        // Check if a syntax node could be an interface with our attribute
    private static bool IsCandidateInterface(SyntaxNode node) =>
        node is InterfaceDeclarationSyntax { AttributeLists.Count: > 0 };

    // Get the semantic model for the interface
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
                if (name == "ServerFunctionCollection" || name == "ServerFunctionCollectionAttribute")
                {
                    return interfaceDecl;
                }
            }
        }

        return null;
    }

    // Detect if this is Server or Client project
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

            var interfaceInfo = ParseInterface(interfaceSymbol, context.CancellationToken);
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
                var clientCode = CodeGenerators.ClientProxyGenerator.Generate(interfaceInfo);
                context.AddSource($"{interfaceInfo.Name}Client.g.cs", clientCode);
            }

            // Generate client registration
            var clientRegistrationCode = ClientRegistrationGenerator.Generate(localInterfaceInfos);
            context.AddSource("ServerFunctionClientsRegistration.g.cs", clientRegistrationCode);
        }
    }

    // Parse interface symbol into our data model
    private static InterfaceInfo? ParseInterface(
        INamedTypeSymbol interfaceSymbol,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Get [ServerFunctionCollection] attribute data
        var attribute = interfaceSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name == "ServerFunctionCollectionAttribute");

        if (attribute is null)
            return null;

        // Extract attribute parameters
        string? routePrefix = null;
        bool requireAuth = false;

        foreach (var namedArg in attribute.NamedArguments)
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

        // Parse methods
        var methods = new List<MethodInfo>();
        foreach (var member in interfaceSymbol.GetMembers())
        {
            if (member is not IMethodSymbol methodSymbol)
                continue;

            var methodInfo = ParseMethod(methodSymbol, cancellationToken);
            if (methodInfo is not null)
            {
                methods.Add(methodInfo);
            }
        }

        return new InterfaceInfo
        {
            Name = interfaceSymbol.Name,
            Namespace = namespaceName,
            RoutePrefix = routePrefix,
            RequireAuthorization = requireAuth,
            Methods = methods
        };
    }

    // Parse method symbol into our data model
    private static MethodInfo? ParseMethod(
        IMethodSymbol methodSymbol,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Skip property accessors, etc.
        if (methodSymbol.MethodKind != MethodKind.Ordinary)
            return null;

        // Get return type
        var returnType = methodSymbol.ReturnType.ToDisplayString();
        bool isAsync = returnType.StartsWith("System.Threading.Tasks.Task");

        // Extract actual return type from Task<T>
        if (isAsync && methodSymbol.ReturnType is INamedTypeSymbol namedType)
        {
            if (namedType.TypeArguments.Length > 0)
            {
                returnType = namedType.TypeArguments[0].ToDisplayString();
            }
            else
            {
                returnType = "void"; // Task with no result
            }
        }

        // Get method attribute if exists
        var methodAttribute = methodSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name == "ServerFunctionAttribute");

        string? customRoute = null;
        bool requireAuthorization = false;
        string httpMethod = "POST";

        if (methodAttribute is not null)
        {
            foreach (var namedArg in methodAttribute.NamedArguments)
            {
                switch (namedArg.Key)
                {
                    case "Route":
                        customRoute = namedArg.Value.Value?.ToString();
                        break;
                    case "HttpMethod":
                        httpMethod = namedArg.Value.Value?.ToString() ?? "POST";
                        break;
                    case "RequireAuthorization":
                        requireAuthorization = namedArg.Value.Value is true;
                        break;
                }
            }
        }

        // Parse parameters
        var parameters = new List<ParameterInfo>();
        foreach (var param in methodSymbol.Parameters)
        {
            parameters.Add(new ParameterInfo
            {
                Name = param.Name,
                Type = param.Type.ToDisplayString(),
                HasDefaultValue = param.HasExplicitDefaultValue,
                DefaultValue = param.HasExplicitDefaultValue ? param.ExplicitDefaultValue?.ToString() : null
            });
        }

        return new MethodInfo
        {
            Name = methodSymbol.Name,
            ReturnType = returnType,
            IsAsync = isAsync,
            RequireAuthorization = requireAuthorization,
            Parameters = parameters,
            CustomRoute = customRoute,
            HttpMethod = httpMethod
        };
    }
}