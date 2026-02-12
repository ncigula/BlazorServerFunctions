namespace BlazorServerFunctions.Generator.UnitTests;

/// <summary>
/// Unit tests for ClientRegistrationGenerator - testing string output generation
/// Tests registration generation for various interface configurations
/// </summary>
public class ClientRegistrationGeneratorTests
{
    // ========================================
    // EDGE CASE TESTS
    // ========================================

    /// <summary>
    /// Setup: Empty list of interfaces (no interfaces provided)
    /// Testing: Generator behavior with no interfaces to register
    /// Assert: Returns empty string (no file content generated)
    /// </summary>
    [Fact]
    public Task Generate_EmptyInterfaceList_ReturnsEmptyString()
    {
        var generated = ClientRegistrationGenerator.Generate([]);

        return Verify(generated);
    }

    // ========================================
    // SINGLE INTERFACE TESTS
    // ========================================

    /// <summary>
    /// Setup: Single interface "IUserService" in namespace "MyApp.Services" with route prefix "users"
    /// Testing: Basic registration generation for single interface
    /// Assert: Generates ServerFunctionClientsRegistration static class with AddServerFunctionClients extension method, contains services. AddHttpClient IUserService, UserServiceClient registration
    /// </summary>
    [Fact]
    public Task Generate_SingleInterface_ProducesCorrectRegistration()
    {
        var interfaces = new List<InterfaceInfo>
        {
            TestDataFactory.BasicGetInterface()
        };

        var generated = ClientRegistrationGenerator.Generate(interfaces);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Single interface "ICustomService" in deeply nested namespace "Custom.Deeply.Nested.Namespace.Services"
    /// Testing: Namespace usage from the first interface in the list
    /// Assert: Generated class uses "Custom.Deeply.Nested.Namespace.Services" namespace, contains a using statement for Microsoft.Extensions.DependencyInjection
    /// </summary>
    [Fact]
    public Task Generate_SingleInterface_UsesCorrectNamespace()
    {
        var interfaces = new List<InterfaceInfo>
        {
            new InterfaceInfoBuilder()
                .WithName("ICustomService")
                .WithNamespace("Custom.Deeply.Nested.Namespace.Services")
                .WithRoutePrefix("custom")
                .WithMethod(TestDataFactory.Method.BasicGet())
                .Build()
        };

        var generated = ClientRegistrationGenerator.Generate(interfaces);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Single interface named "UserService" (without 'I' prefix) in namespace "MyApp.Services"
    /// Testing: Client class name generation when interface name doesn't start with 'I'
    /// Assert: Generates AddHttpClient&lt;UserService, UserServiceClient&gt;() (correctly adds "Client" suffix without trying to remove the non-existent 'I' prefix)
    /// </summary>
    [Fact]
    public Task Generate_InterfaceWithoutIPrefix_RemovesIPrefixCorrectly()
    {
        var interfaces = new List<InterfaceInfo>
        {
            new InterfaceInfoBuilder()
                .WithName("UserService") // No 'I' prefix
                .WithNamespace("MyApp.Services")
                .WithRoutePrefix("users")
                .WithMethod(TestDataFactory.Method.BasicGet())
                .Build()
        };

        var generated = ClientRegistrationGenerator.Generate(interfaces);

        return Verify(generated);
    }

    // ========================================
    // MULTIPLE INTERFACE TESTS
    // ========================================

    /// <summary>
    /// Setup: Three interfaces (IUserService, IProductService, IOrderService) all in namespace "MyApp.Services"
    /// Testing: Multiple interface registration in single registration method
    /// Assert: Generates three AddHttpClient calls: AddHttpClient&lt;IUserService, UserServiceClient&gt;(), AddHttpClient&lt;IProductService, ProductServiceClient&gt;(), AddHttpClient&lt;IOrderService, OrderServiceClient&gt;()
    /// </summary>
    [Fact]
    public Task Generate_MultipleInterfaces_ProducesCorrectRegistrations()
    {
        var interfaces = new List<InterfaceInfo>
        {
            new InterfaceInfoBuilder()
                .WithName("IUserService")
                .WithNamespace("MyApp.Services")
                .WithRoutePrefix("users")
                .WithMethod(TestDataFactory.Method.BasicGet())
                .Build(),
            new InterfaceInfoBuilder()
                .WithName("IProductService")
                .WithNamespace("MyApp.Services")
                .WithRoutePrefix("products")
                .WithMethod(TestDataFactory.Method.GetList())
                .Build(),
            new InterfaceInfoBuilder()
                .WithName("IOrderService")
                .WithNamespace("MyApp.Services")
                .WithRoutePrefix("orders")
                .WithMethod(TestDataFactory.Method.BasicPost())
                .Build()
        };

        var generated = ClientRegistrationGenerator.Generate(interfaces);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Two interfaces with different names (IAlphaService, IBetaService) in namespace "MyApp.Services"
    /// Testing: Multiple interface registration with varied interface names
    /// Assert: Generates two distinct AddHttpClient calls with correct interface-to-client mappings: IAlphaService→AlphaServiceClient, IBetaService→BetaServiceClient
    /// </summary>
    [Fact]
    public Task Generate_MultipleInterfaces_WithDifferentNames()
    {
        var interfaces = new List<InterfaceInfo>
        {
            new InterfaceInfoBuilder()
                .WithName("IAlphaService")
                .WithNamespace("MyApp.Services")
                .WithRoutePrefix("alpha")
                .WithMethod(TestDataFactory.Method.BasicGet())
                .Build(),
            new InterfaceInfoBuilder()
                .WithName("IBetaService")
                .WithNamespace("MyApp.Services")
                .WithRoutePrefix("beta")
                .WithMethod(TestDataFactory.Method.BasicPost())
                .Build()
        };

        var generated = ClientRegistrationGenerator.Generate(interfaces);

        return Verify(generated);
    }

    // ========================================
    // STRUCTURE TESTS
    // ========================================

    /// <summary>
    /// Setup: Single interface "ITestService" in namespace "Test.Services"
    /// Testing: Static class naming and method signature
    /// Assert: Generates class named "ServerFunctionClientsRegistration", method named "AddServerFunctionClients", method returns IServiceCollection, method extends IServiceCollection
    /// </summary>
    [Fact]
    public Task Generate_VerifyStaticClassName()
    {
        var interfaces = new List<InterfaceInfo>
        {
            new InterfaceInfoBuilder()
                .WithName("ITestService")
                .WithNamespace("Test.Services")
                .WithRoutePrefix("test")
                .WithMethod(TestDataFactory.Method.BasicGet())
                .Build()
        };

        var generated = ClientRegistrationGenerator.Generate(interfaces);

        return Verify(generated);
    }
}
