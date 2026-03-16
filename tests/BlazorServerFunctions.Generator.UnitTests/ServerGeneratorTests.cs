namespace BlazorServerFunctions.Generator.UnitTests;

/// <summary>
/// Unit tests for server-side code generation (endpoints + registration)
/// Tests the full pipeline: string interface code → parser → generators → output
/// Each test verifies both the server endpoint mappings and registration files
/// </summary>
public class ServerGeneratorTests
{
    [Fact]
    public Task Generate_BasicInterface_ProducesCorrectCode()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/users")]
                     public interface IUserService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<User> GetUserAsync(int id);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_MultipleInterfaces_ProducesCorrectCode()
    {
        var source = """
                     using System.Collections.Generic;
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/users")]
                     public interface IUserService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<User> GetUserAsync(int id);
                     }

                     [ServerFunctionCollection(RoutePrefix = "/products")]
                     public interface IProductService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<List<Product>> GetAllProductsAsync();
                     }

                     [ServerFunctionCollection(RoutePrefix = "/orders")]
                     public interface IOrderService
                     {
                         [ServerFunction(HttpMethod = "POST")]
                         Task<Order> CreateOrderAsync(int userId, int productId);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_RoutePrefixWithSlashes_PreservesSlashes()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/api/v2/nested/service")]
                     public interface INestedService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<string> GetAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_GetEndpoint_MapsCorrectly()
    {
        var source = """
                     using System.Collections.Generic;
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/users")]
                     public interface IUserService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<List<User>> GetAllAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_PostEndpoint_MapsCorrectly()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/users")]
                     public interface IUserService
                     {
                         [ServerFunction(HttpMethod = "POST")]
                         Task<User> CreateAsync(string name, string email);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_PutEndpoint_MapsCorrectly()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/users")]
                     public interface IUserService
                     {
                         [ServerFunction(HttpMethod = "PUT")]
                         Task<User> UpdateUserAsync(int id, string name);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_PatchEndpoint_MapsCorrectly()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/users")]
                     public interface IUserService
                     {
                         [ServerFunction(HttpMethod = "PATCH")]
                         Task<User> PatchUserAsync(int id, string name);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_DeleteEndpoint_MapsCorrectly()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/users")]
                     public interface IUserService
                     {
                         [ServerFunction(HttpMethod = "DELETE")]
                         Task DeleteAsync(int id);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_MultipleMethods_AllHttpVerbs_ProducesCorrectCode()
    {
        var source = """
                     using System.Collections.Generic;
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/users")]
                     public interface IUserService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<List<User>> GetAllAsync();

                         [ServerFunction(HttpMethod = "GET")]
                         Task<User> GetByIdAsync(int id);

                         [ServerFunction(HttpMethod = "POST")]
                         Task<User> CreateAsync(string name, string email);

                         [ServerFunction(HttpMethod = "PUT")]
                         Task<User> UpdateAsync(int id, string name);

                         [ServerFunction(HttpMethod = "DELETE")]
                         Task DeleteAsync(int id);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_MethodWithNoParameters_NoRequestDto()
    {
        var source = """
                     using System.Collections.Generic;
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/messages")]
                     public interface IMessageService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<List<Message>> GetAllMessagesAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_MethodWithParameters_CreatesRequestDto()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/users")]
                     public interface IUserService
                     {
                         [ServerFunction(HttpMethod = "POST")]
                         Task<User> CreateUserAsync(string name, string email, int age);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_CamelCaseParameters_ConvertToPascalCaseInDto()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/users")]
                     public interface IUserService
                     {
                         [ServerFunction(HttpMethod = "POST")]
                         Task<User> CreateUserAsync(string firstName, string lastName, string emailAddress);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_ServiceCall_PassesParametersFromDto()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/calculator")]
                     public interface ICalculatorService
                     {
                         [ServerFunction(HttpMethod = "POST")]
                         Task<int> AddAsync(int firstNumber, int secondNumber);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_MixOfParameterizedAndParameterlessMethods_SelectiveDtos()
    {
        var source = """
                     using System.Collections.Generic;
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/test")]
                     public interface ITestService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<List<Message>> GetAllMessagesAsync();

                         [ServerFunction(HttpMethod = "POST")]
                         Task<User> CreateAsync(int id, string name);

                         [ServerFunction(HttpMethod = "POST")]
                         Task AnotherMethodAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_ComplexParameterTypes_HandleCorrectly()
    {
        var source = """
                     using System;
                     using System.Collections.Generic;
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/data")]
                     public interface IDataService
                     {
                         [ServerFunction(HttpMethod = "POST")]
                         Task<Result> ProcessAsync(
                             Guid id,
                             DateTime timestamp,
                             List<int> numbers,
                             Dictionary<string, string> metadata);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_VoidReturnType_ReturnsOkOnly()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/notifications")]
                     public interface INotificationService
                     {
                         [ServerFunction(HttpMethod = "POST")]
                         Task SendNotificationAsync(string message);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_PrimitiveReturnType_ReturnsOkWithResult()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/counter")]
                     public interface ICounterService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<int> GetCountAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_ComplexReturnType_ReturnsOkWithResult()
    {
        var source = """
                     using System.Collections.Generic;
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/users")]
                     public interface IUserService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<List<User>> GetAllUsersAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_NullableReturnType_ReturnsOkWithResult()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/users")]
                     public interface IUserService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<User?> FindUserAsync(int id);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_EndpointNames_AreUnique()
    {
        var source = """
                     using System.Collections.Generic;
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/users")]
                     public interface IUserService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<List<User>> GetAllAsync();

                         [ServerFunction(HttpMethod = "GET")]
                         Task<User> GetByIdAsync(int id);

                         [ServerFunction(HttpMethod = "POST")]
                         Task<User> CreateAsync(string name, string email);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_EndpointNames_UseInterfaceAndMethodName()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/special")]
                     public interface ISpecialService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<Result> SpecialMethodAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_CustomRoute_UsesCustomRouteName()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/users")]
                     public interface IUserService
                     {
                         [ServerFunction(HttpMethod = "GET", Route = "by-id")]
                         Task<User> GetUserByIdAsync(int id);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_TaskReturnType_ProducesAsyncEndpoint()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/tasks")]
                     public interface ITaskService
                     {
                         [ServerFunction(HttpMethod = "POST")]
                         Task<Result> ProcessAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_ValueTaskReturnType_ProducesAsyncEndpoint()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/valuetasks")]
                     public interface IValueTaskService
                     {
                         [ServerFunction(HttpMethod = "POST")]
                         ValueTask<Result> ProcessAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_InterfaceLevelAuthorization_AppliesRequireAuthorizationToGroup()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/admin", RequireAuthorization = true)]
                     public interface IAdminService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<string> GetSecretAsync();

                         [ServerFunction(HttpMethod = "POST")]
                         Task DoActionAsync(string command);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_MethodLevelAuthorization_AppliesRequireAuthorizationToEndpoint()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/users")]
                     public interface IUserService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<User> GetUserAsync(int id);

                         [ServerFunction(HttpMethod = "DELETE", RequireAuthorization = true)]
                         Task DeleteUserAsync(int id);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_CancellationToken_ExcludedFromDto_PassedToService()
    {
        var source = """
                     using System.Threading;
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/users")]
                     public interface IUserService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<User> GetUserAsync(int id, CancellationToken cancellationToken);

                         [ServerFunction(HttpMethod = "POST")]
                         Task<User> CreateUserAsync(string name, CancellationToken cancellationToken);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_MultipleInterfaces_DifferentNamespaces_UsesCommonNamespacePrefix()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services.Users
                     {
                         [ServerFunctionCollection(RoutePrefix = "/users")]
                         public interface IUserService
                         {
                             [ServerFunction(HttpMethod = "GET")]
                             Task<User> GetUserAsync(int id);
                         }
                     }

                     namespace MyApp.Services.Orders
                     {
                         [ServerFunctionCollection(RoutePrefix = "/orders")]
                         public interface IOrderService
                         {
                             [ServerFunction(HttpMethod = "POST")]
                             Task<Order> CreateOrderAsync(int userId);
                         }
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }
}
