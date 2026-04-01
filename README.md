# BlazorServerFunctions

[![NuGet](https://img.shields.io/nuget/v/BlazorServerFunctions.svg)](https://www.nuget.org/packages/BlazorServerFunctions)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Benchmarks](https://img.shields.io/badge/benchmarks-live-blue)](https://ncigula.github.io/BlazorServerFunctions/dev/bench/)

**Zero-boilerplate HTTP client proxies and ASP.NET Core minimal API endpoints — generated at compile time from a single C# interface.**

Annotate an interface once. BlazorServerFunctions generates the HttpClient proxy, the server-side minimal API endpoints, and the DI wiring for both. Works with Blazor Server, WASM, and Auto render modes.

---

## Installation

```bash
dotnet add package BlazorServerFunctions
```

The package is a Roslyn source generator — no runtime dependency, no reflection, no middleware.

---

## Quick Start

### 1. Define a shared interface (e.g. in a `.Shared` project)

```csharp
using BlazorServerFunctions.Abstractions;

[ServerFunctionCollection(RoutePrefix = "api/weather")]
public interface IWeatherService
{
    [ServerFunction(HttpMethod = "GET")]
    Task<WeatherForecast[]> GetForecastAsync(string city, CancellationToken ct = default);

    [ServerFunction(HttpMethod = "POST")]
    Task<WeatherForecast> CreateForecastAsync(WeatherForecast forecast, CancellationToken ct = default);
}
```

### 2. Implement it on the server

```csharp
public class WeatherService : IWeatherService
{
    public Task<WeatherForecast[]> GetForecastAsync(string city, CancellationToken ct) { ... }
    public Task<WeatherForecast> CreateForecastAsync(WeatherForecast forecast, CancellationToken ct) { ... }
}
```

### 3. Register on the server (`Program.cs`)

```csharp
builder.Services.AddScoped<IWeatherService, WeatherService>();

var app = builder.Build();

app.MapServerFunctionEndpoints(); // generated — maps all endpoints
```

### 4. Register on the client (`Program.cs` — WASM or Blazor Server)

```csharp
builder.Services.AddServerFunctionClients(
    baseAddress: new Uri("https://localhost:5001"));
```

### 5. Inject and use in a Blazor component

```razor
@inject IWeatherService WeatherService

@code {
    WeatherForecast[]? forecasts;

    protected override async Task OnInitializedAsync()
    {
        forecasts = await WeatherService.GetForecastAsync("London");
    }
}
```

The generator produces a `WeatherServiceClient` that implements `IWeatherService` — your component code never changes between render modes.

---

## Configuration

For advanced scenarios, subclass `ServerFunctionConfiguration` to share settings across multiple interfaces.

```csharp
// Define once — can be shared across interfaces via inheritance
public class MyApiConfig : ServerFunctionConfiguration
{
    public MyApiConfig()
    {
        BaseRoute = "api/v1";
        RouteNaming = RouteNaming.KebabCase;
    }
}

// Apply to an interface
[ServerFunctionCollection(Configuration = typeof(MyApiConfig))]
public interface IUserService
{
    [ServerFunction(HttpMethod = "GET")]
    Task<User[]> GetUsersAsync();  // → GET /api/v1/userservice/get-users-async
}
```

### Available settings

| Setting | Type | Default | Description |
|---|---|---|---|
| `BaseRoute` | `string` | `"api/functions"` | Route prefix for all endpoints in the collection |
| `RouteNaming` | `RouteNaming` | `PascalCase` | Route segment casing: `PascalCase`, `CamelCase`, `KebabCase`, `SnakeCase` |
| `DefaultHttpMethod` | `string?` | `null` | Default HTTP method when `[ServerFunction]` doesn't specify one (suppresses BSF013) |
| `GenerateProblemDetails` | `bool` | `true` | Emit Problem Details error responses from server endpoints |
| `Nullable` | `bool` | `true` | Emit `#nullable enable` at the top of generated files |
| `CustomHttpClientType` | `Type?` | `null` | Use a custom `HttpClient` subclass in generated proxy constructors |
| `ApiType` | `ApiType` | `REST` | Transport protocol (`REST` or `GRPC`) |
| `CacheSeconds` | `int` | `0` | Default output-cache duration (seconds) for all GET endpoints; `0` = disabled; overridable per method |
| `RateLimitPolicy` | `string?` | `null` | Default rate-limiting policy name for all endpoints; `null` = none; overridable per method |
| `Policy` | `string?` | `null` | Default named authorization policy for all endpoints; `null` = none; overridable per method (`""` = opt out) |

**Configuration priority (highest wins):**
```
[ServerFunction(...)] attribute property   ← highest
[ServerFunctionCollection(Configuration = typeof(...))]
Generator defaults                         ← lowest
```

---

## Attribute Reference

### `[ServerFunctionCollection]`

Applied to an interface. Controls route prefix, authorization, and optional configuration for all methods in the collection.

| Property | Type | Default | Description |
|---|---|---|---|
| `RoutePrefix` | `string?` | `null` | Route prefix prepended to all method routes (e.g. `"api/users"`) |
| `RequireAuthorization` | `bool` | `false` | Calls `.RequireAuthorization()` on the generated route group |
| `CorsPolicy` | `string?` | `null` | Named CORS policy applied via `group.RequireCors("name")` on the route group. `null` = no CORS. Empty string is an error (BSF022). Requires `AddCors(...)` + `UseCors()` in the server pipeline |
| `Configuration` | `Type?` | `null` | A `ServerFunctionConfiguration` subclass that controls code generation settings |

### `[ServerFunction]`

Applied to a method. Controls the HTTP method, route, authorization, caching, and rate limiting.

| Property | Type | Default | Description |
|---|---|---|---|
| `HttpMethod` | `string` | *(required)* | `"GET"`, `"POST"`, `"PUT"`, `"PATCH"`, or `"DELETE"` |
| `Route` | `string?` | Method name | Route segment appended to the collection's prefix; supports `{param}` placeholders |
| `RequireAuthorization` | `bool` | `false` | Calls `.RequireAuthorization()` on this specific endpoint |
| `CacheSeconds` | `int` | `-1` (inherit) | Seconds to cache via `.CacheOutput(...)`; `-1` = inherit from config, `0` = disable. Only valid on GET endpoints. Requires `AddOutputCache()` + `UseOutputCache()` in the server pipeline |
| `RateLimitPolicy` | `string?` | `null` (inherit) | Named rate-limiting policy applied via `.RequireRateLimiting("name")`; `null` = inherit from config, `""` = disable. Requires `AddRateLimiter(...)` + `UseRateLimiter()` in the server pipeline |
| `Policy` | `string?` | `null` (inherit) | Named authorization policy applied via `.RequireAuthorization("name")`; `null` = inherit from config, `""` = disable. Does not affect the boolean `RequireAuthorization` setting |
| `Roles` | `string?` | `null` | Comma-separated role names applied via `.RequireAuthorization(new AuthorizeAttribute { Roles = "..." })`; `null` = no restriction. Can be combined with `Policy` and `RequireAuthorization`. Empty string is an error (BSF021). |
| `RequireAntiForgery` | `bool` | `false` | Adds `.WithMetadata(new RequireAntiforgeryTokenAttribute())` to the endpoint. Requires `AddAntiforgery()` + `UseAntiforgery()` in the server pipeline. |
| `Filters` | `Type[]?` | `null` | One or more `IEndpointFilter` types applied via `.AddEndpointFilter<T>()` in declaration order. Example: `Filters = new[] { typeof(MyFilter) }` |

---

## DI Setup

### Server-side (`Program.cs`)

```csharp
// Register your service implementations as usual
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// One call maps all generated endpoints
app.MapServerFunctionEndpoints();
```

### Client-side (`Program.cs`)

```csharp
// Simple — base address only
builder.Services.AddServerFunctionClients(
    baseAddress: new Uri(builder.HostEnvironment.BaseAddress));

// Advanced — customise each IHttpClientBuilder (auth handlers, resilience, etc.)
builder.Services.AddServerFunctionClients(
    baseAddress: new Uri(builder.HostEnvironment.BaseAddress),
    configureClient: builder => builder.AddHttpMessageHandler<AuthHandler>());
```

`configureClient` is called for every registered service client, so it's the right place for cross-cutting concerns like JWT bearer tokens, cookie forwarding, resilience pipelines, or logging handlers.

---

## Authentication & JWT Example

**Server** — protect the whole collection:

```csharp
[ServerFunctionCollection(RoutePrefix = "api/admin", RequireAuthorization = true)]
public interface IAdminService
{
    [ServerFunction(HttpMethod = "GET")]
    Task<AdminStats> GetStatsAsync();
}
```

Or protect individual methods:

```csharp
[ServerFunctionCollection(RoutePrefix = "api/users")]
public interface IUserService
{
    [ServerFunction(HttpMethod = "GET")]
    Task<UserProfile[]> GetAllAsync();                         // public

    [ServerFunction(HttpMethod = "DELETE", RequireAuthorization = true)]
    Task DeleteUserAsync(Guid id);                             // protected
}
```

**Client** — attach a JWT bearer delegating handler:

```csharp
public class JwtBearerHandler : DelegatingHandler
{
    private readonly ITokenService _tokens;
    public JwtBearerHandler(ITokenService tokens) => _tokens = tokens;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        var token = await _tokens.GetAccessTokenAsync(ct);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, ct);
    }
}

// Registration
builder.Services.AddTransient<JwtBearerHandler>();
builder.Services.AddServerFunctionClients(
    baseAddress: new Uri(builder.HostEnvironment.BaseAddress),
    configureClient: b => b.AddHttpMessageHandler<JwtBearerHandler>());
```

**Error handling** — when the server returns a non-success status code, the generated client throws `HttpRequestException`. If the server returned a Problem Details body (RFC 9457), its `detail` field is forwarded as the exception message.

---

## Route / Path Parameters

Use `{param}` placeholders in `Route` to bind URL segments:

```csharp
[ServerFunction(HttpMethod = "GET", Route = "users/{id}")]
Task<User> GetUserAsync(Guid id);

[ServerFunction(HttpMethod = "DELETE", Route = "users/{id}")]
Task DeleteUserAsync(Guid id);
```

The generator binds route parameters from the URL on the server and interpolates them into the request URL on the client — no manual wiring needed.

---

## File Upload

Pass `Stream`, `IFormFile`, or `IFormFileCollection` parameters to upload files through a generated endpoint.

```csharp
[ServerFunctionCollection(RoutePrefix = "api/files")]
public interface IFileService
{
    // Stream — works in WASM and Blazor Server alike
    [ServerFunction(HttpMethod = "POST")]
    Task<string> UploadAsync(Stream file, string fileName);

    // IFormFile — when the caller is server-side only
    [ServerFunction(HttpMethod = "POST")]
    Task<string> UploadFormFileAsync(IFormFile document);

    // Multiple files at once
    [ServerFunction(HttpMethod = "POST")]
    Task<int> UploadManyAsync(IFormFileCollection attachments);
}
```

**Client proxy** — builds `MultipartFormDataContent` automatically:
- `Stream` parameters → `StreamContent` part (name = parameter name)
- `IFormFile` parameters → `StreamContent` wrapping `.OpenReadStream()`, with `FileName` preserved
- `IFormFileCollection` parameters → one `StreamContent` part per file in the collection
- Regular parameters alongside files → `StringContent` form fields

**Server endpoint** — binds via inline `IFormFile`/`[FromForm]` parameters (no DTO record). `.DisableAntiforgery()` is added automatically so global `UseAntiforgery()` middleware does not silently reject multipart requests.

**Restrictions** (compile-time diagnostics):
- **BSF026** (error) — file parameter on a GET or DELETE method; use POST, PUT, or PATCH
- **BSF027** (error) — file parameter combined with `IAsyncEnumerable<T>` return; multipart and streaming cannot be combined
- **BSF028** (error) — file parameter on a gRPC interface; file upload is REST-only

> **WASM note:** `Stream` is the recommended type for interfaces declared in a shared library — `IFormFile` is an ASP.NET Core type that may not be available in WASM projects. The server endpoint always binds an `IFormFile` internally and calls `.OpenReadStream()` when the interface declared `Stream`.

---

## Explicit parameter binding

By default the generator infers how each parameter is bound: route-template matches go to the route, GET/DELETE non-route parameters go to the query string, and POST/PUT/PATCH non-route parameters go to the JSON body. Use `[ServerFunctionParameter]` to override this inference for a single parameter.

### `ParameterSource` reference

| Value | Server binding | Client dispatch | Notes |
|---|---|---|---|
| `Auto` | Inferred (default) | Inferred (default) | No attribute needed |
| `Route` | Route segment (same as inferred) | URL path | Compile-time validation marker — BSF031 (error) if `{paramName}` absent from template |
| `Query` | `[FromQuery]` | URL query string | Useful to force a query-string param on POST/PUT/PATCH |
| `Body` | `[FromBody]` | JSON request body | **Not supported on GET/DELETE** — BSF032 (error). Browsers (Fetch API) forbid a body on GET/DELETE. |
| `Header` | `[FromHeader(Name = "...")]` | `requestMessage.Headers.Add(...)` | Use `Name` to specify the HTTP header name |

### `ParameterSource.Header`

```csharp
[ServerFunctionCollection]
public interface IOrderService
{
    [ServerFunction(HttpMethod = "POST")]
    Task<Order> CreateOrderAsync(
        [ServerFunctionParameter(From = ParameterSource.Header, Name = "X-Tenant-Id")] string tenantId,
        string productId,
        int quantity);
}
```

Generated server endpoint:
```csharp
group.MapPost("/CreateOrderAsync",
    async Task<Results<Ok<Order>, ProblemHttpResult>> (
        [FromHeader(Name = "X-Tenant-Id")] string tenantId,
        [FromBody] CreateOrderAsyncRequest request,
        IOrderService service) => { ... })

private record CreateOrderAsyncRequest(string ProductId, int Quantity);
```

Generated client proxy:
```csharp
var requestMessage = new HttpRequestMessage
{
    Method = HttpMethod.Post,
    RequestUri = new Uri($"{BaseRoute}/CreateOrderAsync", UriKind.Relative),
    Content = JsonContent.Create(request)
};
requestMessage.Headers.Add("X-Tenant-Id", tenantId.ToString());
var response = await _httpClient.SendAsync(requestMessage);
```

### `ParameterSource.Query` on POST

Force specific parameters to the URL query string even on a POST/PUT/PATCH method:

```csharp
[ServerFunction(HttpMethod = "POST")]
Task<PagedResult<Item>> SearchAsync(
    [ServerFunctionParameter(From = ParameterSource.Query)] int page,
    [ServerFunctionParameter(From = ParameterSource.Query)] int pageSize,
    SearchFilter filter);   // Auto → stays in JSON body
```

### `ParameterSource.Route`

An explicit compile-time assertion that the parameter must appear as `{paramName}` in the route template. The generated code is identical to inferred route binding — this attribute is purely a documentation and safety tool. Diagnostic **BSF031** (error) fires if the template does not contain `{paramName}`.

```csharp
[ServerFunction(HttpMethod = "DELETE", Route = "items/{id}")]
Task DeleteItemAsync(
    [ServerFunctionParameter(From = ParameterSource.Route)] int id);
```

---

## Using with MediatR

BlazorServerFunctions works naturally alongside the Mediator pattern. The generated interface is a thin adapter — each method sends a query or command and unwraps the result:

```csharp
// Shared interface (drives generation)
[ServerFunctionCollection(RoutePrefix = "api/users")]
public interface IUserService
{
    [ServerFunction(HttpMethod = "GET", Route = "users/{id}")]
    Task<UserDto> GetUserAsync(Guid id);

    [ServerFunction(HttpMethod = "POST")]
    Task<UserDto> CreateUserAsync(CreateUserRequest request);
}
```

```csharp
// Server implementation — one line per method
public class UserService(IMediator mediator) : IUserService
{
    public async Task<UserDto> GetUserAsync(Guid id)
    {
        var result = await mediator.Send(new GetUserQuery(id));
        return result.IsSuccess ? result.Value : throw new NotFoundException(result.Error);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
    {
        var result = await mediator.Send(new CreateUserCommand(request));
        return result.IsSuccess ? result.Value : throw new ValidationException(result.Error);
    }
}
```

**Failure handling** — throw a typed exception and map it to a `ProblemDetails` response once, centrally, using ASP.NET Core's built-in exception handler:

```csharp
// Program.cs — registered once, applies to every endpoint
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddProblemDetails();
```

```csharp
public class DomainExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken ct)
    {
        var (status, title) = exception switch
        {
            NotFoundException e  => (404, e.Message),
            ValidationException e => (422, e.Message),
            _ => (0, null)
        };

        if (status == 0) return false;

        await Results.Problem(title: title, statusCode: status).ExecuteAsync(context);
        return true;
    }
}
```

The generated client proxy already converts non-success HTTP responses into `HttpRequestException` with the `ProblemDetails.detail` field as the message — so callers in Blazor components get a clean exception with the original error message.

> **Query/command object shape** — parameter names in the interface do not need to match the MediatR request constructor. The wrapper service is the mapping layer; use it to translate freely.

---

## Result Mapper

When service methods return a result wrapper type (`Result<T>`, `Result<T, TError>`, `OneOf<T1, T2>`, etc.), configure a `ResultMapper` on the collection and the generator handles the unwrapping on both sides automatically.

### Complete example

The following steps show everything needed to add result-typed methods from scratch.

**Step 1 — Define your `Result<T>` type in the shared project**

You can use any third-party library (ErrorOr, FluentResults, OneOf, etc.) or roll your own:

```csharp
// MyApp.Shared/Result.cs
public sealed class Result<T>
{
    private Result(bool isSuccess, T? value, string? errorCode, string? message, int status)
    {
        IsSuccess = isSuccess; Value = value;
        ErrorCode = errorCode; ErrorMessage = message; Status = status;
    }

    public bool IsSuccess   { get; }
    public T?   Value       { get; }
    public string? ErrorCode    { get; }
    public string? ErrorMessage { get; }
    public int     Status       { get; }   // HTTP status intended for error responses

    public static Result<T> Ok(T value)            => new(true,  value,   null,          null,    200);
    public static Result<T> NotFound(string msg)   => new(false, default, "NOT_FOUND",   msg,     404);
    public static Result<T> Conflict(string msg)   => new(false, default, "CONFLICT",    msg,     409);
    public static Result<T> Invalid(string msg)    => new(false, default, "VALIDATION",  msg,     400);
    public static Result<T> Failure(string msg)    => new(false, default, "FAILURE",     msg,     500);
}
```

**Step 2 — Implement `IServerFunctionResultMapper<TResult, TValue>` once**

```csharp
// MyApp.Shared/ResultMapper.cs
public sealed class ResultMapper<T> : IServerFunctionResultMapper<Result<T>, T>
    where T : notnull
{
    // ── Server side: unwrap the result into an HTTP response ──────────────────

    public bool IsSuccess(Result<T> r) => r.IsSuccess;
    public T GetValue(Result<T> r)     => r.Value!;

    public ServerFunctionError GetError(Result<T> r) => new()
    {
        Status = r.Status,        // drives the HTTP status code (404, 409, 400…)
        Title  = r.ErrorCode,     // ProblemDetails "title" field
        Detail = r.ErrorMessage,  // ProblemDetails "detail" field
    };

    // ── Client side: reconstruct Result<T> from the HTTP response ─────────────

    public Result<T> WrapValue(T value) => Result<T>.Ok(value);

    public Result<T> WrapFailure(ServerFunctionError e) => e.Status switch
    {
        404 => Result<T>.NotFound(e.Detail ?? "Not found"),
        409 => Result<T>.Conflict(e.Detail ?? "Conflict"),
        400 => Result<T>.Invalid(e.Detail ?? "Validation error"),
        _   => Result<T>.Failure(e.Detail ?? "An error occurred"),
    };
}
```

`ServerFunctionError` is defined in `BlazorServerFunctions.Abstractions` — it has no ASP.NET Core dependency and works in both server and Blazor WASM projects.

**Step 3 — Annotate the interface**

```csharp
// MyApp.Shared/IProductService.cs
[ServerFunctionCollection(
    Configuration = typeof(ApiConfig),
    ResultMapper  = typeof(ResultMapper<>))]   // open generic — BSF closes it per method
public interface IProductService
{
    [ServerFunction(HttpMethod = "GET", Route = "{id}")]
    Task<Result<ProductDto>> GetProductAsync(int id, CancellationToken ct = default);

    [ServerFunction(HttpMethod = "POST")]
    Task<Result<ProductDto>> CreateProductAsync(string name, decimal price, CancellationToken ct = default);

    [ServerFunction(HttpMethod = "DELETE", Route = "{id}")]
    Task<Result<ProductDto>> DeleteProductAsync(int id, CancellationToken ct = default);
}
```

**Step 4 — Implement the service (server project)**

```csharp
// MyApp.Server/ProductService.cs
public sealed class ProductService : IProductService
{
    public async Task<Result<ProductDto>> GetProductAsync(int id, CancellationToken ct)
    {
        var product = await _repository.FindAsync(id, ct);
        return product is null
            ? Result<ProductDto>.NotFound($"Product #{id} was not found.")
            : Result<ProductDto>.Ok(product.ToDto());
    }

    public async Task<Result<ProductDto>> CreateProductAsync(string name, decimal price, CancellationToken ct)
    {
        if (await _repository.ExistsAsync(name, ct))
            return Result<ProductDto>.Conflict($"A product named '{name}' already exists.");

        var created = await _repository.AddAsync(new Product(name, price), ct);
        return Result<ProductDto>.Ok(created.ToDto());
    }

    // …
}
```

**Step 5 — Use it in a Blazor component**

```razor
@* MyApp.Client/Pages/Products.razor *@
@inject IProductService ProductService

@if (_product is not null)
{
    <p>@_product.Name — @_product.Price.ToString("C")</p>
}
else if (_error is not null)
{
    <p class="text-danger">@_error</p>
}

@code {
    private ProductDto? _product;
    private string?     _error;

    protected override async Task OnInitializedAsync()
    {
        var result = await ProductService.GetProductAsync(id: 42);
        if (result.IsSuccess)
            _product = result.Value;
        else
            _error = result.ErrorMessage;   // populated from ProblemDetails.detail
    }
}
```

The Blazor component calls `IProductService` exactly as if it were a local service. The generated `ProductServiceClient` proxy handles all HTTP communication, mapper invocation, and error deserialization automatically.

### What gets generated

**Server endpoint** — calls the mapper to produce the HTTP response:

```csharp
// generated: IProductServiceServerExtensions.g.cs
group.MapGet("/{id}", async Task<Results<Ok<ProductDto>, ProblemHttpResult>> (int id, IProductService service) =>
{
    var result = await service.GetProductAsync(id);
    var __mapper = new ResultMapper<ProductDto>();
    if (__mapper.IsSuccess(result))
        return TypedResults.Ok(__mapper.GetValue(result));
    var __error = __mapper.GetError(result);
    return TypedResults.Problem(__error.Detail, statusCode: __error.Status,
                                title: __error.Title, type: __error.Type);
});
// .Produces<T> and .ProducesProblem are omitted — inferred from the typed lambda return
```

**Client proxy** — deserialises the inner value type on 2xx, parses ProblemDetails on error:

```csharp
// generated: ProductServiceClient.g.cs
// On 4xx/5xx → calls WrapFailure(ServerFunctionError) → Result<T>.NotFound / Conflict / …
// On 2xx     → calls WrapValue(dto)                  → Result<T>.Ok(dto)
```

### Works with two-type-arg results

Pass the two-type-arg open generic and provide a matching mapper overload:

```csharp
[ServerFunctionCollection(ResultMapper = typeof(ResultMapper<,>))]
public interface IUserService
{
    [ServerFunction(HttpMethod = "GET")]
    Task<Result<UserDto, ValidationError>> GetUserAsync(int id);
}

public sealed class ResultMapper<T, TError> : IServerFunctionResultMapper<Result<T, TError>, T>
    where T : notnull
{
    public bool IsSuccess(Result<T, TError> r)              => r.IsSuccess;
    public T GetValue(Result<T, TError> r)                   => r.Value!;
    public ServerFunctionError GetError(Result<T, TError> r) => new() { Status = 400, Detail = r.Error?.ToString() };
    public Result<T, TError> WrapValue(T v)                  => Result<T, TError>.Ok(v);
    public Result<T, TError> WrapFailure(ServerFunctionError e) => Result<T, TError>.Fail(e.Detail);
}
```

### Restrictions (compile-time diagnostics)

- **BSF029** (error) — `ResultMapper` on a gRPC interface; result mapping is REST-only.
- **BSF030** (warning) — a method's return type is non-generic when `ResultMapper` is set (e.g. `Task<string>`); the mapper cannot be applied and the method falls back to `TypedResults.Ok(result)` / direct deserialisation.

> **`void` and streaming methods** are silently excluded from mapper wrapping — `void` emits `TypedResults.Ok()` and `IAsyncEnumerable<T>` passes through unchanged. No diagnostic is emitted for these cases.

---

## Streaming (`IAsyncEnumerable<T>`)

Return `IAsyncEnumerable<T>` for chunked server-sent streaming:

```csharp
[ServerFunction(HttpMethod = "GET")]
IAsyncEnumerable<WeatherForecast> StreamForecastsAsync(CancellationToken ct = default);
```

The server endpoint returns the stream directly (ASP.NET Core handles chunked JSON). The client proxy reads the stream incrementally via `ReadFromJsonAsAsyncEnumerable<T>()` with `HttpCompletionOption.ResponseHeadersRead`.

---

## gRPC Quick-Start (code-first, no `.proto` files)

BlazorServerFunctions supports **code-first gRPC** via [protobuf-net.Grpc](https://github.com/protobuf-net/protobuf-net.Grpc). Set `ApiType = ApiType.GRPC` and the generator produces a gRPC service class and a matching client proxy — no `.proto` files, no manual contract maintenance.

### 1. Add NuGet references

**Shared project** (where the interface lives):

```xml
<PackageReference Include="protobuf-net.Grpc" Version="1.2.*" />
<PackageReference Include="System.ServiceModel.Primitives" Version="8.1.*" />
```

**Server project**:

```xml
<PackageReference Include="protobuf-net.Grpc.AspNetCore" Version="1.2.*" />
```

**Client project** (WASM / Blazor Server):

```xml
<PackageReference Include="Grpc.Net.Client" Version="2.*" />
```

### 2. Declare a gRPC interface in the shared project

```csharp
using BlazorServerFunctions.Abstractions;

[ServerFunctionCollection(ApiType = ApiType.GRPC)]
public interface IGrpcDemoService
{
    Task<string> EchoAsync(string message, CancellationToken ct = default);

    // IAsyncEnumerable<T> maps to a gRPC server-streaming method automatically
    IAsyncEnumerable<string> CountdownAsync(int from, CancellationToken ct = default);
}
```

> `HttpMethod`, `CacheSeconds`, and `RequireAntiForgery` have no meaning on gRPC interfaces — the generator reports diagnostics BSF023/BSF024/BSF025 for those.

### 3. Implement the service on the server

```csharp
public class GrpcDemoService : IGrpcDemoService
{
    public Task<string> EchoAsync(string message, CancellationToken ct)
        => Task.FromResult($"gRPC echo: {message}");

    public async IAsyncEnumerable<string> CountdownAsync(int from, [EnumeratorCancellation] CancellationToken ct)
    {
        for (var i = from; i >= 0; i--)
        {
            yield return i.ToString();
            await Task.Delay(100, ct);
        }
    }
}
```

### 4. Register on the server (`Program.cs`)

```csharp
using ProtoBuf.Grpc.Server;  // for AddCodeFirstGrpc()

builder.Services.AddCodeFirstGrpc();
builder.Services.AddScoped<IGrpcDemoService, GrpcDemoService>();

var app = builder.Build();

app.MapServerFunctionEndpoints(); // also calls MapGrpcService<GrpcDemoServiceGrpcService>()
```

### 5. Register on the client (`Program.cs`)

```csharp
// baseAddress is required when any gRPC interfaces are registered
builder.Services.AddServerFunctionClients(
    baseAddress: new Uri("https://localhost:5001"));
```

### What the generator produces

For a gRPC interface the generator emits:

| Generated file | Content |
|---|---|
| `{Interface}GrpcClient.g.cs` (shared) | `I{Service}GrpcContract` (wire contract with `[ServiceContract]`) + `{Service}GrpcClient : IXxxService` (calls the contract via `GrpcChannel.CreateGrpcService<T>()`) + `[ProtoContract]` request wrapper types |
| `{Interface}GrpcService.g.cs` (server) | `{Service}GrpcService : I{Service}GrpcContract` — the server-side implementation that delegates to your injected `IXxxService` |
| `ServerFunctionClientsRegistration.g.cs` | Registers `GrpcChannel` as a singleton and `{Service}GrpcClient` as transient |
| `ServerFunctionEndpointsRegistration.g.cs` | Calls `endpoints.MapGrpcService<{Service}GrpcService>()` |

---

## Output Caching

Cache GET responses with a single attribute property:

```csharp
// Per-method
[ServerFunction(HttpMethod = "GET", CacheSeconds = 30)]
Task<int> GetCountAsync();

// Or set a collection-level default and override per method
public class CachedConfig : ServerFunctionConfiguration
{
    public CachedConfig() { CacheSeconds = 60; }
}

[ServerFunctionCollection(Configuration = typeof(CachedConfig))]
public interface IProductService
{
    [ServerFunction(HttpMethod = "GET")]
    Task<Product[]> GetAllAsync();         // cached 60 s (from config)

    [ServerFunction(HttpMethod = "GET", CacheSeconds = 0)]
    Task<Product> GetByIdAsync(Guid id);   // explicitly disabled
}
```

Requires `builder.Services.AddOutputCache()` and `app.UseOutputCache()` in the server pipeline.

---

## Rate Limiting

Reference any named rate-limiting policy you've already registered:

```csharp
// Define the policy in Program.cs
builder.Services.AddRateLimiter(options =>
    options.AddFixedWindowLimiter("api", limiter =>
    {
        limiter.PermitLimit = 100;
        limiter.Window = TimeSpan.FromMinutes(1);
    }));
app.UseRateLimiter();

// Apply via attribute (any HTTP method, including streaming)
[ServerFunction(HttpMethod = "POST", RateLimitPolicy = "api")]
Task<Order> CreateOrderAsync(Order order);

// Or set a collection-level default
public class RateLimitedConfig : ServerFunctionConfiguration
{
    public RateLimitedConfig() { RateLimitPolicy = "api"; }
}
```

The generator emits `.RequireRateLimiting("api")` in the fluent endpoint chain. Per-method `RateLimitPolicy = ""` (empty string) explicitly opts a single method out of the collection-level default.

---

## OpenAPI Customization

Five optional properties on `[ServerFunction]` control per-endpoint OpenAPI metadata. All are optional — omitting them preserves the existing auto-generated behaviour.

```csharp
[ServerFunctionCollection(RoutePrefix = "api/products")]
public interface IProductService
{
    // Summary and description appear in Swagger UI
    [ServerFunction(
        HttpMethod = "GET",
        Summary = "Get a product",
        Description = "Returns the full product record including pricing and stock levels.")]
    Task<ProductDto> GetProductAsync(int id);

    // Override the auto-generated tag (defaults to interface name without the "I" prefix)
    [ServerFunction(HttpMethod = "POST", Tags = new[] { "Products", "Catalog" })]
    Task<ProductDto> CreateProductAsync(ProductDto product);

    // Document additional response codes alongside the default 200
    [ServerFunction(HttpMethod = "DELETE", ProducesStatusCodes = new[] { 404 })]
    Task DeleteProductAsync(int id);

    // Hide an endpoint from the OpenAPI documentation entirely
    [ServerFunction(HttpMethod = "GET", ExcludeFromOpenApi = true)]
    Task<string> GetInternalKeyAsync();
}
```

The generator emits the corresponding fluent calls:

| Property | Generated call |
|---|---|
| `Summary = "..."` | `.WithSummary("...")` |
| `Description = "..."` | `.WithDescription("...")` |
| `Tags = new[] { "A", "B" }` | `.WithTags("A", "B")` |
| `ProducesStatusCodes = new[] { 404, 409 }` | `.Produces(404)` `.Produces(409)` |
| `ExcludeFromOpenApi = true` | `.ExcludeFromDescription()` (instead of `.WithOpenApi()`) |

---

## Authorization Policies

Reference any named authorization policy you've already registered:

```csharp
// Apply via attribute
[ServerFunction(HttpMethod = "GET", Policy = "AdminOnly")]
Task<AdminStats> GetStatsAsync();

// Or set a collection-level default
public class AdminConfig : ServerFunctionConfiguration
{
    public AdminConfig() { Policy = "AdminOnly"; }
}
```

The generator emits `.RequireAuthorization("AdminOnly")` in the fluent endpoint chain. Per-method `Policy = ""` (empty string) explicitly opts a single method out of the collection-level default.

This can be combined with the boolean `RequireAuthorization` — a route group can have `.RequireAuthorization()` (from `[ServerFunctionCollection(RequireAuthorization = true)]`) while individual methods apply a more specific named policy on top.

### Role-based auth

Apply role restrictions directly on a method:

```csharp
[ServerFunction(HttpMethod = "DELETE", Roles = "Admin,Manager")]
Task DeleteUserAsync(Guid id);
```

The generator emits `.RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Manager" })`. Multiple roles are comma-separated (OR logic within the list — a user in any one of the roles passes).

`Roles` can be combined with `Policy` and `RequireAuthorization` on the same method — ASP.NET Core ANDs all authorization requirements together:

```csharp
// Must satisfy "PremiumPolicy" AND be in "Admin" or "Manager" role
[ServerFunction(HttpMethod = "GET", Policy = "PremiumPolicy", Roles = "Admin,Manager")]
Task<AdminStats> GetStatsAsync();
```

### CORS per interface

Apply a named CORS policy to all endpoints in a collection via the `CorsPolicy` attribute:

```csharp
[ServerFunctionCollection(RoutePrefix = "api/data", CorsPolicy = "AllowedOrigins")]
public interface IDataService
{
    [ServerFunction(HttpMethod = "GET")]
    Task<string[]> GetItemsAsync();
}
```

The generator emits `group.RequireCors("AllowedOrigins")` on the route group. You must register the policy and enable the middleware in the server:

```csharp
builder.Services.AddCors(options =>
    options.AddPolicy("AllowedOrigins", policy =>
        policy.WithOrigins("https://example.com").AllowAnyHeader().AllowAnyMethod()));

app.UseCors(); // before UseAuthorization
```

A collection-level default can also be set via `ServerFunctionConfiguration.CorsPolicy`; the attribute value overrides the config default.

### Anti-forgery

Apply `RequireAntiforgeryTokenAttribute` metadata to individual endpoints via `RequireAntiForgery = true`:

```csharp
[ServerFunctionCollection(RoutePrefix = "api/forms")]
public interface IFormService
{
    [ServerFunction(HttpMethod = "POST", RequireAntiForgery = true)]
    Task<string> SubmitFormAsync(string data);
}
```

Register antiforgery services and middleware in your server pipeline:

```csharp
builder.Services.AddAntiforgery();
// ...
app.UseAntiforgery();
```

### Endpoint filters

Apply one or more `IEndpointFilter` types to a method via `Filters = new[] { typeof(...) }`:

```csharp
// Implement the filter in a project that is accessible where the interface is declared
public sealed class LoggingFilter : IEndpointFilter
{
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // Pre-handler logic here
        return next(context);
    }
}

[ServerFunctionCollection]
public interface IOrderService
{
    // Single filter
    [ServerFunction(HttpMethod = "POST", Filters = new[] { typeof(LoggingFilter) })]
    Task<OrderDto> CreateOrderAsync(OrderDto order);
}
```

Multiple filters are applied in declaration order:

```csharp
[ServerFunction(HttpMethod = "POST", Filters = new[] { typeof(AuthFilter), typeof(LoggingFilter) })]
Task<OrderDto> CreateOrderAsync(OrderDto order);
// Generated: .AddEndpointFilter<AuthFilter>().AddEndpointFilter<LoggingFilter>()
```

---

## How It Works

The generator inspects every `[ServerFunctionCollection]` interface at compile time and produces four files:

| Generated file | Content |
|---|---|
| `{Interface}Client.g.cs` | `HttpClient`-based implementation of the interface |
| `{Interface}ServerExtensions.g.cs` | Minimal API endpoint mappings |
| `ServerFunctionClientsRegistration.g.cs` | `AddServerFunctionClients(...)` extension method |
| `ServerFunctionEndpointsRegistration.g.cs` | `MapServerFunctionEndpoints()` extension method |

The generator detects project type automatically:
- **Server project** (references `IEndpointRouteBuilder`) → generates all four files
- **WASM / Library project** → generates client proxy + client registration only

---

## Sample App

See [`samples/`](samples/) for a complete Blazor app demonstrating all four render modes (Server, WASM, Auto, and a plain HTTP client) with Aspire orchestration.

---

## License

MIT — see [LICENSE](LICENSE).
