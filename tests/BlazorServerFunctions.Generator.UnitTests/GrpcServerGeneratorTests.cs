namespace BlazorServerFunctions.Generator.UnitTests;

/// <summary>
/// Unit tests for gRPC server-side code generation (§6.1).
/// Tests the full pipeline: string interface code → parser → GrpcServerGenerator → output.
/// Each snapshot test verifies the generated service class and request types.
/// Each diagnostic test verifies BSF023/024/025 gRPC-specific validations.
/// </summary>
public class GrpcServerGeneratorTests
{
    // ─── Helpers ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Common gRPC config preamble included in every test source.
    /// Declares a <c>TestGrpcConfig</c> with <c>ApiType = ApiType.GRPC</c>.
    /// </summary>
    private const string GrpcConfigPreamble = """
        using BlazorServerFunctions.Abstractions;

        public class TestGrpcConfig : ServerFunctionConfiguration
        {
            public TestGrpcConfig() { ApiType = ApiType.GRPC; }
        }

        """;

    // ─── Snapshot tests ───────────────────────────────────────────────────────

    [Fact]
    public Task Generate_BasicGrpcInterface_ProducesServiceClass()
    {
        var source = GrpcConfigPreamble + """
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection(Configuration = typeof(TestGrpcConfig))]
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
        var source = GrpcConfigPreamble + """
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection(Configuration = typeof(TestGrpcConfig))]
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
        // The TestGrpcConfig class is declared inline here (not via preamble) so that
        // 'using System.Threading' can precede it and Roslyn resolves CancellationToken correctly.
        var source = """
            using System.Threading;
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            public class TestGrpcConfig : ServerFunctionConfiguration
            {
                public TestGrpcConfig() { ApiType = ApiType.GRPC; }
            }

            namespace MyApp.Services;

            [ServerFunctionCollection(Configuration = typeof(TestGrpcConfig))]
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
        var source = GrpcConfigPreamble + """
            using System.Collections.Generic;
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection(Configuration = typeof(TestGrpcConfig))]
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
        var source = GrpcConfigPreamble + """
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection(Configuration = typeof(TestGrpcConfig))]
            public interface IUserService
            {
                [ServerFunction]
                Task<string> GetUserAsync(int id);
            }

            [ServerFunctionCollection(Configuration = typeof(TestGrpcConfig))]
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
        var source = GrpcConfigPreamble + """
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection]
            public interface IRestService
            {
                [ServerFunction(HttpMethod = "GET")]
                Task<string> GetAsync(int id);
            }

            [ServerFunctionCollection(Configuration = typeof(TestGrpcConfig))]
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
        var source = GrpcConfigPreamble + """
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection(Configuration = typeof(TestGrpcConfig))]
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
        var source = GrpcConfigPreamble + """
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            public class UserDto
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";
            }

            [ServerFunctionCollection(Configuration = typeof(TestGrpcConfig))]
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
        var source = GrpcConfigPreamble + """
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

            [ServerFunctionCollection(Configuration = typeof(TestGrpcConfig))]
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

    // ─── Diagnostic tests ─────────────────────────────────────────────────────

    [Fact]
    public void BSF023_HttpMethodOnGrpcInterface_EmitsError()
    {
        var source = GrpcConfigPreamble + """
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection(Configuration = typeof(TestGrpcConfig))]
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
        var source = GrpcConfigPreamble + """
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection(Configuration = typeof(TestGrpcConfig))]
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
        var source = GrpcConfigPreamble + """
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection(Configuration = typeof(TestGrpcConfig))]
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
        var source = GrpcConfigPreamble + """
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection(Configuration = typeof(TestGrpcConfig))]
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
