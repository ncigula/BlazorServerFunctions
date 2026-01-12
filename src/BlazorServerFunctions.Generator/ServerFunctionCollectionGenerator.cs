using System.Collections.Immutable;
using BlazorServerFunctions.Abstractions;
using BlazorServerFunctions.Generator.CodeGenerators;
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
        // Step 1: Find all interfaces with [ServerFunction] attribute
        var interfaceDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsCandidateInterface(node),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);

        // Step 2: Detect project type (Server vs Client)
        var projectInfo = context.AnalyzerConfigOptionsProvider
            .Select(static (options, _) => GetProjectInfo(options));

        // Step 3: Combine everything
        var compilation = context.CompilationProvider.Combine(interfaceDeclarations.Collect());
        var compilationAndProject = compilation.Combine(projectInfo);

        // Step 4: Generate code
        context.RegisterSourceOutput(compilationAndProject,
            static (spc, source) => Execute(spc, source.Left.Left, source.Left.Right, source.Right));
    }
    
    // Check if a syntax node could be an interface with our attribute
    private static bool IsCandidateInterface(SyntaxNode node)
    {
        if (node is not InterfaceDeclarationSyntax interfaceDecl)
            return false;

        // Quick check: does it have any attributes?
        return interfaceDecl.AttributeLists.Count > 0;
    }
    
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
                var symbol = context.SemanticModel.GetSymbolInfo(attribute).Symbol;
                if (symbol is not IMethodSymbol attributeSymbol)
                    continue;

                var attributeType = attributeSymbol.ContainingType;
                
                var fullName = attributeType.ToDisplayString();

                if (fullName == "BlazorServerFunctions.Abstractions.ServerFunctionCollectionAttribute")
                {
                    return interfaceDecl;
                }
            }
        }

        return null;
    }
    
    // Detect if this is Server or Client project
    private static ProjectInfo GetProjectInfo(AnalyzerConfigOptionsProvider options)
{
    options.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace);
    options.GlobalOptions.TryGetValue("build_property.ProjectName", out var projectName);
    
    // Strategy 1: Check project SDK type
    options.GlobalOptions.TryGetValue("build_property.UsingMicrosoftNETSdkWeb", out var isWeb);
    options.GlobalOptions.TryGetValue("build_property.UsingMicrosoftNETSdkBlazorWebAssembly", out var isWasm);
    
    bool isClientProject = bool.Parse(isWasm ?? "false");
    bool isServerProject = bool.Parse(isWeb ?? "false");
    
    // Strategy 2: Check for references that hint at project type
    // (This is harder in incremental generators, so we'll skip it)
    
    // Strategy 3: Naming convention as last resort
    if (!isClientProject && !isServerProject && projectName != null)
    {
        // Check naming patterns
        isClientProject = projectName.EndsWith(".Client") || 
                         projectName.EndsWith(".WASM") ||
                         projectName.EndsWith(".WebAssembly");
                         
        isServerProject = projectName.EndsWith(".Server") || 
                         projectName.EndsWith(".Web") ||
                         projectName.Contains("API");
    }
    
    // Allow explicit override
    options.GlobalOptions.TryGetValue("build_property.GenerateServerFunctionEndpoints", out var explicitEndpoints);
    options.GlobalOptions.TryGetValue("build_property.GenerateServerFunctionClients", out var explicitClients);
    
    bool generateEndpoints = bool.TryParse(explicitEndpoints, out var e) 
        ? e 
        : isServerProject;
        
    bool generateClients = bool.TryParse(explicitClients, out var c) 
        ? c 
        : isClientProject;

    return new ProjectInfo
    {
        RootNamespace = rootNamespace ?? "Generated",
        ProjectName = projectName ?? "Unknown",
        IsClientProject = isClientProject,
        IsServerProject = isServerProject,
        GenerateEndpoints = generateEndpoints,
        GenerateClients = generateClients
    };
}
    
    // Main execution method
    private static void Execute(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<InterfaceDeclarationSyntax?> interfaces,
        ProjectInfo projectInfo)
    {
        if (interfaces.IsDefaultOrEmpty)
            return;

        var interfaceInfos = new List<InterfaceInfo>();

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
                interfaceInfos.Add(interfaceInfo);
            }
        }

        if (interfaceInfos.Count == 0)
            return;

        // Generate server endpoints
        if (projectInfo.GenerateEndpoints)
        {
            foreach (var interfaceInfo in interfaceInfos)
            {
                var serverCode = ServerEndpointGenerator.Generate(interfaceInfo);
                context.AddSource($"{interfaceInfo.Name}ServerExtensions.g.cs", serverCode);
            }

            // Generate master registration
            var registrationCode = ServerRegistrationGenerator.Generate(interfaceInfos);
            context.AddSource("ServerFunctionEndpointsRegistration.g.cs", registrationCode);
        }

        // Generate client proxies
        if (projectInfo.GenerateClients)
        {
            foreach (var interfaceInfo in interfaceInfos)
            {
                var clientCode = ClientProxyGenerator.Generate(interfaceInfo);
                context.AddSource($"{interfaceInfo.Name}Client.g.cs", clientCode);
            }

            // Generate client registration
            var clientRegistrationCode = ClientRegistrationGenerator.Generate(interfaceInfos);
            context.AddSource("ServerFunctionClientsRegistration.g.cs", clientRegistrationCode);
        }
    }

    // Parse interface symbol into our data model
    private static InterfaceInfo? ParseInterface(
        INamedTypeSymbol interfaceSymbol,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Get [ServerFunction] attribute data
        var attribute = interfaceSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == 
                "BlazorServerFunctions.Abstractions.ServerFunctionAttribute");

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
            Symbol = interfaceSymbol,
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
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == 
                "BlazorServerFunctions.Abstractions.ServerFunctionAttribute");

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
            Symbol = methodSymbol,
            ReturnType = returnType,
            IsAsync = isAsync,
            RequireAuthorization = requireAuthorization,
            Parameters = parameters,
            CustomRoute = customRoute,
            HttpMethod = httpMethod
        };
    }
}

public sealed record ProjectInfo
{
    public string RootNamespace { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public bool IsClientProject { get; set; }
    public bool IsServerProject { get; set; }
    public bool GenerateEndpoints { get; set; }
    public bool GenerateClients { get; set; }
}
