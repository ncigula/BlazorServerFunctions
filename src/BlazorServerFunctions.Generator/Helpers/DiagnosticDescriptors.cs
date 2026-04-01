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
        messageFormat: "Method '{0}' must return Task, Task<T>, ValueTask, ValueTask<T>, or IAsyncEnumerable<T>",
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

    /// <summary>
    /// BSF017: Route parameter has no matching method parameter
    /// </summary>
    public static readonly DiagnosticDescriptor RouteParameterNotFound = new(
        id: "BSF017",
        title: "Route parameter not found",
        messageFormat: "Route parameter '{{{1}}}' in method '{0}' does not match any method parameter",
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

    /// <summary>
    /// BSF018: Route parameter has complex type that may not be route-bindable
    /// </summary>
    public static readonly DiagnosticDescriptor RouteParameterComplexType = new(
        id: "BSF018",
        title: "Route parameter has complex type",
        messageFormat: "Route parameter '{1}' in method '{0}' has type '{2}' which may not be route-bindable; route parameters should be primitive types",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// BSF019: CacheSeconds configured on a streaming method — ignored
    /// </summary>
    public static readonly DiagnosticDescriptor CacheOnStreamingMethod = new(
        id: "BSF019",
        title: "Output caching incompatible with streaming",
        messageFormat: "Method '{0}' returns IAsyncEnumerable<T> — output caching is incompatible with streaming and CacheSeconds will be ignored",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// BSF020: CacheSeconds configured on a non-GET method — caching mutating endpoints is almost always wrong
    /// </summary>
    public static readonly DiagnosticDescriptor CacheOnNonGetMethod = new(
        id: "BSF020",
        title: "Output caching on non-GET method",
        messageFormat: "Method '{0}' uses HTTP {1} — output caching is only valid for GET endpoints. CacheSeconds on POST/PUT/PATCH/DELETE will cache side-effecting responses, breaking correctness.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// BSF021: Roles is set to an empty string — has no effect
    /// </summary>
    public static readonly DiagnosticDescriptor EmptyRoles = new(
        id: "BSF021",
        title: "Empty Roles value",
        messageFormat: "Method '{0}' has Roles set to an empty string, which has no effect. Remove the Roles property or provide role names.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// BSF022: CorsPolicy is set to an empty string — has no effect
    /// </summary>
    public static readonly DiagnosticDescriptor EmptyCorsPolicy = new(
        id: "BSF022",
        title: "Empty CorsPolicy value",
        messageFormat: "Interface '{0}' has CorsPolicy set to an empty string, which has no effect. Remove the CorsPolicy property or provide a policy name.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// BSF023: HttpMethod on a gRPC interface method — gRPC always uses HTTP POST at the transport layer
    /// </summary>
    public static readonly DiagnosticDescriptor HttpMethodIgnoredForGrpc = new(
        id: "BSF023",
        title: "HttpMethod has no effect on gRPC interface",
        messageFormat: "Method '{0}' is on a gRPC interface — HttpMethod has no effect and must be removed (gRPC always uses HTTP POST at the transport layer)",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // ========================================
    // WARNINGS: BSF101+ (continued)
    // ========================================

    /// <summary>
    /// BSF024: CacheSeconds on a gRPC interface method — output caching is not supported for gRPC
    /// </summary>
    public static readonly DiagnosticDescriptor CacheSecondsIgnoredForGrpc = new(
        id: "BSF024",
        title: "CacheSeconds has no effect on gRPC interface",
        messageFormat: "Method '{0}' is on a gRPC interface — output caching (CacheSeconds) is not supported for gRPC and will be ignored",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// BSF025: RequireAntiForgery on a gRPC interface method — anti-forgery is not supported for gRPC
    /// </summary>
    public static readonly DiagnosticDescriptor AntiForgeryIgnoredForGrpc = new(
        id: "BSF025",
        title: "RequireAntiForgery has no effect on gRPC interface",
        messageFormat: "Method '{0}' is on a gRPC interface — anti-forgery is not supported for gRPC and will be ignored",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// BSF026: File upload parameter on a GET or DELETE method
    /// </summary>
    public static readonly DiagnosticDescriptor FileUploadOnGetOrDelete = new(
        id: "BSF026",
        title: "File upload parameter not valid on GET or DELETE",
        messageFormat: "Method '{0}' has a file upload parameter '{1}' but uses HTTP {2}. File upload (Stream/IFormFile) requires POST, PUT, or PATCH.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// BSF027: File upload parameter combined with IAsyncEnumerable return type
    /// </summary>
    public static readonly DiagnosticDescriptor FileUploadWithStreamingReturn = new(
        id: "BSF027",
        title: "File upload parameter incompatible with streaming return",
        messageFormat: "Method '{0}' has a file upload parameter but returns IAsyncEnumerable<T>. Multipart upload and streaming response cannot be combined.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// BSF028: File upload parameter on a gRPC interface method
    /// </summary>
    public static readonly DiagnosticDescriptor FileUploadNotSupportedForGrpc = new(
        id: "BSF028",
        title: "File upload parameters not supported on gRPC interfaces",
        messageFormat: "Method '{0}' has a file upload parameter '{1}' but is on a gRPC interface. File upload (Stream/IFormFile) is only supported for REST interfaces.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// BSF029: ResultMapper set on a gRPC interface — result mapping is REST-only
    /// </summary>
    public static readonly DiagnosticDescriptor ResultMapperNotSupportedForGrpc = new(
        id: "BSF029",
        title: "ResultMapper not supported on gRPC interfaces",
        messageFormat: "Interface '{0}' has ResultMapper set but uses gRPC transport. Result mapping is only supported for REST interfaces.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// BSF030: ResultMapper set but a method's return type is non-generic — the mapper cannot be applied
    /// </summary>
    public static readonly DiagnosticDescriptor ResultMapperReturnTypeNotGeneric = new(
        id: "BSF030",
        title: "ResultMapper cannot be applied to non-generic return type",
        messageFormat: "Method '{0}' has a non-generic return type '{1}'. ResultMapper requires generic return types (e.g. Result<T>) so the inner value type can be extracted. This method will fall back to Results.Ok(result) / direct deserialisation.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// BSF031: ParameterSource.Route specified but {paramName} not present in route template
    /// </summary>
    public static readonly DiagnosticDescriptor ExplicitRouteParameterMissingFromTemplate = new(
        id: "BSF031",
        title: "Explicit Route parameter not in route template",
        messageFormat: "Parameter '{0}' on method '{1}' is marked ParameterSource.Route but '{{{0}}}' is not present in the route template",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// BSF032: ParameterSource.Body on GET or DELETE
    /// </summary>
    public static readonly DiagnosticDescriptor BodyParameterOnGetOrDelete = new(
        id: "BSF032",
        title: "Body parameter on GET or DELETE method",
        messageFormat: "Parameter '{0}' on method '{1}' is marked ParameterSource.Body but the method uses GET or DELETE. Browsers forbid a request body on GET/DELETE (Fetch API restriction), making this endpoint unreachable from WebAssembly clients.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}