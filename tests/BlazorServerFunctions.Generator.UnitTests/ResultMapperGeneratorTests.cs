namespace BlazorServerFunctions.Generator.UnitTests;

/// <summary>
/// Tests for code generation when <c>ResultMapper</c> is set on
/// <c>[ServerFunctionCollection]</c>.
///
/// Each test exercises the full pipeline: string interface code → parser → generators → output.
/// Snapshot files capture both client proxy and server endpoint output.
/// </summary>
public class ResultMapperGeneratorTests
{
    // ── Shared stubs used as inline source in the test strings ──────────────

    /// <summary>
    /// Minimal <c>Result&lt;T&gt;</c> stub + mapper used for single-type-arg tests.
    /// Both types are declared in the same compilation as the interface under test.
    /// </summary>
    private const string SingleArgResultStubs = """
        using BlazorServerFunctions.Abstractions;

        public record Result<T>(bool IsSuccess, T Value, string? ErrorMessage, int Status = 500);

        public class ResultMapper<T> : IServerFunctionResultMapper<Result<T>, T>
            where T : notnull
        {
            public bool IsSuccess(Result<T> r)              => r.IsSuccess;
            public T GetValue(Result<T> r)                   => r.Value;
            public ServerFunctionError GetError(Result<T> r) => new() { Status = r.Status, Detail = r.ErrorMessage };
            public Result<T> WrapValue(T v)                  => new(true, v, null);
            public Result<T> WrapFailure(ServerFunctionError e) => new(false, default!, e.Detail, e.Status);
        }
        """;

    /// <summary>
    /// Two-type-arg <c>Result&lt;T, TError&gt;</c> stub + mapper.
    /// </summary>
    private const string TwoArgResultStubs = """
        using BlazorServerFunctions.Abstractions;

        public record Error(string Code, string Message);
        public record Result<T, TError>(bool IsSuccess, T Value, TError ErrorValue);

        public class ResultMapper<T, TError> : IServerFunctionResultMapper<Result<T, TError>, T>
            where T : notnull
        {
            public bool IsSuccess(Result<T, TError> r)              => r.IsSuccess;
            public T GetValue(Result<T, TError> r)                   => r.Value;
            public ServerFunctionError GetError(Result<T, TError> r) => new() { Detail = r.ErrorValue?.ToString() };
            public Result<T, TError> WrapValue(T v)                  => new(true, v, default!);
            public Result<T, TError> WrapFailure(ServerFunctionError e) => new(false, default!, default!);
        }
        """;

    // ── Happy-path snapshot tests ────────────────────────────────────────────

    [Fact]
    public Task Generate_ResultMapper_SingleTypeArg_Client()
    {
        var source = SingleArgResultStubs + """
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection(RoutePrefix = "orders", ResultMapper = typeof(ResultMapper<>))]
            public interface IOrderService
            {
                [ServerFunction(HttpMethod = "GET")]
                Task<Result<OrderDto>> GetOrderAsync(int id);

                [ServerFunction(HttpMethod = "POST")]
                Task<Result<OrderDto>> CreateOrderAsync(string name);
            }

            public record OrderDto(int Id, string Name);
            """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_ResultMapper_SingleTypeArg_Server()
    {
        var source = SingleArgResultStubs + """
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection(RoutePrefix = "orders", ResultMapper = typeof(ResultMapper<>))]
            public interface IOrderService
            {
                [ServerFunction(HttpMethod = "GET")]
                Task<Result<OrderDto>> GetOrderAsync(int id);

                [ServerFunction(HttpMethod = "POST")]
                Task<Result<OrderDto>> CreateOrderAsync(string name);
            }

            public record OrderDto(int Id, string Name);
            """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_ResultMapper_TwoTypeArgs_Client()
    {
        var source = TwoArgResultStubs + """
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection(RoutePrefix = "users", ResultMapper = typeof(ResultMapper<,>))]
            public interface IUserService
            {
                [ServerFunction(HttpMethod = "GET")]
                Task<Result<UserDto, Error>> GetUserAsync(int id);

                [ServerFunction(HttpMethod = "DELETE")]
                Task<Result<UserDto, Error>> DeleteUserAsync(int id);
            }

            public record UserDto(int Id, string Name);
            """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_ResultMapper_TwoTypeArgs_Server()
    {
        var source = TwoArgResultStubs + """
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection(RoutePrefix = "users", ResultMapper = typeof(ResultMapper<,>))]
            public interface IUserService
            {
                [ServerFunction(HttpMethod = "GET")]
                Task<Result<UserDto, Error>> GetUserAsync(int id);

                [ServerFunction(HttpMethod = "DELETE")]
                Task<Result<UserDto, Error>> DeleteUserAsync(int id);
            }

            public record UserDto(int Id, string Name);
            """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_ResultMapper_VoidMethod_SkipsMapper()
    {
        // void-returning methods never use the mapper — they get Results.Ok() with no body.
        // No BSF030 warning should be emitted for void.
        var source = SingleArgResultStubs + """
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection(RoutePrefix = "notifications", ResultMapper = typeof(ResultMapper<>))]
            public interface INotificationService
            {
                [ServerFunction(HttpMethod = "POST")]
                Task NotifyAsync(string message);
            }
            """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_ResultMapper_StreamingMethod_ExcludedFromMapping()
    {
        // IAsyncEnumerable methods bypass the mapper — no diagnostic, just falls through.
        var source = SingleArgResultStubs + """
            using System.Collections.Generic;
            using System.Threading;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection(RoutePrefix = "events", ResultMapper = typeof(ResultMapper<>))]
            public interface IEventService
            {
                [ServerFunction(HttpMethod = "GET")]
                IAsyncEnumerable<string> StreamEventsAsync(CancellationToken cancellationToken = default);
            }
            """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_ResultMapper_MixedMethods_WrappedAndVoid()
    {
        // Collection with both wrapped and void methods — only wrapped ones use the mapper.
        var source = SingleArgResultStubs + """
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection(RoutePrefix = "products", ResultMapper = typeof(ResultMapper<>))]
            public interface IProductService
            {
                [ServerFunction(HttpMethod = "GET")]
                Task<Result<ProductDto>> GetProductAsync(int id);

                [ServerFunction(HttpMethod = "DELETE")]
                Task DeleteProductAsync(int id);
            }

            public record ProductDto(int Id, string Name);
            """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    [Fact]
    public Task Generate_ResultMapper_WithCancellationToken_Client()
    {
        // Verifies that methods with CancellationToken parameters are code-generated correctly
        // in the mapper path.  Note: in the unit test compilation context CancellationToken does
        // not fully resolve (the parser cannot confirm its fully-qualified name), so it is treated
        // as a regular parameter in the snapshot — this is a known unit-test compilation
        // limitation.  End-to-end tests (ResultDemoServiceClientTests) exercise the real CT
        // behaviour against the live sample app.
        var source = SingleArgResultStubs + """
            using System.Threading;
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection(RoutePrefix = "stock", ResultMapper = typeof(ResultMapper<>))]
            public interface IStockService
            {
                [ServerFunction(HttpMethod = "GET")]
                Task<Result<StockDto>> GetStockAsync(int id, CancellationToken cancellationToken = default);

                [ServerFunction(HttpMethod = "POST")]
                Task<Result<StockDto>> ReserveStockAsync(string sku, CancellationToken cancellationToken = default);
            }

            public record StockDto(int Id, string Sku);
            """;

        var result = GeneratorTestHelper.RunGeneratorAsClient(
            source,
            new ServerFunctionCollectionGenerator());

        return result.VerifyNoDiagnostics();
    }

    // ── Diagnostic tests ─────────────────────────────────────────────────────

    [Fact]
    public void Generate_ResultMapper_OnGrpc_ReportsBsf029()
    {
        var source = SingleArgResultStubs + """
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection(ApiType = ApiType.GRPC, ResultMapper = typeof(ResultMapper<>))]
            public interface IOrderService
            {
                [ServerFunction]
                Task<Result<string>> GetAsync();
            }
            """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        result.AssertDiagnostic("BSF029");
    }

    [Fact]
    public void Generate_ResultMapper_NonGenericReturnType_ReportsBsf030()
    {
        var source = SingleArgResultStubs + """
            using System.Threading.Tasks;
            using BlazorServerFunctions.Abstractions;

            namespace MyApp.Services;

            [ServerFunctionCollection(RoutePrefix = "misc", ResultMapper = typeof(ResultMapper<>))]
            public interface IMiscService
            {
                [ServerFunction(HttpMethod = "GET")]
                Task<string> GetVersionAsync();
            }
            """;

        var result = GeneratorTestHelper.RunGeneratorAsServer(
            source,
            new ServerFunctionCollectionGenerator());

        result.AssertDiagnostic("BSF030");
    }
}
