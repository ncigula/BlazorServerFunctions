namespace BlazorServerFunctions.Generator.UnitTests;

/// <summary>
/// Tests for the §2 Configuration system.
/// Each test exercises the full pipeline: config class + interface → parser → generators → output.
/// </summary>
public class ConfigurationTests
{
    // ─── Defaults (backward-compatibility) ───────────────────────────────────

    [Fact]
    public Task DefaultConfig_NoConfigurationProperty_ProducesDefaultRoutes()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp;

                     [ServerFunctionCollection]
                     public interface IUserService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<string> GetUserAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(source, new ServerFunctionCollectionGenerator());
        return result.VerifyNoDiagnostics();
    }

    // ─── BaseRoute ────────────────────────────────────────────────────────────

    [Fact]
    public Task BaseRoute_Override_ChangesGeneratedRoute()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp;

                     public class MyApiConfig : ServerFunctionConfiguration
                     {
                         public MyApiConfig() { BaseRoute = "api/v2"; }
                     }

                     [ServerFunctionCollection(Configuration = typeof(MyApiConfig))]
                     public interface IUserService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<string> GetUserAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(source, new ServerFunctionCollectionGenerator());
        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task BaseRoute_WithLeadingSlash_IsNormalised()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp;

                     public class SlashConfig : ServerFunctionConfiguration
                     {
                         public SlashConfig() { BaseRoute = "/api/v3"; }
                     }

                     [ServerFunctionCollection(Configuration = typeof(SlashConfig))]
                     public interface IOrderService
                     {
                         [ServerFunction(HttpMethod = "POST")]
                         Task CreateOrderAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(source, new ServerFunctionCollectionGenerator());
        return result.VerifyNoDiagnostics();
    }

    // ─── RouteNaming ──────────────────────────────────────────────────────────

    [Fact]
    public Task RouteNaming_KebabCase_TransformsMethodName()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp;

                     public class KebabConfig : ServerFunctionConfiguration
                     {
                         public KebabConfig() { RouteNaming = RouteNaming.KebabCase; }
                     }

                     [ServerFunctionCollection(Configuration = typeof(KebabConfig))]
                     public interface IUserService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<string> GetUserByIdAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(source, new ServerFunctionCollectionGenerator());
        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task RouteNaming_CamelCase_TransformsMethodName()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp;

                     public class CamelConfig : ServerFunctionConfiguration
                     {
                         public CamelConfig() { RouteNaming = RouteNaming.CamelCase; }
                     }

                     [ServerFunctionCollection(Configuration = typeof(CamelConfig))]
                     public interface IProductService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<string> GetAllProductsAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(source, new ServerFunctionCollectionGenerator());
        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task RouteNaming_SnakeCase_TransformsMethodName()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp;

                     public class SnakeConfig : ServerFunctionConfiguration
                     {
                         public SnakeConfig() { RouteNaming = RouteNaming.SnakeCase; }
                     }

                     [ServerFunctionCollection(Configuration = typeof(SnakeConfig))]
                     public interface IOrderService
                     {
                         [ServerFunction(HttpMethod = "POST")]
                         Task CreateNewOrderAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(source, new ServerFunctionCollectionGenerator());
        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task RouteNaming_ServerSide_KebabCase_TransformsEndpointRoute()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp;

                     public class KebabConfig : ServerFunctionConfiguration
                     {
                         public KebabConfig() { RouteNaming = RouteNaming.KebabCase; }
                     }

                     [ServerFunctionCollection(Configuration = typeof(KebabConfig))]
                     public interface IUserService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<string> GetUserByIdAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(source, new ServerFunctionCollectionGenerator());
        return result.VerifyNoDiagnostics();
    }

    // ─── DefaultHttpMethod ────────────────────────────────────────────────────

    [Fact]
    public Task DefaultHttpMethod_SuppressesBsf013AndAppliesMethod()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp;

                     public class PostOnlyConfig : ServerFunctionConfiguration
                     {
                         public PostOnlyConfig() { DefaultHttpMethod = "POST"; }
                     }

                     [ServerFunctionCollection(Configuration = typeof(PostOnlyConfig))]
                     public interface ICommandService
                     {
                         // No HttpMethod specified — should use POST from config without BSF013
                         [ServerFunction]
                         Task ExecuteAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(source, new ServerFunctionCollectionGenerator());
        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task DefaultHttpMethod_ExplicitMethodOnMethod_OverridesConfig()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp;

                     public class PostDefaultConfig : ServerFunctionConfiguration
                     {
                         public PostDefaultConfig() { DefaultHttpMethod = "POST"; }
                     }

                     [ServerFunctionCollection(Configuration = typeof(PostDefaultConfig))]
                     public interface IQueryService
                     {
                         // Explicit GET should override the POST default
                         [ServerFunction(HttpMethod = "GET")]
                         Task<string> GetStatusAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(source, new ServerFunctionCollectionGenerator());
        return result.VerifyNoDiagnostics();
    }

    // ─── CustomHttpClientType ─────────────────────────────────────────────────

    [Fact]
    public Task CustomHttpClientType_ChangesProxyConstructorType()
    {
        var source = """
                     using System.Net.Http;
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp;

                     public class TypedHttpClient : HttpClient { }

                     public class TypedClientConfig : ServerFunctionConfiguration
                     {
                         public TypedClientConfig() { CustomHttpClientType = typeof(TypedHttpClient); }
                     }

                     [ServerFunctionCollection(Configuration = typeof(TypedClientConfig))]
                     public interface IUserService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<string> GetUserAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(source, new ServerFunctionCollectionGenerator());
        return result.VerifyNoDiagnostics();
    }

    // ─── Nullable ─────────────────────────────────────────────────────────────

    [Fact]
    public Task Nullable_False_OmitsNullableDirective()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp;

                     public class NoNullConfig : ServerFunctionConfiguration
                     {
                         public NoNullConfig() { Nullable = false; }
                     }

                     [ServerFunctionCollection(Configuration = typeof(NoNullConfig))]
                     public interface IEchoService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<string> EchoAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(source, new ServerFunctionCollectionGenerator());
        return result.VerifyNoDiagnostics();
    }

    // ─── Inheritance ──────────────────────────────────────────────────────────

    [Fact]
    public Task InheritedConfig_DerivedOverridesBase()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp;

                     public class BaseConfig : ServerFunctionConfiguration
                     {
                         public BaseConfig()
                         {
                             BaseRoute = "api/v1";
                             RouteNaming = RouteNaming.KebabCase;
                         }
                     }

                     public class AdminConfig : BaseConfig
                     {
                         public AdminConfig()
                         {
                             BaseRoute = "api/v1/admin";
                         }
                     }

                     [ServerFunctionCollection(RequireAuthorization = true, Configuration = typeof(AdminConfig))]
                     public interface IAdminService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<string> GetStatsAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(source, new ServerFunctionCollectionGenerator());
        return result.VerifyNoDiagnostics();
    }

    // ─── Combined BaseRoute + RouteNaming ─────────────────────────────────────

    [Fact]
    public Task BaseRoute_And_RouteNaming_BothApplied()
    {
        var source = """
                     using System.Threading.Tasks;
                     using BlazorServerFunctions.Abstractions;

                     namespace MyApp;

                     public class FullConfig : ServerFunctionConfiguration
                     {
                         public FullConfig()
                         {
                             BaseRoute = "api/v2";
                             RouteNaming = RouteNaming.KebabCase;
                         }
                     }

                     [ServerFunctionCollection(Configuration = typeof(FullConfig))]
                     public interface IOrderService
                     {
                         [ServerFunction(HttpMethod = "GET")]
                         Task<string> GetOrderDetailsAsync();

                         [ServerFunction(HttpMethod = "POST")]
                         Task CreateNewOrderAsync();
                     }
                     """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(source, new ServerFunctionCollectionGenerator());
        return result.VerifyNoDiagnostics();
    }
}
