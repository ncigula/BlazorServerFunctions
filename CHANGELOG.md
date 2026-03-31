# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.13.0] - 2026-03-31

### Added

- **OpenAPI customization per method** — five new optional properties on `[ServerFunction]` for per-endpoint OpenAPI metadata:
  - `Summary` (`string?`) — emits `.WithSummary("...")` on the generated endpoint; the short label shown in Swagger UI
  - `Description` (`string?`) — emits `.WithDescription("...")` on the generated endpoint; supports Markdown
  - `Tags` (`string[]?`) — overrides the auto-generated `.WithTags(interfaceName)` call; use to assign one or more custom tag groups
  - `ProducesStatusCodes` (`int[]?`) — emits additional `.Produces(statusCode)` calls alongside the existing `.Produces<T>(200)` annotation; use to document 404, 409, etc.
  - `ExcludeFromOpenApi` (`bool`) — emits `.ExcludeFromDescription()` instead of `.WithOpenApi()`, hiding the endpoint from OpenAPI documentation
- All new properties are optional; omitting them leaves existing behaviour unchanged

### Tests

- 8 new unit snapshot tests in `ServerGeneratorTests.cs` covering all 5 properties individually and in combination

---

## [0.12.0] - 2026-03-25

### Added

- **Result Mapper — library-agnostic `Result<T>` / discriminated union support**
  - **New `IServerFunctionResultMapper<TResult, TValue>` interface** (in `BlazorServerFunctions.Abstractions`, targets `netstandard2.0`) — implement once for any result wrapper type (`Result<T>`, `Result<T, TError>`, `OneOf<A, B>`, custom types) to teach the generator how to unwrap success values and convert failures to HTTP error responses:
    - `bool IsSuccess(TResult result)` — decides which branch to take
    - `TValue GetValue(TResult result)` — extracts the success payload (called only when `IsSuccess` is true)
    - `ServerFunctionError GetError(TResult result)` — extracts error details for the ProblemDetails response
    - `TResult WrapValue(TValue value)` — re-wraps a success value (used on the client proxy side)
    - `TResult WrapFailure(ServerFunctionError error)` — re-wraps a failure from the HTTP error response (used on the client proxy side)
  - **New `ServerFunctionError` DTO** (in `BlazorServerFunctions.Abstractions`) — lightweight RFC 9457–shaped error type with `Status` (int), `Title`, `Detail`, and `Type` string properties; no ASP.NET Core or `Microsoft.AspNetCore.Mvc` dependency, fully usable from Blazor WASM
  - **`ResultMapper` property on `[ServerFunctionCollection]`** — accepts an open-generic mapper type (e.g. `typeof(ResultMapper<>)` or `typeof(ResultMapper<,>)`); the generator closes the type arguments from each method's return type at code-generation time
  - **Server endpoint code-generation** — when `ResultMapper` is set, the generated lambda instantiates `new MapperType<T>()` and calls `IsSuccess` / `GetValue` / `GetError` to emit either `Results.Ok(value)` or `Results.Problem(...)` without any server-specific base class or helper method; `.Produces<TValue>()` annotation uses the inner value type, not the wrapper
  - **Client proxy code-generation** — on 2xx responses the proxy calls `new MapperType<T>().WrapValue(value)` (deserialising the inner type from JSON); on 4xx/5xx it reads the response body as a string, uses `System.Text.Json.JsonDocument` to parse the ProblemDetails fields, and calls `new MapperType<T>().WrapFailure(error)` — no `Microsoft.AspNetCore.Mvc` reference required in WASM projects
  - **Void / streaming exclusions** — methods with `void` / `Task` return types silently skip the mapper and emit `Results.Ok()` as before; `IAsyncEnumerable<T>` streaming methods also skip the mapper without a diagnostic
  - **New diagnostics:**
    - **BSF029** (error) — `ResultMapper` set on a gRPC interface; result mapping is REST-only
    - **BSF030** (warning) — a method in a `ResultMapper` collection has a non-generic return type; the mapper cannot be applied and falls back to `Results.Ok(result)`, which will likely serialize the wrapper object rather than its inner value
  - **Sample** — `Result<T>`, `ResultMapper<T>`, and `IResultDemoService` added to `BlazorServerFunctions.Sample.Shared`; in-memory `ResultDemoService` (server) demonstrates 404-not-found and 409-conflict scenarios; Blazor Server (`/demos/result-mapper/server`) and WASM (`/demos/result-mapper/wasm`) demo pages added; nav entries wired

### Tests

- 8 new unit snapshot tests in `ResultMapperGeneratorTests.cs`: single type-arg client/server, two type-arg client/server, void method skip, streaming method skip, mixed methods, and a combined test
- 2 new unit diagnostic tests: `BSF029` on gRPC interface, `BSF030` on non-generic return type

---

## [0.11.0] - 2026-03-25

### Added

- **§1 File upload — `Stream` / `IFormFile` / `IFormFileCollection` parameter support**
  - **Client proxy** — generates `MultipartFormDataContent` automatically: `Stream` parameters become a named `StreamContent` part; `IFormFile` parameters wrap `.OpenReadStream()` with `FileName` preserved; `IFormFileCollection` emits one part per file; regular parameters alongside files are serialised as `StringContent` form fields
  - **Server endpoint** — binds via inline per-parameter `IFormFile` / `[FromForm]` declarations (no generated DTO record); `.DisableAntiforgery()` is added automatically so global `UseAntiforgery()` middleware does not silently reject multipart requests
  - **New `FileKind` enum** (`None` / `Stream` / `FormFile` / `FormFileCollection`) — internal discriminated type that drives both client and server code-generation paths
  - **New diagnostics:**
    - **BSF026** (error) — file parameter on a GET or DELETE method; multipart form data requires POST, PUT, or PATCH
    - **BSF027** (error) — file parameter combined with `IAsyncEnumerable<T>` return; multipart upload and streaming response cannot be combined
    - **BSF028** (error) — file parameter on a gRPC interface; file upload is REST-only
  - **Sample** — `IFileUploadService` with `UploadAsync(Stream file, string fileName)` and `UploadStreamOnlyAsync(Stream file)`; Blazor Server and WASM demo pages added; nav entries wired

### Tests

- 8 new unit snapshot tests (4 client, 4 server) covering `Stream`, `IFormFile`, `IFormFileCollection`, and mixed file + regular parameter combinations
- 4 new integration diagnostic tests for BSF026, BSF027, and BSF028
- 4 new E2E round-trip tests: byte-count verification, empty stream, stream-only endpoint, 100 KB payload

---

## [0.10.0] - 2026-03-23

### Added

- **§6.5 gRPC authentication** — `[Authorize]` attributes are now emitted on generated gRPC service classes and methods, and `.RequireAuthorization()` is chained on `MapGrpcService<T>()` registrations:
  - Interface-level `[ServerFunctionCollection(RequireAuthorization = true)]` → `[global::Microsoft.AspNetCore.Authorization.Authorize]` on the generated service class + `.RequireAuthorization()` on `MapGrpcService<T>()`
  - Method-level `[ServerFunction(Policy = "AdminOnly")]` → `[Authorize(Policy = "AdminOnly")]` on the service method
  - Method-level `[ServerFunction(Roles = "Admin")]` → `[Authorize(Roles = "Admin")]` on the service method
  - Method-level `[ServerFunction(RequireAuthorization = true)]` (no role/policy) → bare `[Authorize]` on the service method
  - Interface-level and method-level auth are independent and composable (class-level attribute + method-level attribute are both emitted when both are set)
- **`innerGrpcHttpHandler` parameter on `AddServerFunctionClients`** — when one or more gRPC interfaces are registered, the generated `AddServerFunctionClients` extension gains an `HttpMessageHandler? innerGrpcHttpHandler = null` parameter. Passing a `DelegatingHandler` here (e.g. one that injects `Authorization: Bearer <token>`) wraps the gRPC channel's inner transport, enabling token-based auth for gRPC-Web clients without requiring `CallCredentials` or `UnsafeUseInsecureChannelCallCredentials`.

### Changed

- **Sample — multi-scheme authentication** — the sample server now runs Cookie auth and JWT Bearer auth side-by-side via a `PolicyScheme` that routes to the correct handler based on the presence of an `Authorization` request header:
  - Browser sessions continue to use cookie auth (no change to existing behaviour)
  - API clients (including the new E2E JWT tests) send `Authorization: Bearer <token>` and are validated by `JwtBearer`
  - Login endpoints (`/demos/admin/login`, `/demos/admin/login/admin`) now return `{ "token": "..." }` in the JSON body in addition to setting the session cookie — existing cookie-based tests are unaffected
  - Logout endpoint no longer redirects; returns `200 OK`
- **Sample — demo pages show 3 auth tiers** — both the Admin WASM page (`/demos/admin/wasm`) and the gRPC demo page (`/demos/grpc`) now visually distinguish three user states (Not authenticated / User / Admin) with a live session badge and in-place data refresh on login/logout. `forceLoad: true` navigation (which triggered a full WASM runtime reload) replaced with in-place `LoadAsync()` calls; Blazor WASM's browser-managed cookie jar makes reload unnecessary.
- **`IGrpcDemoService` — new `GetUserSecretAsync()` method** — adds a "plain user" (any authenticated, no role) protected gRPC method alongside the existing AdminOnly `GetSecretAsync()`, enabling the three-tier auth demo on the gRPC page.

### Tests

- **`BearerTokenHandler`** — new `DelegatingHandler` (in `tests/BlazorServerFunctions.EndToEndTests/Fixtures/`) that injects `Authorization: Bearer <token>` on every outgoing request. Symmetric across REST (`HttpClient`) and gRPC (`GrpcChannel.HttpHandler`).
- **`JwtAuthClientTests`** — 6 new E2E tests exercising JWT Bearer auth end-to-end:
  - REST (`IAdminService.GetPolicySecretAsync`, `Policy = "AdminOnly"`): no token → 401, plain user token → 403, admin token → 200
  - gRPC (`IGrpcDemoService.GetSecretAsync`, `Policy = "AdminOnly"`): no token → `Unauthenticated`, plain user token → `PermissionDenied`, admin token → success
- **`GrpcAuthServiceClientTests`** — cookie-based gRPC auth tests (unauthenticated / plain user / admin via session cookie).
- **Collection fixtures** — `E2ECollection` and `ServerCollection` introduced; all 13 `IClassFixture<E2EFixture>` test classes and 7 `IClassFixture<WebApplicationFactory<Program>>` test classes migrated. Reduces server startups from 20 to 2 per test run.

---

## [0.9.0] - 2026-03-22

### Added

- **§6.2 gRPC client proxy generator** — Code-first gRPC client proxy generation. Any `[ServerFunctionCollection]` interface with `ApiType = ApiType.GRPC` now generates:
  - A `[ProtoContract]` request wrapper type per method with parameters
  - A `public I{ServiceName}GrpcContract` interface carrying `[ServiceContract]` and `[OperationContract]` annotations (the wire contract shared between client and server)
  - A `{ServiceName}GrpcClient` class implementing the BSF interface and delegating to the contract interface via `GrpcChannel.CreateGrpcService<T>()`
  - Consuming projects that declare the BSF interface must reference `protobuf-net.Grpc` (≥1.2.2) and `System.ServiceModel.Primitives` (≥8.1.2)
- **§6.3 DI registration extension** — `AddServerFunctionClients()` now registers gRPC clients alongside REST clients:
  - REST interfaces: registered via `AddHttpClient<IXxx, XxxClient>()`
  - gRPC interfaces: `GrpcChannel` registered as `TryAddSingleton` (allows tests to pre-register their own channel); each gRPC interface registered as `AddTransient<IXxx, XxxGrpcClient>()`
  - `baseAddress` parameter is required when at least one gRPC interface is registered (validated at runtime)
- **§6.4 gRPC server-streaming** — `IAsyncEnumerable<T>` return types are fully supported on both client and server. protobuf-net.Grpc maps server-streaming methods automatically.
- **`ApiType` shortcut property** — `[ServerFunctionCollection(ApiType = ApiType.GRPC)]` can now be used directly on the attribute instead of requiring a separate `Configuration` class
- **`MapServerFunctionEndpoints()` update** — the generated `ServerFunctionEndpointsRegistration` now calls `endpoints.MapGrpcService<{ServiceName}GrpcService>()` for each gRPC interface alongside the existing REST endpoint mappings

### Changed

- **`ClientProxyGenerator` renamed to `RestClientProxyGenerator`** and **`ServerEndpointGenerator` renamed to `RestServerEndpointGenerator`** — internal names clarified to reflect that they only handle REST (HTTP) interfaces
- **gRPC service class no longer carries `[ServiceContract]`** — the attribute lives on the shared contract interface (`I{ServiceName}GrpcContract`) so that client and server share the same wire contract definition, eliminating service-name mismatches
- Request wrapper types (`{ServiceName}{MethodName}GrpcRequest`) are now emitted **only once** (in the shared/library project via the client proxy generator); the server generator skips re-emitting them when they come from a referenced assembly, preventing CS0436 duplicate-type conflicts

## [0.8.0] - 2026-03-22

### Added

- **§6.1 gRPC server generator** — Code-first gRPC service class generation using [protobuf-net.Grpc](https://github.com/protobuf-net/protobuf-net.Grpc). No `.proto` files required.
  - Any `[ServerFunctionCollection]` interface configured with `ApiType = ApiType.GRPC` generates an `XxxGrpcService` class decorated with `[ServiceContract]`, delegating every operation to the injected `IXxxService`
  - Methods with ≥1 parameter get a generated `[ProtoContract]` request wrapper type (`{ServiceName}{MethodName}GrpcRequest`) — gRPC transport requires a single message per operation
  - Methods with zero parameters receive only a `CallContext context = default` argument (no wrapper generated)
  - `CancellationToken` parameters are stripped from the request type and forwarded as `context.CancellationToken`
  - `IAsyncEnumerable<T>` return types are supported — protobuf-net.Grpc maps these to gRPC server-streaming automatically
  - The consuming server project must reference `protobuf-net.Grpc.AspNetCore` (same pattern as ASP.NET Core itself for REST endpoints)
- **§6.6 gRPC diagnostics** — Three new diagnostics guard REST-only attributes on gRPC interfaces:
  - **BSF023** (error) — `[ServerFunction(HttpMethod = "...")]` on a gRPC method: `HttpMethod` has no effect on gRPC (transport always uses HTTP POST) and must be removed
  - **BSF024** (warning) — `CacheSeconds > 0` on a gRPC method: output caching is not supported for gRPC and will be ignored
  - **BSF025** (warning) — `RequireAntiForgery = true` on a gRPC method: anti-forgery tokens are not supported for gRPC and will be ignored
  - BSF012 (`HttpMethod` required) is suppressed for gRPC interfaces — gRPC methods do not need an `HttpMethod` attribute

---

## [0.7.0] - 2026-03-22

### Added

- **§5.7 Benchmark tests** — `BlazorServerFunctions.Benchmarks` project measuring incremental generator performance with [BenchmarkDotNet](https://benchmarkdotnet.org/):
  - `FullGenerationBenchmarks` — cold-start pipeline time parameterized by interface count (1 / 5 / 20); shows scaling behaviour and allocation cost
  - `IncrementalGenerationBenchmarks` — re-run time after an unrelated edit; a near-zero result confirms the `IIncrementalGenerator` pipeline's caching is working correctly
  - Run locally: `dotnet run -c Release --project tests/BlazorServerFunctions.Benchmarks`
- **GitHub Actions CI** — two workflows in `.github/workflows/`:
  - `ci.yml` — builds the solution and runs all unit / integration / E2E tests on every push and pull request; test results uploaded as a TRX artifact
  - `benchmarks.yml` — runs on every push to `master`; exports BenchmarkDotNet JSON and stores historical results via `github-action-benchmark` in the `gh-pages` branch (trend charts at `https://<owner>.github.io/<repo>/dev/bench/`); alerts and fails when a benchmark regresses by more than 150%
- `benchmarks/BENCHMARKS.md` — how-to guide for running locally, interpreting output, and updating committed baselines

---

## [0.6.0] - 2026-03-22

### Added

- **§5.6 Code fix providers** — `Ctrl+.` quick-fix actions in the IDE for six diagnostics:
  - **BSF003** `Make interface public` — adds `public` modifier (replaces `internal`/`private` if present)
  - **BSF012** `Set HttpMethod = "VERB"` — adds the missing `HttpMethod` argument to `[ServerFunction]`; one action per valid verb (GET, POST, PUT, DELETE, PATCH)
  - **BSF013** `Change to HttpMethod = "VERB"` — replaces an invalid verb with a valid one; one action per valid verb
  - **BSF020** `Remove CacheSeconds (not valid on non-GET endpoints)` — removes the `CacheSeconds` argument from `[ServerFunction]`
  - **BSF021** `Remove empty Roles property` — removes `Roles = ""` from `[ServerFunction]`
  - **BSF022** `Remove empty CorsPolicy property` — removes `CorsPolicy = ""` from `[ServerFunctionCollection]`
  - Supported in Visual Studio, VS Code (C# Dev Kit), and JetBrains Rider

---

## [0.5.0] - 2026-03-22

### Added

- **§5.4 Health checks** — `AddServerFunctionHealthChecks()` auto-registers an `IHealthCheck` for every BSF-managed service interface using a generated `__BsfResolveCheck<T>` that opens a DI scope and calls `GetRequiredService<T>()`; `MapServerFunctionHealthChecks(pattern)` maps a filtered health endpoint (default: `/health/server-functions`) showing only BSF checks via the `"bsf"` tag; new services appear automatically with no manual maintenance required

### Removed

- **`ServerFunctionConfiguration.EnableResilience`** — unused config property removed; use the `configureClient` hook on `AddServerFunctionClients(configureClient: b => b.AddStandardResilienceHandler())` instead

---

## [0.4.0] - 2026-03-20

### Added

- **§4.5 Endpoint filters** — `[ServerFunction(Filters = new[] { typeof(MyFilter) })]` emits `.AddEndpointFilter<MyFilter>()` on the generated minimal API endpoint; multiple filter types are applied in declaration order; each type must implement `IEndpointFilter`
- **§4.4 Anti-forgery** — `[ServerFunction(RequireAntiForgery = true)]` adds `.WithMetadata(new RequireAntiforgeryTokenAttribute())` to the generated minimal API endpoint; requires `builder.Services.AddAntiforgery()` and `app.UseAntiforgery()` in the server pipeline
- **§4.3 CORS per interface** — `[ServerFunctionCollection(CorsPolicy = "AllowedOrigins")]` emits `group.RequireCors("AllowedOrigins")` on the route group for all endpoints in the collection; `ServerFunctionConfiguration.CorsPolicy` sets a collection-level config default (attribute overrides it); BSF022 error when `CorsPolicy` is set to an empty string; requires `builder.Services.AddCors(...)` and `app.UseCors()` in the server pipeline

### Fixed

- **`ConfigManifestGenerator` missing `__Policy` field** — `ServerFunctionConfiguration.Policy` was silently lost in cross-compilation scenarios (shared library referenced by server/client projects) because the manifest emitter omitted the `__Policy` const field while the manifest reader expected it

---

## [0.3.0] - 2026-03-19

### Added

- **§4.1 Named authorization policies** — `[ServerFunction(Policy = "AdminOnly")]` emits `.RequireAuthorization("AdminOnly")` on the endpoint; `ServerFunctionConfiguration.Policy` sets a collection-level default; per-method value overrides config (`""` = explicitly disable); can be combined with the boolean `RequireAuthorization` — named policy is applied in addition to any group-level `.RequireAuthorization()` on the route group
- **§4.2 Role-based auth** — `[ServerFunction(Roles = "Admin,Manager")]` emits `.RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Manager" })` on the endpoint; can be combined with `Policy` and the boolean `RequireAuthorization` (all are ANDed by ASP.NET Core); BSF021 error when `Roles` is set to an empty string

---

## [0.2.0] - 2026-03-19

### Added

- **§3.1 Route/path parameters** — `[ServerFunction(Route = "items/{id}")]` binds `{id}` from the URL on the server and interpolates it into the request URL on the client; BSF015 (invalid route format), BSF016 (duplicate route), BSF017 (route parameter not found in method parameters), BSF018 (complex type used as route parameter)
- **§3.2 `IAsyncEnumerable<T>` streaming** — server methods can return `IAsyncEnumerable<T>` for chunked JSON streaming; server generator emits a synchronous lambda that returns the stream directly (ASP.NET Core handles chunking); client proxy uses `HttpCompletionOption.ResponseHeadersRead` + `ReadFromJsonAsAsyncEnumerable<T>()` + `[EnumeratorCancellation]`; BSF007 updated to accept `IAsyncEnumerable<T>` as a valid return type
- **§3.4 OpenAPI metadata** — generated server endpoints always emit `.WithTags("{InterfaceName}")` and `.Produces<T>(200)` / `.Produces(200)` / `.ProducesProblem(500)`; `.WithOpenApi()` is added automatically when `Microsoft.AspNetCore.OpenApi` is referenced in the project
- **§3.5 Output caching** — `[ServerFunction(CacheSeconds = N)]` emits `.CacheOutput(p => p.Expire(TimeSpan.FromSeconds(N)))` on the endpoint; `ServerFunctionConfiguration.CacheSeconds` sets a collection-level default; per-method value overrides config (`0` = explicitly disable); BSF019 (warning: `CacheSeconds` on streaming method), BSF020 (error: `CacheSeconds` on non-GET method)
- **§3.6 Rate limiting** — `[ServerFunction(RateLimitPolicy = "policyName")]` emits `.RequireRateLimiting("policyName")` on the endpoint; `ServerFunctionConfiguration.RateLimitPolicy` sets a collection-level default; per-method value overrides config (`""` = explicitly disable); valid on any HTTP method and any return type; user is responsible for registering the named policy via `builder.Services.AddRateLimiter(...)`
- **2 new BSF diagnostics**: BSF019 (warning), BSF020 (error) for invalid output-cache usage

### Fixed

- `ReadFromJsonAsync<T?>()` — generated client proxies now always use the nullable type argument (e.g. `ReadFromJsonAsync<int?>()`) so that the `?? throw` null-guard compiles correctly for value types in .NET 10, where the non-nullable overload returns `T` directly rather than `T?`

### Changed

- Test suite expanded: **116 unit tests**, **31 integration tests**, **56 E2E tests** (up from 72 / 31 / 34)

---

## [0.1.0] - 2026-03-18

### Added

- Roslyn incremental source generator that produces HTTP client proxies and ASP.NET Core minimal API endpoints from annotated C# interfaces
- `[ServerFunctionCollection]` attribute — marks an interface for code generation; supports `RoutePrefix` and `RequireAuthorization`
- `[ServerFunction]` attribute — marks a method for generation; supports `HttpMethod`, `Route`, and `RequireAuthorization`
- **HTTP transport** — full support for GET, POST, PUT, PATCH, DELETE
- **Client proxy generator** — `{Interface}Client.g.cs` implementing the interface via `HttpClient`
- **Server endpoint generator** — `{Interface}ServerExtensions.g.cs` mapping each method to a minimal API endpoint
- **Client DI registration** — `AddServerFunctionClients(baseAddress, configureClient)` extension on `IServiceCollection`
- **Server DI registration** — `MapServerFunctionEndpoints()` extension on `IEndpointRouteBuilder`
- **Project type detection** — automatically generates server endpoints only in server projects (detects `IEndpointRouteBuilder`); generates client-only in WASM/library projects
- **Authorization** — interface-level and method-level `RequireAuthorization`; `configureClient` hook for attaching delegating handlers (JWT bearer, cookies, etc.)
- **Problem Details** — server errors surfaced as RFC 9457 Problem Details; message forwarded in `HttpRequestException`
- **CancellationToken** support — excluded from request DTOs, forwarded to the underlying service call
- **18 BSF diagnostics** (BSF001–BSF102) covering all invalid usage patterns with clear error messages
- Full test suite: 72 unit tests, 31 integration tests, 34 E2E tests
- Sample Blazor app (Server / WASM / Auto render modes) with Aspire orchestration
