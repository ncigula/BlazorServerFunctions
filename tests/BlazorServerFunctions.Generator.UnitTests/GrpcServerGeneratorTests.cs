namespace BlazorServerFunctions.Generator.UnitTests;

/// <summary>
/// Unit tests for gRPC server-side code generation (§6.1).
/// Tests the full pipeline: string interface code → parser → GrpcServerGenerator → output.
/// Each snapshot test verifies the generated service class and request types.
/// Each diagnostic test verifies BSF023/024/025 gRPC-specific validations.
/// </summary>
public class GrpcServerGeneratorTests
{
    // ─── Snapshot tests ───────────────────────────────────────────────────────

    [Fact]
    public Task Generate_BasicGrpcInterface_ProducesServiceClass()
    {
        var source = """
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection(ApiType = ApiType.GRPC)]
            public interface IUserService
            {
                [ServerFunction]
                Task<string> GetUserAsync(int id, string name);
            }
            """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_NoParams_UsesCallContextOnly()
    {
        var source = """
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection(ApiType = ApiType.GRPC)]
            public interface IPingService
            {
                [ServerFunction]
                Task PingAsync();
            }
            """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_CancellationToken_PassedViaContext()
    {
        // All using directives must appear before any type declarations in the compilation unit.
        var source = """
            using System.Threading;
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection(ApiType = ApiType.GRPC)]
            public interface IOrderService
            {
                [ServerFunction]
                Task<string> CreateOrderAsync(int quantity, CancellationToken cancellationToken);
            }
            """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_AsyncEnumerable_ServerStreaming()
    {
        var source = """
            using System.Collections.Generic;
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection(ApiType = ApiType.GRPC)]
            public interface IStreamService
            {
                [ServerFunction]
                IAsyncEnumerable<string> StreamNamesAsync(string prefix);
            }
            """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_MultipleInterfaces_BothGenerated()
    {
        var source = """
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection(ApiType = ApiType.GRPC)]
            public interface IUserService
            {
                [ServerFunction]
                Task<string> GetUserAsync(int id);
            }

            [ServerFunctionCollection(ApiType = ApiType.GRPC)]
            public interface IProductService
            {
                [ServerFunction]
                Task<string> GetProductAsync(int id);
            }
            """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_MixedRestAndGrpc_BothGenerated()
    {
        var source = """
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection]
            public interface IRestService
            {
                [ServerFunction(HttpMethod = "GET")]
                Task<string> GetAsync(int id);
            }

            [ServerFunctionCollection(ApiType = ApiType.GRPC)]
            public interface IGrpcService
            {
                [ServerFunction]
                Task<string> CallAsync(int id);
            }
            """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_VoidTask_NoReturnType()
    {
        var source = """
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection(ApiType = ApiType.GRPC)]
            public interface ICommandService
            {
                [ServerFunction]
                Task DeleteAsync(int id);
            }
            """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_DtoReturnType_PassesThroughToMethodSignature()
    {
        var source = """
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            public class UserDto
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";
            }

            [ServerFunctionCollection(ApiType = ApiType.GRPC)]
            public interface IUserService
            {
                [ServerFunction]
                Task<UserDto> GetUserAsync(int id);
            }
            """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_DtoParameter_GetsNullableInitializerInRequestWrapper()
    {
        var source = """
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            public class CreateUserRequest
            {
                public string Name { get; set; } = "";
            }

            public class UserDto
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";
            }

            [ServerFunctionCollection(ApiType = ApiType.GRPC)]
            public interface IUserService
            {
                [ServerFunction]
                Task<UserDto> CreateUserAsync(CreateUserRequest request);
            }
            """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    // ─── Auth tests (§6.5) ────────────────────────────────────────────────────

    [Fact]
    public Task Generate_MethodWithPolicy_AddsAuthorizeAttribute()
    {
        var source = """
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection(ApiType = ApiType.GRPC)]
            public interface ISecretService
            {
                [ServerFunction(Policy = "AdminOnly")]
                Task<string> GetSecretAsync();
            }
            """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_MethodWithRoles_AddsAuthorizeAttribute()
    {
        var source = """
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection(ApiType = ApiType.GRPC)]
            public interface ISecretService
            {
                [ServerFunction(Roles = "Admin,Manager")]
                Task<string> GetSecretAsync();
            }
            """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_MethodRequireAuth_AddsAuthorizeAttribute()
    {
        var source = """
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection(ApiType = ApiType.GRPC)]
            public interface ISecretService
            {
                [ServerFunction(RequireAuthorization = true)]
                Task<string> GetSecretAsync();
            }
            """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_InterfaceLevelRequireAuth_AddsClassAuthorizeAndRequireAuthorization()
    {
        var source = """
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection(ApiType = ApiType.GRPC, RequireAuthorization = true)]
            public interface ISecretService
            {
                [ServerFunction]
                Task<string> GetSecretAsync();

                [ServerFunction]
                Task<string> GetOtherSecretAsync();
            }
            """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    // ─── Diagnostic tests ─────────────────────────────────────────────────────

    [Fact]
    public void BSF023_HttpMethodOnGrpcInterface_EmitsError()
    {
        var source = """
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection(ApiType = ApiType.GRPC)]
            public interface IUserService
            {
                [ServerFunction(HttpMethod = "GET")]
                Task<string> GetUserAsync(int id);
            }
            """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        result.AssertDiagnostic("BSF023");
    }

    [Fact]
    public void BSF024_CacheSecondsOnGrpcInterface_EmitsWarning()
    {
        var source = """
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection(ApiType = ApiType.GRPC)]
            public interface IUserService
            {
                [ServerFunction(CacheSeconds = 30)]
                Task<string> GetUserAsync(int id);
            }
            """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        result.AssertDiagnostic("BSF024");
    }

    [Fact]
    public void BSF025_AntiForgeryOnGrpcInterface_EmitsWarning()
    {
        var source = """
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection(ApiType = ApiType.GRPC)]
            public interface IUserService
            {
                [ServerFunction(RequireAntiForgery = true)]
                Task<string> GetUserAsync(int id);
            }
            """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        result.AssertDiagnostic("BSF025");
    }

    [Fact]
    public void BSF012_NotEmitted_ForGrpcInterface()
    {
        // gRPC methods do not need HttpMethod — BSF012 must NOT be emitted
        var source = """
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection(ApiType = ApiType.GRPC)]
            public interface IUserService
            {
                [ServerFunction]
                Task<string> GetUserAsync(int id);
            }
            """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        Assert.DoesNotContain(result.Diagnostics,
            d => string.Equals(d.Id, "BSF012", StringComparison.Ordinal));
    }
}
