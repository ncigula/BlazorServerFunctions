namespace BlazorServerFunctions.Generator.UnitTests;

/// <summary>
/// Unit tests for gRPC client proxy code generation (§6.2).
/// Tests the full pipeline: string interface code → parser → GrpcClientProxyGenerator → output.
/// Each snapshot test verifies the generated client proxy class, contract interface, and request types.
/// </summary>
public class GrpcClientProxyGeneratorTests
{
    [Fact]
    public Task Generate_BasicGrpcInterface_ProducesClientProxy()
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

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_NoParams_UsesDirectCall()
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

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_CancellationToken_WrappedInCallContext()
    {
        // All using directives must appear before any type declarations in the compilation unit.
        // Roslyn needs 'using System.Threading' before it resolves CancellationToken.
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

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_AsyncEnumerable_ReturnedDirectly()
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

        var result = GeneratorTestHelper.RunGeneratorAsClient(
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

        var result = GeneratorTestHelper.RunGeneratorAsClient(
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

        var result = GeneratorTestHelper.RunGeneratorAsClient(
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

        var result = GeneratorTestHelper.RunGeneratorAsClient(
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

        var result = GeneratorTestHelper.RunGeneratorAsClient(
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

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    // ─── Auth / HttpMessageHandler tests (§6.5) ───────────────────────────────

    [Fact]
    public Task Generate_GrpcInterface_RegistrationIncludesHttpHandlerParameter()
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

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }
}
