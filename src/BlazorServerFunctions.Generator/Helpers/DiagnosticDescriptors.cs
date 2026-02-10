// File: BlazorServerFunctions.Generator/Diagnostics/DiagnosticDescriptors.cs

using Microsoft.CodeAnalysis;

namespace BlazorServerFunctions.Generator.Helpers;

/// <summary>
/// Centralized diagnostic descriptors for the BlazorServerFunctions source generator.
/// All error codes (BSF001-BSF999) are defined here.
/// </summary>
internal static class DiagnosticDescriptors
{
    private const string Category = "Usage";

    // ========================================
    // ERRORS: BSF001-BSF099
    // ========================================

    /// <summary>
    /// BSF001: Interface must have [ServerFunctionCollection] attribute
    /// </summary>
    public static readonly DiagnosticDescriptor MissingServerFunctionCollectionAttribute = new(
        id: "BSF001",
        title: "Missing server function collection attribute",
        messageFormat: "Interface '{0}' must have a [ServerFunctionCollection] attribute",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// BSF002: Method must have [ServerFunction] attribute
    /// </summary>
    public static readonly DiagnosticDescriptor MissingServerFunctionAttribute = new(
        id: "BSF002",
        title: "Missing server function attribute",
        messageFormat: "Method '{0}' must have a [ServerFunction] attribute",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// BSF003: Interface must be public
    /// Priority: HIGH
    /// </summary>
    public static readonly DiagnosticDescriptor InterfaceMustBePublic = new(
        id: "BSF003",
        title: "Interface must be public",
        messageFormat: "Interface '{0}' must be public to generate server functions",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// BSF004: Interface cannot be generic
    /// Priority: MEDIUM
    /// </summary>
    public static readonly DiagnosticDescriptor InterfaceCannotBeGeneric = new(
        id: "BSF004",
        title: "Interface cannot be generic",
        messageFormat: "Interface '{0}' cannot have generic type parameters",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// BSF005: Interface cannot have properties
    /// Priority: LOW
    /// </summary>
    public static readonly DiagnosticDescriptor InterfaceCannotHaveProperties = new(
        id: "BSF005",
        title: "Interface cannot have properties",
        messageFormat: "Interface '{0}' contains properties. Only methods are supported.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// BSF006: Interface cannot have events
    /// Priority: LOW
    /// </summary>
    public static readonly DiagnosticDescriptor InterfaceCannotHaveEvents = new(
        id: "BSF006",
        title: "Interface cannot have events",
        messageFormat: "Interface '{0}' contains events. Only methods are supported.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// BSF007: Method must return Task/ValueTask
    /// Priority: CRITICAL - Currently throws exception at InterfaceParser.cs:105
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidReturnType = new(
        id: "BSF007",
        title: "Invalid return type",
        messageFormat: "Method '{0}' must return Task, Task<T>, ValueTask, or ValueTask<T>",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// BSF008: Method cannot be generic
    /// Priority: MEDIUM
    /// </summary>
    public static readonly DiagnosticDescriptor MethodCannotBeGeneric = new(
        id: "BSF008",
        title: "Method cannot be generic",
        messageFormat: "Method '{0}' cannot have generic type parameters",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// BSF009: Method cannot have out parameters
    /// Priority: MEDIUM
    /// </summary>
    public static readonly DiagnosticDescriptor OutParametersNotSupported = new(
        id: "BSF009",
        title: "Out parameters not supported",
        messageFormat: "Method '{0}' has 'out' parameter '{1}'. Out parameters are not supported.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// BSF010: Method cannot have ref parameters
    /// Priority: MEDIUM
    /// </summary>
    public static readonly DiagnosticDescriptor RefParametersNotSupported = new(
        id: "BSF010",
        title: "Ref parameters not supported",
        messageFormat: "Method '{0}' has 'ref' parameter '{1}'. Ref parameters are not supported.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// BSF011: Method cannot have params parameters
    /// Priority: LOW
    /// </summary>
    public static readonly DiagnosticDescriptor ParamsNotSupported = new(
        id: "BSF011",
        title: "Params parameters not supported",
        messageFormat: "Method '{0}' has 'params' parameter '{1}'. Params arrays are not supported.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// BSF012: HttpMethod is required
    /// Priority: CRITICAL - Currently uses null-forgiving operator at InterfaceParser.cs:140
    /// </summary>
    public static readonly DiagnosticDescriptor HttpMethodRequired = new(
        id: "BSF012",
        title: "HttpMethod is required",
        messageFormat: "Method '{0}' must specify HttpMethod in [ServerFunction] attribute",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// BSF013: Invalid HttpMethod value
    /// Priority: HIGH
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidHttpMethod = new(
        id: "BSF013",
        title: "Invalid HttpMethod",
        messageFormat: "Method '{0}' has invalid HttpMethod '{1}'. Valid values: GET, POST, PUT, DELETE, PATCH.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// BSF014: Duplicate route detected
    /// Priority: MEDIUM
    /// </summary>
    public static readonly DiagnosticDescriptor DuplicateRoute = new(
        id: "BSF014",
        title: "Duplicate route",
        messageFormat: "Method '{0}' has duplicate route '{1}' already used by method '{2}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// BSF015: Invalid route format
    /// Priority: LOW
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidRouteFormat = new(
        id: "BSF015",
        title: "Invalid route format",
        messageFormat: "Method '{0}' has invalid route format '{1}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// BSF016: Failed to parse referenced interface
    /// Priority: LOW
    /// </summary>
    public static readonly DiagnosticDescriptor ReferencedInterfaceParseFailure = new(
        id: "BSF016",
        title: "Failed to parse referenced interface",
        messageFormat: "Failed to parse referenced interface '{0}' in assembly '{1}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // ========================================
    // WARNINGS: BSF101+
    // ========================================

    /// <summary>
    /// BSF101: Interface has no methods
    /// </summary>
    public static readonly DiagnosticDescriptor EmptyInterface = new(
        id: "BSF101",
        title: "Interface has no methods",
        messageFormat: "Interface '{0}' has no methods. No code will be generated.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// BSF102: Method has many parameters
    /// </summary>
    public static readonly DiagnosticDescriptor TooManyParameters = new(
        id: "BSF102",
        title: "Method has many parameters",
        messageFormat: "Method '{0}' has {1} parameters. Consider using a request object.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}