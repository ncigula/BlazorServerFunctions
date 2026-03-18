namespace BlazorServerFunctions.Generator.UnitTests;

/// <summary>
/// Unit tests for client-side code generation (proxy + registration)
/// Tests the full pipeline: string interface code → parser → generators → output
/// Each test verifies both the client proxy and registration files
/// </summary>
public class ClientGeneratorTests
{
    [Fact]
    public Task Generate_RouteParam_Get_InterpolatesIntoUrl_ExcludesFromQueryString()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/users")]
                     public interface IUserService
                     {
                         [ServerFunction(HttpMethod = "GET", Route = "users/{id}")]
                         Task<string> GetUserAsync(int id);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_RouteParam_Post_ExcludesFromBody()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/users")]
                     public interface IUserService
                     {
                         [ServerFunction(HttpMethod = "POST", Route = "users/{id}/reset")]
                         Task<string> ResetUserAsync(int id, string reason);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_RouteParam_ConstraintStrippedForUrl()
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

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }


    [Fact]
    public Task Generate_BasicInterface_ProducesCorrectCode()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace BlazorServerFunctions.Sample.Shared;

                     [ServerFunctionCollection]
                     public interface IWeatherService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<WeatherForecastDto[]> GetWeatherForecastsAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_MultipleInterfaces_ProducesCorrectCode()
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

                     [ServerFunctionCollection(RoutePrefix = "/products")]
                     public interface IProductService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<Product[]> GetAllProductsAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_InterfaceWithoutIPrefix_HandlesCorrectly()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/users")]
                     public interface UserService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<User> GetUserAsync(int id);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }


    [Fact]
    public Task Generate_NullRoutePrefix_UsesDefaultRoute()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection]
                     public interface IServiceWithoutPrefix
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<string> GetAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(
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

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_GetMethod_WithNoParameters_UsesSimpleGetAsync()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/messages")]
                     public interface IMessageService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<string> GetMessageAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_GetMethod_WithParameters_BuildsQueryString()
    {
        var source = """
                     using System.Collections.Generic;
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/search")]
                     public interface ISearchService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<List<Result>> SearchAsync(string query, int pageSize);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_PostMethod_WithNoParameters_PostsNull()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/actions")]
                     public interface IActionService
                     {
                         [ServerFunction(HttpMethod = "POST")]
                         Task TriggerActionAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_PostMethod_WithParameters_PostsJsonObject()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/users")]
                     public interface IUserService
                     {
                         [ServerFunction(HttpMethod = "POST")]
                         Task<User> CreateUserAsync(string name, string email);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_PutMethod_WithParameters_UsesHttpRequestMessage()
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

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_PutMethod_WithNoParameters_UsesHttpRequestMessage()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/resources")]
                     public interface IResourceService
                     {
                         [ServerFunction(HttpMethod = "PUT")]
                         Task RefreshAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_PatchMethod_WithParameters_UsesHttpRequestMessage()
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

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_DeleteMethod_WithParameters_UsesHttpRequestMessage()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/users")]
                     public interface IUserService
                     {
                         [ServerFunction(HttpMethod = "DELETE")]
                         Task DeleteUserAsync(int id);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_DeleteMethod_WithNoParameters_UsesHttpRequestMessage()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/cache")]
                     public interface ICacheService
                     {
                         [ServerFunction(HttpMethod = "DELETE")]
                         Task ClearAllAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(
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

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_VoidReturnType_NoDeserialization()
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

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_PrimitiveReturnType_DeserializesCorrectly()
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

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_ListReturnType_DeserializesCorrectly()
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

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_ArrayReturnType_DeserializesCorrectly()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/users")]
                     public interface IUserService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<User[]> GetAllUsersAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_DictionaryReturnType_DeserializesCorrectly()
    {
        var source = """
                     using System.Collections.Generic;
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/data")]
                     public interface IDataService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<Dictionary<string, List<int>>> GetMappingAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_NullableReferenceReturnType_DeserializesCorrectly()
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

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_NullableValueReturnType_DeserializesCorrectly()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/counter")]
                     public interface ICounterService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<int?> GetCountAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_TaskReturnType_ProducesAsyncMethod()
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

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_ValueTaskReturnType_ProducesAsyncValueTask()
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

        var result = GeneratorTestHelper.RunGeneratorAsClient(
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

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_DefaultRoute_UsesMethodName()
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

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_MultipleParameters_CreatesRequestObject()
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

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_CamelCaseParameters_ConvertToPascalCase()
    {
        var source = """
                     using System.Collections.Generic;
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/search")]
                     public interface ISearchService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<List<Result>> SearchAsync(string searchTerm, int pageNumber, int pageSize);

                         [ServerFunction(HttpMethod = "POST")]
                         Task<User> CreateUserAsync(string firstName, string lastName, string emailAddress);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_NullableParameters_HandleCorrectly()
    {
        var source = """
                     using System.Collections.Generic;
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/search")]
                     public interface ISearchService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<List<Result>> SearchAsync(string? query, int? maxResults);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(
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

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_DefaultParameterValues_AllTypes()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp.Services;

                     [ServerFunctionCollection(RoutePrefix = "/test")]
                     public interface ITestService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<string> TestMethodAsync(
                             string value = "hello",
                             bool isActive = true,
                             string? nullable = null,
                             int count = 10,
                             double percentage = 0.5);
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_CancellationToken_ExcludedFromQueryString_ForwardedToHttpClient()
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

        var result = GeneratorTestHelper.RunGeneratorAsClient(
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

                     namespace MyApp.Services.Products
                     {
                         [ServerFunctionCollection(RoutePrefix = "/products")]
                         public interface IProductService
                         {
                             [ServerFunction(HttpMethod = "GET")]
                             Task<Product> GetProductAsync(int id);
                         }
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }
}
