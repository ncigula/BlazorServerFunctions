# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.4.0] - 2026-03-19

### Added

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
