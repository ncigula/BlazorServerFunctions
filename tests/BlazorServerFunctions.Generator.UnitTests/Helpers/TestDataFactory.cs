namespace BlazorServerFunctions.Generator.UnitTests.Helpers;

/// <summary>
/// Factory for creating common test data scenarios
/// </summary>
internal static class TestDataFactory
{
    // ========================================
    // COMMON INTERFACES
    // ========================================
    
    /// <summary>
    /// Creates a basic interface with a single GET method
    /// </summary>
    internal static InterfaceInfo BasicGetInterface() =>
        new InterfaceInfoBuilder()
            .WithName("IUserService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("users")
            .WithMethod(Method.BasicGet())
            .Build();

    /// <summary>
    /// Creates a CRUD interface with all HTTP methods
    /// </summary>
    internal static InterfaceInfo CrudInterface() =>
        new InterfaceInfoBuilder()
            .WithName("IUserService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("users")
            .WithMethods(
                Method.GetList(),
                Method.GetById(),
                Method.Create(),
                Method.Update(),
                Method.Delete())
            .Build();

    /// <summary>
    /// Creates an empty interface with no methods
    /// </summary>
    internal static InterfaceInfo EmptyInterface() =>
        new InterfaceInfoBuilder()
            .WithName("IEmptyService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("empty")
            .Build();

    // ========================================
    // COMMON METHODS
    // ========================================
    
    internal static class Method
    {
        /// <summary>
        /// Basic GET method: Task&lt;User&gt; GetUserAsync(int id)
        /// </summary>
        internal static MethodInfo BasicGet() =>
            new MethodInfoBuilder()
                .WithName("GetUserAsync")
                .Returning("UserDto")
                .UsingHttp(HttpMethod.Get)
                .IsAsyncMethod(AsyncType.Task)
                .WithParameter(Param.Id())
                .Build();

        /// <summary>
        /// GET list: Task&lt;List&lt;User&gt;&gt; GetUsersAsync()
        /// </summary>
        internal static MethodInfo GetList() =>
            new MethodInfoBuilder()
                .WithName("GetUsersAsync")
                .Returning("List<User>")
                .UsingHttp(HttpMethod.Get)
                .IsAsyncMethod(AsyncType.Task)
                .Build();

        /// <summary>
        /// GET by ID: Task&lt;User&gt; GetUserAsync(int id)
        /// </summary>
        internal static MethodInfo GetById() =>
            new MethodInfoBuilder()
                .WithName("GetUserAsync")
                .Returning("User")
                .UsingHttp(HttpMethod.Get)
                .IsAsyncMethod(AsyncType.Task)
                .WithParameter(Param.Id())
                .Build();

        /// <summary>
        /// POST create: Task&lt;User&gt; CreateUserAsync(string name, string email)
        /// </summary>
        internal static MethodInfo Create() =>
            new MethodInfoBuilder()
                .WithName("CreateUserAsync")
                .Returning("User")
                .UsingHttp(HttpMethod.Post)
                .IsAsyncMethod(AsyncType.Task)
                .WithParameters(Param.Name(), Param.Email())
                .Build();

        /// <summary>
        /// PUT update: Task&lt;User&gt; UpdateUserAsync(int id, string name, string email)
        /// </summary>
        internal static MethodInfo Update() =>
            new MethodInfoBuilder()
                .WithName("UpdateUserAsync")
                .Returning("User")
                .UsingHttp(HttpMethod.Put)
                .IsAsyncMethod(AsyncType.Task)
                .WithParameters(Param.Id(), Param.Name(), Param.Email())
                .Build();

        /// <summary>
        /// DELETE: Task DeleteUserAsync(int id)
        /// </summary>
        internal static MethodInfo Delete() =>
            new MethodInfoBuilder()
                .WithName("DeleteUserAsync")
                .Returning("void")
                .UsingHttp(HttpMethod.Delete)
                .IsAsyncMethod(AsyncType.Task)
                .WithParameter(Param.Id())
                .Build();

        /// <summary>
        /// Void method: Task DoSomethingAsync()
        /// </summary>
        internal static MethodInfo VoidMethod() =>
            new MethodInfoBuilder()
                .WithName("DoSomethingAsync")
                .Returning("void")
                .UsingHttp(HttpMethod.Post)
                .IsAsyncMethod(AsyncType.Task)
                .Build();

        /// <summary>
        /// Method with no parameters: Task&lt;string&gt; GetMessageAsync()
        /// </summary>
        internal static MethodInfo WithNoParameters() =>
            new MethodInfoBuilder()
                .WithName("GetMessageAsync")
                .Returning("string")
                .UsingHttp(HttpMethod.Get)
                .IsAsyncMethod(AsyncType.Task)
                .Build();

        /// <summary>
        /// Basic POST method: Task&lt;User&gt; CreateUserAsync(string name, string email)
        /// </summary>
        internal static MethodInfo BasicPost() =>
            new MethodInfoBuilder()
                .WithName("CreateUserAsync")
                .Returning("User")
                .UsingHttp(HttpMethod.Post)
                .IsAsyncMethod(AsyncType.Task)
                .WithParameters(Param.Name(), Param.Email())
                .Build();
    }

    // ========================================
    // COMMON PARAMETERS
    // ========================================
    
    internal static class Param
    {
        /// <summary>
        /// int id
        /// </summary>
        internal static ParameterInfo Id() =>
            new ParameterInfoBuilder()
                .WithName("id")
                .WithType("int")
                .Build();

        /// <summary>
        /// string name
        /// </summary>
        internal static ParameterInfo Name() =>
            new ParameterInfoBuilder()
                .WithName("name")
                .WithType("string")
                .Build();

        /// <summary>
        /// string email
        /// </summary>
        internal static ParameterInfo Email() =>
            new ParameterInfoBuilder()
                .WithName("email")
                .WithType("string")
                .Build();

        /// <summary>
        /// string? optional
        /// </summary>
        internal static ParameterInfo OptionalString() =>
            new ParameterInfoBuilder()
                .WithName("optional")
                .WithType("string?")
                .Build();

        /// <summary>
        /// Create a parameter with a default value
        /// </summary>
        internal static ParameterInfo WithDefault(string name, string type, object? defaultValue) =>
            new ParameterInfoBuilder()
                .WithName(name)
                .WithType(type)
                .WithDefault(defaultValue?.ToString() ?? "null")
                .Build();
    }
}