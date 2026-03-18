# BlazorServerFunctions — Feature Roadmap

## Size legend
- 🟢 Small — a few hours
- 🟡 Medium — a day or two
- 🔴 Large — multiple days / architectural work

---

## 1. Packaging & discoverability

| # | Item | Size |
|---|---|---|
| 1.1 | **README.md** — Quick-start, attribute reference, DI setup, auth + JWT example, sample link | 🟢 |
| 1.2 | **NuGet versioning** — `<PackageVersion>`, `<PackageDescription>`, `<PackageReleaseNotes>` | 🟢 |
| 1.3 | **Package metadata** — Icon, license, repo URL, tags | 🟢 |
| 1.4 | **Source Link** — `<PublishRepositoryUrl>` + `<EmbedUntrackedSources>` so consumers can step into generated code | 🟢 |
| 1.5 | **CHANGELOG.md** | 🟢 |

---

## 2. Configuration system

### Design: type-based configuration class

Users create a class that inherits from `ServerFunctionConfiguration` and pass it to `[ServerFunctionCollection]`.
The generator reads the class's property initializers via Roslyn symbols at compile time — strongly typed, IDE-friendly, shareable via inheritance.

```csharp
// Abstractions — ships with the package
public class ServerFunctionConfiguration
{
    public string BaseRoute { get; init; } = "api/functions";
    public RouteNaming RouteNaming { get; init; } = RouteNaming.PascalCase;
    public string? DefaultHttpMethod { get; init; } = null;  // null = must be explicit
    public string HttpVersion { get; init; } = "1.1";
    public bool GenerateProblemDetails { get; init; } = true;
    public bool EnableResilience { get; init; } = false;
    public bool Nullable { get; init; } = true;
    public Type? CustomHttpClientType { get; init; } = null;  // e.g. typeof(MyHttpClient)
    public ApiType ApiType { get; init; } = ApiType.REST;
}

// User's project — create once, reference everywhere
public class MyApiConfig : ServerFunctionConfiguration
{
    public MyApiConfig()
    {
        BaseRoute = "api/v1";
        RouteNaming = RouteNaming.KebabCase;
        HttpVersion = "2.0";
    }
}

// Interface uses it
[ServerFunctionCollection(Configuration = typeof(MyApiConfig))]
public interface IUserService { ... }

// Another interface inherits a variant
public class MyAdminConfig : MyApiConfig
{
    public MyAdminConfig() { /* inherits all of MyApiConfig's settings */ }
}

[ServerFunctionCollection(RequireAuthorization = true, Configuration = typeof(MyAdminConfig))]
public interface IAdminService { ... }
```

**Configuration priority (highest wins):**
```
[ServerFunction(...)] property   ← highest
[ServerFunctionCollection(Configuration = typeof(...))]
Generator defaults               ← lowest
```

No JSON files, no MSBuild properties — everything lives in C# with full IDE support.

### Settings catalogue

| Setting | Values | Default |
|---|---|---|
| `BaseRoute` | any string | `"api/functions"` |
| `RouteNaming` | `PascalCase`, `CamelCase`, `KebabCase`, `SnakeCase` | `PascalCase` |
| `DefaultHttpMethod` | `"GET"`, `"POST"`, etc. | null (must be explicit) |
| `HttpVersion` | `"1.1"`, `"2.0"`, `"3.0"` | `"1.1"` |
| `ApiType` | `REST`, `gRPC` | `REST` |
| `GenerateProblemDetails` | `true`, `false` | `true` |
| `EnableResilience` | `true`, `false` | `false` |
| `Nullable` | `true`, `false` | `true` |
| `CustomHttpClientType` | `typeof(MyHttpClient)` — must extend `HttpClient` | null |

### Implementation tasks

| # | Item | Size |
|---|---|---|
| 2.A | Add `ServerFunctionConfiguration`, `RouteNaming`, `ApiType` enums to Abstractions | 🟢 |
| 2.B | Add `Configuration = typeof(...)` property to `[ServerFunctionCollection]` | 🟢 |
| 2.C | Generator reads config type via Roslyn and merges with per-interface/method values | 🟡 |
| 2.D | `BaseRoute` config applied in client proxy + server endpoint generators | 🟢 |
| 2.E | `RouteNaming` convention applied to route segments | 🟢 |
| 2.F | `HttpVersion` sets `HttpClient.DefaultRequestVersion` in generated DI registration | 🟢 |
| 2.G | `CustomHttpClientType` changes `AddHttpClient<IFoo, FooClient>` → `AddHttpClient<IFoo, FooClient, CustomType>` | 🟢 |

---

## 3. HTTP transport — feature gaps

| # | Item | Size | Notes |
|---|---|---|---|
| 3.1 | **Route/path parameters** | 🟡 | `[ServerFunction(Route = "users/{id}")]` → bind `{id}` from route on server, interpolate in URL on client |
| 3.2 | **`IAsyncEnumerable<T>` streaming** | 🔴 | Server: chunked JSON via `Results.Stream`; Client: `GetFromJsonAsAsyncEnumerable<T>()` |
| 3.3 | **File upload** | 🟡 | `Stream` / `IFormFile` params → multipart on client, `[FromForm]` on server |
| 3.4 | **OpenAPI metadata** | 🟡 | Emit `.WithName()`, `.WithTags()`, `.WithOpenApi()`, `Produces<T>()` on generated endpoints |
| 3.5 | **Response/output caching** | 🟢 | `[ServerFunction(CacheSeconds = 30)]` → `.CacheOutput(...)` |
| 3.6 | **Rate limiting** | 🟢 | `[ServerFunction(RateLimitPolicy = "fixed")]` → `.RequireRateLimiting(...)` |
| 3.7 | **API versioning** | 🟡 | `[ServerFunctionCollection(Version = "v2")]` → route prefix `/api/v2/...` |

---

## 4. Security & auth

| # | Item | Size | Notes |
|---|---|---|---|
| 4.1 | **Named authorization policies** | 🟢 | `[ServerFunction(Policy = "AdminOnly")]` → `.RequireAuthorization("AdminOnly")` |
| 4.2 | **Role-based auth** | 🟢 | `[ServerFunction(Roles = "Admin,Manager")]` |
| 4.3 | **CORS per interface** | 🟡 | `[ServerFunctionCollection(CorsPolicy = "AllowedOrigins")]` → `.RequireCors(...)` on the route group |
| 4.4 | **Anti-forgery** | 🟢 | `[ServerFunction(RequireAntiForgery = true)]` → `.ValidateAntiforgery()` |
| 4.5 | **Endpoint filters** | 🟡 | `[ServerFunction(Filter = typeof(MyFilter))]` → `.AddEndpointFilter<MyFilter>()` on the generated minimal API endpoint; supports multiple filters via array |

> **Delegating handlers** are runtime factory lambdas (`() => new MyHandler()`) — the Roslyn generator cannot read or embed these, so they stay as the `configureClient` hook on `AddServerFunctionClients`.
>
> **`CustomHttpClientType`** works in settings because it is a `Type` reference (`typeof(MyHttpClient)`), not a factory. The generator reads it as a Roslyn `ITypeSymbol` at compile time and emits the type name into the generated proxy constructor (`public FooClient(MyHttpClient client)`) — no runtime instantiation needed.
>
> **Endpoint filters** are the server-side equivalent: applied at code-gen time via `[ServerFunction(Filter = typeof(...))]`.
>
> Note: idempotency keys are already handled by the `configureClient` delegating handler hook — no generator feature needed.

---

## 5. Production readiness

| # | Item | Size | Notes |
|---|---|---|---|
| 5.1 | **Distributed tracing** | 🟡 | Auto-emit `Activity` start/stop in generated **server endpoints only** (WASM runs client-side, no server Activity needed); integrates with OpenTelemetry |
| 5.2 | **Structured logging** | 🟢 | Generated server endpoints log request/response at `Debug` via `ILogger<T>` |
| 5.3 | **Input validation** | 🟡 | Generated server endpoints validate request DTOs using `Validator.TryValidateObject` (built-in `System.ComponentModel.DataAnnotations` — `[Required]`, `[Range]`, `[StringLength]`, etc.); return 400 with validation errors as Problem Details |
| 5.4 | **Health checks** | 🟡 | `MapServerFunctionHealthChecks()` — verifies all registered service implementations resolve |
| 5.5 | **Built-in resilience** | 🟢 | `ServerFunctionConfiguration.EnableResilience = true` (or override per-interface) → auto-applies `AddStandardResilienceHandler()` in generated `AddServerFunctionClients`; can also be toggled at registration time via `AddServerFunctionClients(..., resilience: true)` |
| 5.6 | **Code fix providers** | 🟡 | IDE quick-fixes for every BSF diagnostic (already have the errors, add the fix actions) |
| 5.7 | **Benchmark tests** | 🟡 | Measure incremental generator performance with BenchmarkDotNet; catch compile-time regressions |

---

## 6. gRPC transport — code-first

Generate `.proto` + gRPC service/client from the same `[ServerFunctionCollection]` interface.
Activated via `[ServerFunctionCollection(Configuration = typeof(MyGrpcConfig))]` where `ApiType = ApiType.gRPC`.

| # | Item | Size | Notes |
|---|---|---|---|
| 6.1 | **Proto file generator** | 🔴 | Walk `InterfaceInfo` → emit `.proto` (message types from DTOs, rpc from methods) |
| 6.2 | **gRPC server generator** | 🔴 | `XxxServiceGrpcImpl : XxxServiceBase` delegating to `IXxxService` |
| 6.3 | **gRPC client proxy** | 🔴 | `XxxServiceGrpcClient : IXxxService` using `GrpcChannel` + generated stub |
| 6.4 | **gRPC DI registration** | 🟡 | `AddServerFunctionGrpcClients(channel)` + `MapServerFunctionGrpcServices()` |
| 6.5 | **Streaming via IAsyncEnumerable** | 🔴 | Map `IAsyncEnumerable<T>` → gRPC server-streaming rpc |
| 6.6 | **Auth over gRPC** | 🟡 | Bearer tokens via `CallCredentials`; integrates with `configureClient` hook |

---

## 7. gRPC transport — manual `.proto`

Reverse: user provides `.proto` → generator produces typed C# interface + client + server.

| # | Item | Size | Notes |
|---|---|---|---|
| 7.1 | **Proto file parser** | 🔴 | Read `.proto` as `AdditionalFile`; parse service + message definitions |
| 7.2 | **Interface generator from proto** | 🔴 | Emit `[ServerFunctionCollection]` interface + DTOs from proto |
| 7.3 | **Round-trip validation** | 🟡 | code-first → proto → code-first must produce equivalent types |

---

## Current status

- [x] All 18 BSF diagnostics implemented and tested
- [x] HTTP/REST transport (GET/POST/PUT/PATCH/DELETE)
- [x] Interface-level + method-level `RequireAuthorization`
- [x] `configureClient` hook (auth, resilience, logging delegating handlers)
- [x] Problem Details error body in `HttpRequestException.Message`
- [x] Full test suite: 72 unit + 31 integration + 34 E2E
- [x] Sample app with Server/WASM/Auto demo pages for all 4 services
- [ ] Everything above

---

## Suggested session order

Make sure to update the documentation from section §1 whenever you add a new feature and mark it as done below with a checkbox.

- [x] **§1** — README + versioning (quick wins, needed before public release)
- [ ] **§2.A–2.G** — Configuration class system (foundation for all future features)
- [ ] **§3.1** — Route/path parameters (biggest HTTP gap)
- [ ] **§4.1 + §4.2** — Named policies + roles (rounds out auth)
- [ ] **§5.1–5.3** — Tracing + logging + validation (production readiness)
- [ ] **§3.4** — OpenAPI metadata (discoverability)
- [ ] **§6** — gRPC code-first (major transport addition)
