using HttpMethod = BlazorServerFunctions.Generator.Models.HttpMethod;

namespace BlazorServerFunctions.Generator.UnitTests;

/// <summary>
/// Unit tests for ServerEndpointGenerator - testing string output generation
/// Tests every possible variation of InterfaceInfo properties and endpoint generation
/// </summary>
public class ServerEndpointGeneratorTests
{
    // ========================================
    // BASIC STRUCTURE TESTS
    // ========================================

    /// <summary>
    /// Setup: Basic interface (IUserService) in MyApp.Services namespace with route prefix "users" and single GET method
    /// Testing: Basic structure generation - extension class naming, MapGroup usage, and method registration
    /// Assert: Generates IUserServiceServerExtensions class with MapIUserServiceEndpoints method using correct namespace, route group, and MapGet registration
    /// </summary>
    [Fact]
    public Task Generate_BasicInterface_ProducesCorrectCode()
    {
        var interfaceInfo = TestDataFactory.BasicGetInterface();

        var generated = ServerEndpointGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: CRUD interface (IUserService) with route prefix "users" containing four methods: GetAllAsync (GET), GetByIdAsync (GET), CreateAsync (POST), DeleteAsync (DELETE)
    /// Testing: Multiple methods in single interface - each method should be registered correctly with the appropriate HTTP verb
    /// Assert: Generates extension class with four endpoint registrations (MapGet, MapGet, MapPost, MapDelete) with correct routes and endpoint names
    /// </summary>
    [Fact]
    public Task Generate_MultipleMethodsInterface_ProducesCorrectCode()
    {
        var interfaceInfo = TestDataFactory.CrudInterface();

        var generated = ServerEndpointGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface (ICustomService) in deeply nested namespace "Custom.Deeply.Nested.Namespace.Services" with route prefix "custom" and single GET method
    /// Testing: Namespace handling - generator should use exact namespace provided in InterfaceInfo
    /// Assert: Generates extension class in Custom.Deeply.Nested.Namespace.Services namespace with correct using statements
    /// </summary>
    [Fact]
    public Task Generate_DifferentNamespace_UsesCorrectNamespace()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("ICustomService")
            .WithNamespace("Custom.Deeply.Nested.Namespace.Services")
            .WithRoutePrefix("custom")
            .WithMethod(TestDataFactory.Method.BasicGet())
            .Build();

        var generated = ServerEndpointGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface named ISpecialService in MyApp.Services namespace with route prefix "special" and single GET method
    /// Testing: Extension class naming convention - should be {InterfaceName}ServerExtensions
    /// Assert: Generates class named ISpecialServiceServerExtensions with method named MapISpecialServiceEndpoints
    /// </summary>
    [Fact]
    public Task Generate_ExtensionClassName_UsesInterfaceName()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("ISpecialService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("special")
            .WithMethod(TestDataFactory.Method.BasicGet())
            .Build();

        var generated = ServerEndpointGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface (INestedService) with complex route prefix "api/v2/nested/service" and single GET method
    /// Testing: Route group prefix handling - MapGroup should use exact route prefix including forward slashes
    /// Assert: Generates code with MapGroup("api/v2/nested/service") creating versioned/nested route structure
    /// </summary>
    [Fact]
    public Task Generate_RouteGroup_UsesRoutePrefix()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("INestedService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("api/v2/nested/service")
            .WithMethod(TestDataFactory.Method.BasicGet())
            .Build();

        var generated = ServerEndpointGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    // ========================================
    // HTTP METHOD MAPPING TESTS
    // ========================================

    /// <summary>
    /// Setup: Interface (IUserService) with route prefix "users" containing a single GET method (GetAllAsync)
    /// Testing: HTTP GET verb mapping - should use MapGet for GET methods
    /// Assert: Generates endpoint registration using group.MapGet(...) with the correct route and endpoint name
    /// </summary>
    [Fact]
    public Task Generate_GetEndpoint_MapsCorrectly()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("IUserService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("users")
            .WithMethod(TestDataFactory.Method.BasicGet())
            .Build();

        var generated = ServerEndpointGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface (IUserService) with route prefix "users" containing single POST method (CreateAsync) with id and name parameters
    /// Testing: HTTP POST verb mapping - should use MapPost for POST methods and include request DTO
    /// Assert: Generates endpoint registration using group.MapPost(...) with CreateAsyncRequest DTO class and FromBody parameter binding
    /// </summary>
    [Fact]
    public Task Generate_PostEndpoint_MapsCorrectly()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("IUserService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("users")
            .WithMethod(TestDataFactory.Method.BasicPost())
            .Build();

        var generated = ServerEndpointGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface (IUserService) with route prefix "users" containing single PUT method (UpdateUserAsync) with id and name parameters returning User type
    /// Testing: HTTP PUT verb mapping - should use MapPut for PUT methods
    /// Assert: Generates endpoint registration using group.MapPut(...) with UpdateUserAsyncRequest DTO and correct service call with request.Id and request.Name
    /// </summary>
    [Fact]
    public Task Generate_PutEndpoint_MapsCorrectly()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("IUserService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("users")
            .WithMethod(new MethodInfoBuilder()
                .WithName("UpdateUserAsync")
                .Returning("User")
                .UsingHttp(HttpMethod.Put)
                .IsAsyncMethod(AsyncType.Task)
                .WithParameters(TestDataFactory.Param.Id(), TestDataFactory.Param.Name())
                .Build())
            .Build();

        var generated = ServerEndpointGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface (IUserService) with route prefix "users" containing a single PATCH method (PatchUserAsync) with id and name parameters returning User type
    /// Testing: HTTP PATCH verb mapping - should use MapPatch for PATCH methods
    /// Assert: Generates endpoint registration using group.MapPatch(...) with PatchUserAsyncRequest DTO and correct service invocation
    /// </summary>
    [Fact]
    public Task Generate_PatchEndpoint_MapsCorrectly()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("IUserService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("users")
            .WithMethod(new MethodInfoBuilder()
                .WithName("PatchUserAsync")
                .Returning("User")
                .UsingHttp(HttpMethod.Patch)
                .IsAsyncMethod(AsyncType.Task)
                .WithParameters(TestDataFactory.Param.Id(), TestDataFactory.Param.Name())
                .Build())
            .Build();

        var generated = ServerEndpointGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface (IUserService) with route prefix "users" containing a single DELETE method (DeleteAsync) with id parameter
    /// Testing: HTTP DELETE verb mapping - should use MapDelete for DELETE methods
    /// Assert: Generates endpoint registration using group.MapDelete(...) with DeleteAsyncRequest DTO containing the Id property
    /// </summary>
    [Fact]
    public Task Generate_DeleteEndpoint_MapsCorrectly()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("IUserService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("users")
            .WithMethod(TestDataFactory.Method.Delete())
            .Build();

        var generated = ServerEndpointGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    // ========================================
    // PARAMETER TESTS
    // ========================================

    /// <summary>
    /// Setup: Interface (IMessageService) with route prefix "messages" containing parameterless GET method (GetAllMessagesAsync)
    /// Testing: Parameterless method handling - should not generate request DTO when the method has no parameters
    /// Assert: Generates endpoint with direct service call without request DTO or FromBody binding - service.GetAllMessagesAsync(cancellationToken)
    /// </summary>
    [Fact]
    public Task Generate_MethodWithNoParameters_NoRequestDto()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("IMessageService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("messages")
            .WithMethod(TestDataFactory.Method.WithNoParameters())
            .Build();

        var generated = ServerEndpointGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface (IUserService) with POST method (CreateUserAsync) containing three parameters: name (string), email (string), age (int)
    /// Testing: Request DTO generation for parameterized methods - should create inner DTO class with properties matching parameters
    /// Assert: Generates CreateUserAsyncRequest DTO with Name, Email, Age properties and endpoint accepting [FromBody] CreateUserAsyncRequest request
    /// </summary>
    [Fact]
    public Task Generate_MethodWithParameters_CreatesRequestDto()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("IUserService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("users")
            .WithMethod(new MethodInfoBuilder()
                .WithName("CreateUserAsync")
                .Returning("User")
                .UsingHttp(HttpMethod.Post)
                .IsAsyncMethod(AsyncType.Task)
                .WithParameters(
                    TestDataFactory.Param.Name(),
                    TestDataFactory.Param.Email(),
                    new ParameterInfoBuilder().WithName("age").WithType("int").Build())
                .Build())
            .Build();

        var generated = ServerEndpointGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface (IUserService) with POST method (CreateUserAsync) containing camelCase parameters: firstName, lastName, emailAddress
    /// Testing: Property name casing transformation - DTO properties should be PascalCase regardless of parameter casing
    /// Assert: Generates CreateUserAsyncRequest DTO with FirstName, LastName, EmailAddress properties (PascalCase) from firstName, lastName, emailAddress parameters
    /// </summary>
    [Fact]
    public Task Generate_RequestDto_UsesPascalCaseProperties()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("IUserService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("users")
            .WithMethod(new MethodInfoBuilder()
                .WithName("CreateUserAsync")
                .Returning("User")
                .UsingHttp(HttpMethod.Post)
                .IsAsyncMethod(AsyncType.Task)
                .WithParameters(
                    new ParameterInfoBuilder().WithName("firstName").WithType("string").Build(),
                    new ParameterInfoBuilder().WithName("lastName").WithType("string").Build(),
                    new ParameterInfoBuilder().WithName("emailAddress").WithType("string").Build())
                .Build())
            .Build();

        var generated = ServerEndpointGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface (ICalculatorService) with POST method (AddAsync) containing two int parameters: firstNumber, secondNumber
    /// Testing: Parameter passing from request DTO to service method - should use request.PropertyName syntax
    /// Assert: Generates service call as service.AddAsync(request.FirstNumber, request.SecondNumber, cancellationToken) with PascalCase property access
    /// </summary>
    [Fact]
    public Task Generate_ServiceCall_PassesParametersCorrectly()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("ICalculatorService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("calculator")
            .WithMethod(new MethodInfoBuilder()
                .WithName("AddAsync")
                .Returning("int")
                .UsingHttp(HttpMethod.Post)
                .IsAsyncMethod(AsyncType.Task)
                .WithParameters(
                    new ParameterInfoBuilder().WithName("firstNumber").WithType("int").Build(),
                    new ParameterInfoBuilder().WithName("secondNumber").WithType("int").Build())
                .Build())
            .Build();

        var generated = ServerEndpointGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface (ITestService) with three methods: GetAllMessagesAsync (no params), CreateAsync (with id/name params), AnotherMethodAsync (no params)
    /// Testing: Selective DTO generation - only methods with parameters should have request DTOs generated
    /// Assert: Generates CreateAsyncRequest DTO only, while GetAllMessagesAsync and AnotherMethodAsync have direct service calls without DTOs
    /// </summary>
    [Fact]
    public Task Generate_MixOfParameterizedAndParameterlessMethodsCreatesSelectiveDtos()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("ITestService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("test")
            .WithMethods(
                TestDataFactory.Method.WithNoParameters(),
                TestDataFactory.Method.BasicPost(),
                new MethodInfoBuilder()
                    .WithName("AnotherMethodAsync")
                    .Returning("void")
                    .UsingHttp(HttpMethod.Post)
                    .IsAsyncMethod(AsyncType.Task)
                    .Build())
            .Build();

        var generated = ServerEndpointGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    // ========================================
    // RETURN TYPE TESTS
    // ========================================

    /// <summary>
    /// Setup: Interface (INotificationService) with route prefix "notifications" containing void POST method (SendNotificationAsync)
    /// Testing: Void return type handling - should return Results.Ok() without value
    /// Assert: Generates endpoint returning Results.Ok() after service call, no result variable or Results.Ok(result)
    /// </summary>
    [Fact]
    public Task Generate_VoidReturnType_ReturnsOkOnly()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("INotificationService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("notifications")
            .WithMethod(TestDataFactory.Method.VoidMethod())
            .Build();

        var generated = ServerEndpointGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface (ICounterService) with route prefix "counter" containing GET method (GetCountAsync) returning int
    /// Testing: Non-void return type handling - should return Results.Ok(result) with the returned value
    /// Assert: Generates endpoint with result variable assignment and Results.Ok(result) returning the int value from service call
    /// </summary>
    [Fact]
    public Task Generate_NonVoidReturnType_ReturnsOkWithResult()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("ICounterService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("counter")
            .WithMethod(new MethodInfoBuilder()
                .WithName("GetCountAsync")
                .Returning("int")
                .UsingHttp(HttpMethod.Get)
                .IsAsyncMethod(AsyncType.Task)
                .Build())
            .Build();

        var generated = ServerEndpointGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface (IUserService) with GET method (GetAllUsersAsync) returning complex generic type List&lt;User&gt;
    /// Testing: Complex return type handling - should handle generic types and return Results.Ok(result) with proper type
    /// Assert: Generates endpoint capturing List&lt;User&gt; result and returning Results.Ok(result) with the collection
    /// </summary>
    [Fact]
    public Task Generate_ComplexReturnType_ReturnsOkWithResult()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("IUserService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("users")
            .WithMethod(new MethodInfoBuilder()
                .WithName("GetAllUsersAsync")
                .Returning("List<User>")
                .UsingHttp(HttpMethod.Get)
                .IsAsyncMethod(AsyncType.Task)
                .Build())
            .Build();

        var generated = ServerEndpointGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    // ========================================
    // ENDPOINT NAMING TESTS
    // ========================================

    /// <summary>
    /// Setup: Interface (IUserService) with three methods: GetAllAsync, GetByIdAsync, CreateAsync
    /// Testing: Endpoint name uniqueness - each endpoint should have unique name based on InterfaceName_MethodName pattern
    /// Assert: Generates three endpoints with unique names: IUserService_GetAllAsync, IUserService_GetByIdAsync, IUserService_CreateAsync using WithName()
    /// </summary>
    [Fact]
    public Task Generate_EndpointNames_AreUnique()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("IUserService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("users")
            .WithMethods(
                TestDataFactory.Method.GetList(),
                TestDataFactory.Method.GetById(),
                TestDataFactory.Method.Create())
            .Build();

        var generated = ServerEndpointGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface (ISpecialService) with a single method (SpecialMethodAsync) returning Result type
    /// Testing: Endpoint naming convention - should follow {InterfaceName}_{MethodName} format
    /// Assert: Generates endpoint with WithName("ISpecialService_SpecialMethodAsync") combining interface and method names
    /// </summary>
    [Fact]
    public Task Generate_EndpointNames_UseInterfaceAndMethodName()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("ISpecialService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("special")
            .WithMethod(new MethodInfoBuilder()
                .WithName("SpecialMethodAsync")
                .Returning("Result")
                .UsingHttp(HttpMethod.Get)
                .IsAsyncMethod(AsyncType.Task)
                .Build())
            .Build();

        var generated = ServerEndpointGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    // ========================================
    // ROUTE TESTS
    // ========================================

    /// <summary>
    /// Setup: Interface (IUserService) with GET method (GetUserByIdAsync) having custom route "by-id" and id parameter
    /// Testing: Custom route handling - method-level custom route should override default route name
    /// Assert: Generates MapGet with route "by-id" instead of method name, still creating GetUserByIdAsyncRequest DTO for the id parameter
    /// </summary>
    [Fact]
    public Task Generate_CustomRoute_UsesCustomRouteName()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("IUserService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("users")
            .WithMethod(new MethodInfoBuilder()
                .WithName("GetUserByIdAsync")
                .Returning("User")
                .UsingHttp(HttpMethod.Get)
                .IsAsyncMethod(AsyncType.Task)
                .WithCustomRoute("by-id")
                .WithParameter(TestDataFactory.Param.Id())
                .Build())
            .Build();

        var generated = ServerEndpointGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface (IUserService) with GET method (GetUserAsync) without custom route, having id parameter
    /// Testing: Default route behavior - should use method name as route when no custom route specified
    /// Assert: Generates MapGet with route "GetUserAsync" derived from the method name, with GetUserAsyncRequest DTO for the id parameter
    /// </summary>
    [Fact]
    public Task Generate_DefaultRoute_UsesMethodName()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("IUserService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("users")
            .WithMethod(new MethodInfoBuilder()
                .WithName("GetUserAsync")
                .Returning("User")
                .UsingHttp(HttpMethod.Get)
                .IsAsyncMethod(AsyncType.Task)
                .WithParameter(TestDataFactory.Param.Id())
                .Build())
            .Build();

        var generated = ServerEndpointGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }
}
