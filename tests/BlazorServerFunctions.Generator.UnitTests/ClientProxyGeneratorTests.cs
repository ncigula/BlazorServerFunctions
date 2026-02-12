using HttpMethod = BlazorServerFunctions.Generator.Models.HttpMethod;

namespace BlazorServerFunctions.Generator.UnitTests;

/// <summary>
/// Unit tests for ClientProxyGenerator - testing string output generation
/// Tests every possible variation of InterfaceInfo properties and code generation paths
/// </summary>
public class ClientProxyGeneratorTests
{
    // ========================================
    // BASIC STRUCTURE TESTS
    // ========================================

    /// <summary>
    /// Setup: Interface with the name "IUserService", namespace "MyApp.Services", route prefix "users", one GET method with single int parameter returning UserDto
    /// Testing: Basic client proxy generation with standard setup
    /// Assert: Class named "UserServiceClient", implements IUserService, constructor takes HttpClient, BaseRoute uses route prefix, single method generated
    /// </summary>
    [Fact]
    public Task Generate_BasicInterface_ProducesCorrectCode()
    {
        var interfaceInfo = TestDataFactory.BasicGetInterface();

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface with name "IUserService", namespace "MyApp.Services", route prefix "users", 5 methods (GetList, GetById, Create, Update, Delete) covering all HTTP verbs
    /// Testing: Client proxy generation with multiple methods using different HTTP methods
    /// Assert: All 5 methods generated with correct HTTP method calls (GetAsync, PostAsJsonAsync, HttpRequestMessage for PUT/DELETE), proper parameter handling for each
    /// </summary>
    [Fact]
    public Task Generate_MultipleMethodsInterface_ProducesCorrectCode()
    {
        var interfaceInfo = TestDataFactory.CrudInterface();

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface with name "UserService" (no 'I' prefix), namespace "MyApp.Services", route prefix "users", one GET method
    /// Testing: Client proxy class name generation when interface name doesn't start with 'I'
    /// Assert: Class named "UserServiceClient" (correctly adds "Client" suffix without trying to remove 'I' prefix)
    /// </summary>
    [Fact]
    public Task Generate_InterfaceWithoutIPrefix_HandlesCorrectly()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("UserService") // No 'I' prefix
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("users")
            .WithMethod(TestDataFactory.Method.BasicGet())
            .Build();

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface with the name "ICustomService", namespace "Custom.Deeply.Nested.Namespace.Services", route prefix "custom", one GET method
    /// Testing: Client proxy namespace generation with deeply nested namespace
    /// Assert: Generated class uses exactly "Custom.Deeply.Nested.Namespace.Services" namespace
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

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface with the name "IServiceWithoutPrefix", namespace "MyApp.Services", route prefix NULL, one GET method
    /// Testing: BaseRoute generation when RoutePrefix is null
    /// Assert: BaseRoute constant is "/api/functions/" (with null at the end of the string interpolation)
    /// </summary>
    [Fact]
    public Task Generate_NullRoutePrefix_UsesNullInBaseRoute()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("IServiceWithoutPrefix")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix(null)
            .WithMethod(TestDataFactory.Method.BasicGet())
            .Build();

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface with the name "INestedService", namespace "MyApp.Services", route prefix "api/v2/nested/service", one GET method
    /// Testing: BaseRoute generation when RoutePrefix contains forward slashes
    /// Assert: BaseRoute is "/api/functions/api/v2/nested/service" (slashes preserved in route prefix)
    /// </summary>
    [Fact]
    public Task Generate_RoutePrefixWithSlashes_UsesCorrectly()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("INestedService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("api/v2/nested/service")
            .WithMethod(TestDataFactory.Method.BasicGet())
            .Build();

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    // ========================================
    // HTTP METHOD TESTS
    // ========================================

    /// <summary>
    /// Setup: Interface "IMessageService", one GET method "GetMessageAsync" returning string with NO parameters
    /// Testing: GET request generation without parameters
    /// Assert: Uses simple GetAsync call without query string building, route is just method name
    /// </summary>
    [Fact]
    public Task Generate_GetMethod_WithNoParameters_ProducesCorrectCode()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("IMessageService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("messages")
            .WithMethod(TestDataFactory.Method.WithNoParameters())
            .Build();

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface "ISearchService", one GET method "SearchAsync" with 2 parameters (query: string, pageSize: int)
    /// Testing: GET request generation with parameters (should build query string)
    /// Assert: Creates queryString using HttpUtility.ParseQueryString, adds parameters with PascalCase names (Query, PageSize), appends to URL
    /// </summary>
    [Fact]
    public Task Generate_GetMethod_WithParameters_ProducesQueryString()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("ISearchService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("search")
            .WithMethod(new MethodInfoBuilder()
                .WithName("SearchAsync")
                .Returning("List<Result>")
                .UsingHttp(HttpMethod.Get)
                .IsAsyncMethod(AsyncType.Task)
                .WithParameters(
                    new ParameterInfoBuilder().WithName("query").WithType("string").Build(),
                    new ParameterInfoBuilder().WithName("pageSize").WithType("int").Build())
                .Build())
            .Build();

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface "IActionService", one POST method "TriggerActionAsync" returning void with NO parameters
    /// Testing: POST request generation without parameters
    /// Assert: Uses PostAsync with null as content (no request object created)
    /// </summary>
    [Fact]
    public Task Generate_PostMethod_WithNoParameters_PostsNull()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("IActionService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("actions")
            .WithMethod(new MethodInfoBuilder()
                .WithName("TriggerActionAsync")
                .Returning("void")
                .UsingHttp(HttpMethod.Post)
                .IsAsyncMethod(AsyncType.Task)
                .Build())
            .Build();

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface "IUserService", one POST method "CreateUserAsync" with 2 parameters (name: string, email: string)
    /// Testing: POST request generation with parameters
    /// Assert: Creates an anonymous request object with PascalCase properties, uses PostAsJsonAsync with a request object
    /// </summary>
    [Fact]
    public Task Generate_PostMethod_WithParameters_PostsJson()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("IUserService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("users")
            .WithMethod(TestDataFactory.Method.BasicPost())
            .Build();

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface "IUserService", one PUT method "UpdateUserAsync" with 2 parameters (id: int, name: string)
    /// Testing: PUT request generation with parameters
    /// Assert: Creates HttpRequestMessage with Method = HttpMethod.PUT, creates a request object, uses JsonContent.Create(request) for content, uses SendAsync
    /// </summary>
    [Fact]
    public Task Generate_PutMethod_WithParameters_UsesHttpRequestMessage()
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

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface "IResourceService", one PUT method "RefreshAsync" with NO parameters returning void
    /// Testing: PUT request generation without parameters
    /// Assert: Creates HttpRequestMessage with Method = HttpMethod.PUT, NO content property set, uses SendAsync
    /// </summary>
    [Fact]
    public Task Generate_PutMethod_WithNoParameters_UsesHttpRequestMessage()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("IResourceService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("resources")
            .WithMethod(new MethodInfoBuilder()
                .WithName("RefreshAsync")
                .Returning("void")
                .UsingHttp(HttpMethod.Put)
                .IsAsyncMethod(AsyncType.Task)
                .Build())
            .Build();

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface "IUserService", one PATCH method "PatchUserAsync" with 2 parameters (id: int, name: string)
    /// Testing: PATCH request generation with parameters
    /// Assert: Creates HttpRequestMessage with Method = HttpMethod.PATCH, creates a request object, uses JsonContent.Create(request) for content, uses SendAsync
    /// </summary>
    [Fact]
    public Task Generate_PatchMethod_WithParameters_UsesHttpRequestMessage()
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

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface "IUserService", one DELETE method "DeleteUserAsync" with 1 parameter (id: int) returning void
    /// Testing: DELETE request generation with parameters
    /// Assert: Creates HttpRequestMessage with Method = HttpMethod.DELETE, creates a request object, uses JsonContent.Create(request) for content, uses SendAsync
    /// </summary>
    [Fact]
    public Task Generate_DeleteMethod_WithParameters_UsesHttpRequestMessage()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("IUserService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("users")
            .WithMethod(new MethodInfoBuilder()
                .WithName("DeleteUserAsync")
                .Returning("void")
                .UsingHttp(HttpMethod.Delete)
                .IsAsyncMethod(AsyncType.Task)
                .WithParameter(TestDataFactory.Param.Id())
                .Build())
            .Build();

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface "ICacheService", one DELETE method "ClearAllAsync" with NO parameters returning void
    /// Testing: DELETE request generation without parameters
    /// Assert: Creates HttpRequestMessage with Method = HttpMethod.DELETE, NO content property set, uses SendAsync
    /// </summary>
    [Fact]
    public Task Generate_DeleteMethod_WithNoParameters_UsesHttpRequestMessage()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("ICacheService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("cache")
            .WithMethod(new MethodInfoBuilder()
                .WithName("ClearAllAsync")
                .Returning("void")
                .UsingHttp(HttpMethod.Delete)
                .IsAsyncMethod(AsyncType.Task)
                .Build())
            .Build();

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    // ========================================
    // RETURN TYPE TESTS
    // ========================================

    /// <summary>
    /// Setup: Interface "INotificationService", one POST method "DoSomethingAsync" returning void
    /// Testing: Method generation with void return type
    /// Assert: NO ReadFromJsonAsync call, method just calls EnsureSuccessStatusCode and returns
    /// </summary>
    [Fact]
    public Task Generate_VoidReturnType_NoDeserialization()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("INotificationService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("notifications")
            .WithMethod(TestDataFactory.Method.VoidMethod())
            .Build();

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface "ICounterService", one GET method "GetCountAsync" returning int
    /// Testing: Method generation with simple value type return
    /// Assert: Uses ReadFromJsonAsync() to deserialize response
    /// </summary>
    [Fact]
    public Task Generate_SimpleReturnType_DeserializesCorrectly()
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

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface "IUserService", one GET method "GetAllUsersAsync" returning List&lt;User&gt;
    /// Testing: Method generation with generic List return type
    /// Assert: Uses ReadFromJsonAsync&lt;List&lt;User&gt;&gt;() to deserialize response
    /// </summary>
    [Fact]
    public Task Generate_ComplexReturnType_List_DeserializesCorrectly()
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

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface "IDataService", one GET method "GetMappingAsync" returning Dictionary&lt;string, List&lt;int&gt;&gt;
    /// Testing: Method generation with nested generic return type
    /// Assert: Uses ReadFromJsonAsync Dictionary string, List of int to deserialize response
    /// </summary>
    [Fact]
    public Task Generate_ComplexReturnType_Dictionary_DeserializesCorrectly()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("IDataService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("data")
            .WithMethod(new MethodInfoBuilder()
                .WithName("GetMappingAsync")
                .Returning("Dictionary<string, List<int>>")
                .UsingHttp(HttpMethod.Get)
                .IsAsyncMethod(AsyncType.Task)
                .Build())
            .Build();

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface "IUserService", one GET method "FindUserAsync" returning User? (nullable)
    /// Testing: Method generation with nullable reference return type
    /// Assert: Uses ReadFromJsonAsync User to deserialize response
    /// </summary>
    [Fact]
    public Task Generate_NullableReturnType_DeserializesCorrectly()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("IUserService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("users")
            .WithMethod(new MethodInfoBuilder()
                .WithName("FindUserAsync")
                .Returning("User?")
                .UsingHttp(HttpMethod.Get)
                .IsAsyncMethod(AsyncType.Task)
                .WithParameter(TestDataFactory.Param.Id())
                .Build())
            .Build();

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    // ========================================
    // ASYNC TYPE TESTS
    // ========================================

    /// <summary>
    /// Setup: Interface "ITaskService", one POST method "ProcessAsync" returning Result with AsyncType.Task
    /// Testing: Method signature generation with Task&lt;T&gt; return type
    /// Assert: Method signature is "public async Task&lt;Result&gt; ProcessAsync()"
    /// </summary>
    [Fact]
    public Task Generate_AsyncTypeTask_ProducesAsyncTask()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("ITaskService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("tasks")
            .WithMethod(new MethodInfoBuilder()
                .WithName("ProcessAsync")
                .Returning("Result")
                .UsingHttp(HttpMethod.Post)
                .IsAsyncMethod(AsyncType.Task)
                .Build())
            .Build();

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface "IValueTaskService", one POST method "ProcessAsync" returning Result with AsyncType.ValueTask
    /// Testing: Method signature generation with ValueTask&lt;T&gt; return type
    /// Assert: Method signature is "public async ValueTask&lt;Result&gt; ProcessAsync()"
    /// </summary>
    [Fact]
    public Task Generate_AsyncTypeValueTask_ProducesAsyncValueTask()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("IValueTaskService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("valuetasks")
            .WithMethod(new MethodInfoBuilder()
                .WithName("ProcessAsync")
                .Returning("Result")
                .UsingHttp(HttpMethod.Post)
                .IsAsyncMethod(AsyncType.ValueTask)
                .Build())
            .Build();

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface "ISyncService", one POST method "Process" returning Result with AsyncType.None
    /// Testing: Method signature generation with synchronous (non-async) return type
    /// Assert: Method signature is "public Result Process()" (no async keyword, no Task wrapper)
    /// </summary>
    [Fact]
    public Task Generate_AsyncTypeNone_ProducesSync()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("ISyncService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("sync")
            .WithMethod(new MethodInfoBuilder()
                .WithName("Process")
                .Returning("Result")
                .UsingHttp(HttpMethod.Post)
                .IsAsyncMethod(AsyncType.None)
                .Build())
            .Build();

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    // ========================================
    // ROUTE TESTS
    // ========================================

    /// <summary>
    /// Setup: Interface "IUserService", one GET method "GetUserByIdAsync" with CustomRoute = "by-id"
    /// Testing: Route generation when a method has custom route specified
    /// Assert: HTTP call uses custom route "by-id" instead of method name (e.g., $"{BaseRoute}/by-id")
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

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface "IUserService", one GET method "GetUserAsync" with NO CustomRoute (null)
    /// Testing: Route generation when the method has a default route
    /// Assert: HTTP call uses method name as route (e.g., $"{BaseRoute}/GetUserAsync")
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

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    // ========================================
    // PARAMETER TESTS
    // ========================================

    /// <summary>
    /// Setup: Interface "IUserService", one POST method "CreateUserAsync" with 3 parameters (name: string, email: string, age: int)
    /// Testing: Request object creation for POST method with multiple parameters
    /// Assert: Creates an anonymous request object with properties Name, Email, Age (PascalCase), uses PostAsJsonAsync with a request object
    /// </summary>
    [Fact]
    public Task Generate_MultipleParameters_CreatesRequestObject()
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

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface "ISearchService", one GET method "SearchAsync" with 3 camelCase parameters (searchTerm: string, pageNumber: int, pageSize: int)
    /// Testing: Query string parameter name conversion to PascalCase for GET requests
    /// Assert: Query string keys are "SearchTerm", "PageNumber", "PageSize" (all PascalCase)
    /// </summary>
    [Fact]
    public Task Generate_QueryStringParameters_UsePascalCase()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("ISearchService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("search")
            .WithMethod(new MethodInfoBuilder()
                .WithName("SearchAsync")
                .Returning("List<Result>")
                .UsingHttp(HttpMethod.Get)
                .IsAsyncMethod(AsyncType.Task)
                .WithParameters(
                    new ParameterInfoBuilder().WithName("searchTerm").WithType("string").Build(),
                    new ParameterInfoBuilder().WithName("pageNumber").WithType("int").Build(),
                    new ParameterInfoBuilder().WithName("pageSize").WithType("int").Build())
                .Build())
            .Build();

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface "IUserService", one POST method "CreateUserAsync" with 3 camelCase parameters (firstName: string, lastName: string, emailAddress: string)
    /// Testing: Request object property name conversion to PascalCase for POST requests
    /// Assert: Request object properties are "FirstName", "LastName", "EmailAddress" (all PascalCase)
    /// </summary>
    [Fact]
    public Task Generate_RequestObjectParameters_UsePascalCase()
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

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface "ISearchService", one GET method "SearchAsync" with 2 nullable parameters (query: string?, maxResults: int?)
    /// Testing: Nullable parameter handling in query string generation
    /// Assert: Query string assignment uses conditional access operator (e.g., query?.ToString()) for nullable types
    /// </summary>
    [Fact]
    public Task Generate_NullableParameterTypes_HandleCorrectly()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("ISearchService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("search")
            .WithMethod(new MethodInfoBuilder()
                .WithName("SearchAsync")
                .Returning("List<Result>")
                .UsingHttp(HttpMethod.Get)
                .IsAsyncMethod(AsyncType.Task)
                .WithParameters(
                    new ParameterInfoBuilder().WithName("query").WithType("string?").Build(),
                    new ParameterInfoBuilder().WithName("maxResults").WithType("int?").Build())
                .Build())
            .Build();

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    // ========================================
    // DEFAULT PARAMETER VALUE TESTS
    // ========================================

    /// <summary>
    /// Setup: Interface "ITestService", one GET method "TestMethodAsync" with parameter (value: string = "hello")
    /// Testing: Default parameter value formatting for string type
    /// Assert: Method signature includes parameter with default value: string value = "hello" (quoted string literal)
    /// </summary>
    [Fact]
    public Task Generate_DefaultParameterValue_String_FormatsCorrectly()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("ITestService")
            .WithNamespace("Test")
            .WithRoutePrefix("test")
            .WithMethod(new MethodInfoBuilder()
                .WithName("TestMethodAsync")
                .Returning("string")
                .UsingHttp(HttpMethod.Get)
                .IsAsyncMethod(AsyncType.Task)
                .WithParameter(new ParameterInfoBuilder()
                    .WithName("value")
                    .WithType("string")
                    .WithDefault("\"hello\"")
                    .Build())
                .Build())
            .Build();

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface "ITestService", one GET method "TestMethodAsync" with parameter (isActive: bool = true)
    /// Testing: Default parameter value formatting for bool type
    /// Assert: Method signature includes parameter with default value: bool isActive = true (lowercase true)
    /// </summary>
    [Fact]
    public Task Generate_DefaultParameterValue_Bool_FormatsCorrectly()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("ITestService")
            .WithNamespace("Test")
            .WithRoutePrefix("test")
            .WithMethod(new MethodInfoBuilder()
                .WithName("TestMethodAsync")
                .Returning("string")
                .UsingHttp(HttpMethod.Get)
                .IsAsyncMethod(AsyncType.Task)
                .WithParameter(new ParameterInfoBuilder()
                    .WithName("isActive")
                    .WithType("bool")
                    .WithDefault("true")
                    .Build())
                .Build())
            .Build();

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface "ITestService", one GET method "TestMethodAsync" with parameter (value: string? = null)
    /// Testing: Default parameter value formatting for null value
    /// Assert: Method signature includes parameter with default value: string? value = null
    /// </summary>
    [Fact]
    public Task Generate_DefaultParameterValue_Null_FormatsCorrectly()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("ITestService")
            .WithNamespace("Test")
            .WithRoutePrefix("test")
            .WithMethod(new MethodInfoBuilder()
                .WithName("TestMethodAsync")
                .Returning("string")
                .UsingHttp(HttpMethod.Get)
                .IsAsyncMethod(AsyncType.Task)
                .WithParameter(new ParameterInfoBuilder()
                    .WithName("value")
                    .WithType("string?")
                    .WithDefault("null")
                    .Build())
                .Build())
            .Build();

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Interface "ITestService", one GET method "TestMethodAsync" with 2 parameters (count: int = 10, percentage: double = 0.5)
    /// Testing: Default parameter value formatting for numeric types (int, double)
    /// Assert: Method signature includes parameters with default values: int count = 10, double percentage = 0.5 (no quotes)
    /// </summary>
    [Fact]
    public Task Generate_DefaultParameterValue_Numeric_FormatsCorrectly()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("ITestService")
            .WithNamespace("Test")
            .WithRoutePrefix("test")
            .WithMethod(new MethodInfoBuilder()
                .WithName("TestMethodAsync")
                .Returning("int")
                .UsingHttp(HttpMethod.Get)
                .IsAsyncMethod(AsyncType.Task)
                .WithParameters(
                    new ParameterInfoBuilder()
                        .WithName("count")
                        .WithType("int")
                        .WithDefault("10")
                        .Build(),
                    new ParameterInfoBuilder()
                        .WithName("percentage")
                        .WithType("double")
                        .WithDefault("0.5")
                        .Build())
                .Build())
            .Build();

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }
}
