namespace BlazorServerFunctions.Generator.UnitTests;

/// <summary>
/// Unit tests for ServerRegistrationGenerator - testing string output generation
/// Tests endpoint registration generation for various interface configurations
/// </summary>
public class ServerRegistrationGeneratorTests
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
        var generated = ServerRegistrationGenerator.Generate([]);

        return Verify(generated);
    }

    // ========================================
    // SINGLE INTERFACE TESTS
    // ========================================

    /// <summary>
    /// Setup: Single interface "IUserService" in namespace "MyApp.Services" with route prefix "users"
    /// Testing: Basic endpoint registration generation for single interface
    /// Assert: Generates ServerFunctionEndpointsRegistration static class with MapServerFunctionEndpoints extension method, contains endpoints.MapIUserServiceEndpoints() call
    /// </summary>
    [Fact]
    public Task Generate_SingleInterface_ProducesCorrectRegistration()
    {
        var interfaces = new List<InterfaceInfo>
        {
            TestDataFactory.BasicGetInterface()
        };

        var generated = ServerRegistrationGenerator.Generate(interfaces);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Single interface "ICustomService" in deeply nested namespace "Custom.Deeply.Nested.Namespace.Services"
    /// Testing: Namespace usage from the first interface in the list
    /// Assert: Generated class uses "Custom.Deeply.Nested.Namespace.Services" namespace, contains using statements for Microsoft.AspNetCore.Builder and Microsoft.AspNetCore.Routing
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

        var generated = ServerRegistrationGenerator.Generate(interfaces);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Single interface "ISpecialService" in namespace "MyApp.Services"
    /// Testing: Map method naming convention - should be Map{InterfaceName}Endpoints
    /// Assert: Generates call to endpoints.MapISpecialServiceEndpoints() matching interface name
    /// </summary>
    [Fact]
    public Task Generate_SingleInterface_UsesCorrectMapMethod()
    {
        var interfaces = new List<InterfaceInfo>
        {
            new InterfaceInfoBuilder()
                .WithName("ISpecialService")
                .WithNamespace("MyApp.Services")
                .WithRoutePrefix("special")
                .WithMethod(TestDataFactory.Method.BasicGet())
                .Build()
        };

        var generated = ServerRegistrationGenerator.Generate(interfaces);

        return Verify(generated);
    }

    // ========================================
    // MULTIPLE INTERFACE TESTS
    // ========================================

    /// <summary>
    /// Setup: Three interfaces (IUserService, IProductService, IOrderService) all in namespace "MyApp.Services"
    /// Testing: Multiple endpoint registrations in single registration method
    /// Assert: Generates three Map calls: endpoints.MapIUserServiceEndpoints(), endpoints.MapIProductServiceEndpoints(), endpoints.MapIOrderServiceEndpoints()
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

        var generated = ServerRegistrationGenerator.Generate(interfaces);

        return Verify(generated);
    }

    /// <summary>
    /// Setup: Two interfaces with different names (IAlphaService, IBetaService) in namespace "MyApp.Services"
    /// Testing: Multiple endpoint registrations with varied interface names
    /// Assert: Generates two distinct Map calls with correct interface name mappings: endpoints.MapIAlphaServiceEndpoints(), endpoints.MapIBetaServiceEndpoints()
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

        var generated = ServerRegistrationGenerator.Generate(interfaces);

        return Verify(generated);
    }

    // ========================================
    // STRUCTURE TESTS
    // ========================================

    /// <summary>
    /// Setup: Single interface "ITestService" in namespace "Test.Services"
    /// Testing: Static class naming and method signature
    /// Assert: Generates class named "ServerFunctionEndpointsRegistration", method named "MapServerFunctionEndpoints", method returns IEndpointRouteBuilder, method extends IEndpointRouteBuilder
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

        var generated = ServerRegistrationGenerator.Generate(interfaces);

        return Verify(generated);
    }
}
