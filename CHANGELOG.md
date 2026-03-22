# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
