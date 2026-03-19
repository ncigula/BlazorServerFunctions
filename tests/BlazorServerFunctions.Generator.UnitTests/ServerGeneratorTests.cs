namespace BlazorServerFunctions.Generator.UnitTests;

/// <summary>
/// Unit tests for server-side code generation (endpoints + registration)
/// Tests the full pipeline: string interface code → parser → generators → output
/// Each test verifies both the server endpoint mappings and registration files
/// </summary>
public class ServerGeneratorTests
{
    [Fact]
    public Task Generate_RouteParam_Delete_NoDto_DirectLambdaParam()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/items")]
                     public interface IItemService
                     {
                         [ServerFunction(HttpMethod = "DELETE", Route = "{id}")]
                         Task DeleteAsync(int id);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_RouteParam_Get_MixedWithQueryString()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/users")]
                     public interface IUserPostService
                     {
                         [ServerFunction(HttpMethod = "GET", Route = "users/{id}/posts")]
                         Task<string> GetUserPostsAsync(int id, int page);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_RouteParam_Put_MixedWithBody()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/items")]
                     public interface IItemService
                     {
                         [ServerFunction(HttpMethod = "PUT", Route = "{id}")]
                         Task<string> UpdateAsync(int id, string value);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_RouteParam_WithConstraint_PreservedOnServer()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/users")]
                     public interface IUserService
                     {
                         [ServerFunction(HttpMethod = "GET", Route = "users/{id:int}")]
                         Task<string> GetUserAsync(int id);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_RouteParam_KebabCaseConfig_DoesNotCorruptExplicitRoute()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp;

                     public class KebabConfig : ServerFunctionConfiguration
                     {
                         public KebabConfig() { RouteNaming = RouteNaming.KebabCase; }
                     }

                     [ServerFunctionCollection(RoutePrefix = "/users", Configuration = typeof(KebabConfig))]
                     public interface IUserService
                     {
                         [ServerFunction(HttpMethod = "GET", Route = "users/{id}")]
                         Task<string> GetUserAsync(int id);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public void BSF017_RouteParamHasNoMatchingMethodParam_EmitsError()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/items")]
                     public interface ITestService
                     {
                         [ServerFunction(HttpMethod = "GET", Route = "items/{missing}")]
                         System.Threading.Tasks.Task<string> GetAsync(int id);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        result.AssertDiagnostic("BSF017");
    }


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

    // ── §3.2 IAsyncEnumerable<T> streaming ────────────────────────────────────

    [Fact]
    public Task Generate_Streaming_Get_NoParams_DirectReturn()
    {
        var source = """
                     using System.Collections.Generic;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/weather")]
                     public interface IWeatherService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         IAsyncEnumerable<string> StreamForecastsAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_Streaming_Get_WithRouteParam_DirectReturn()
    {
        var source = """
                     using System.Collections.Generic;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/sensors")]
                     public interface ISensorService
                     {
                         [ServerFunction(HttpMethod = "GET", Route = "{deviceId}/readings")]
                         IAsyncEnumerable<string> StreamReadingsAsync(int deviceId);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_Streaming_WithCancellationToken_DirectReturn()
    {
        var source = """
                     using System.Collections.Generic;
                     using System.Threading;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/events")]
                     public interface IEventService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         IAsyncEnumerable<string> StreamEventsAsync(CancellationToken ct = default);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    // ── §3.4 OpenAPI Metadata ──────────────────────────────────────────────────

    private static readonly PortableExecutableReference OpenApiReference =
        MetadataReference.CreateFromFile(
            typeof(Microsoft.AspNetCore.Builder.OpenApiEndpointConventionBuilderExtensions).Assembly.Location);

    [Fact]
    public Task Generate_OpenApiMetadata_WithOpenApiPackage_EmitsWithOpenApi()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/weather")]
                     public interface IWeatherService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<WeatherForecast[]> GetForecastsAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator(),
            OpenApiReference);

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_OpenApiMetadata_VoidReturn_EmitsProducesWithoutTypeArg()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/notifications")]
                     public interface INotificationService
                     {
                         [ServerFunction(HttpMethod = "POST")]
                         Task SendAsync(string message);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator(),
            OpenApiReference);

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_OpenApiMetadata_Streaming_SkipsProducesAndProblem()
    {
        var source = """
                     using System.Collections.Generic;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/weather")]
                     public interface IWeatherService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         IAsyncEnumerable<WeatherForecast> StreamForecastsAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator(),
            OpenApiReference);

        return result.VerifyNoDiagnostics();
    }
}
