# BlazorServerFunctions — Feature Roadmap

## Size legend
- 🟢 Small — a few hours
- 🟡 Medium — a day or two
- 🔴 Large — multiple days / architectural work

---

## Completed features

<details>
<summary>§1 — Packaging & discoverability ✅</summary>

| # | Item |
|---|---|
| 1.1 | README.md — Quick-start, attribute reference, DI setup, auth + JWT example, sample link |
| 1.2 | NuGet versioning — `<PackageVersion>`, `<PackageDescription>`, `<PackageReleaseNotes>` |
| 1.3 | Package metadata — Icon, license, repo URL, tags |
| 1.4 | Source Link — `<PublishRepositoryUrl>` + `<EmbedUntrackedSources>` so consumers can step into generated code |
| 1.5 | CHANGELOG.md |

</details>

<details>
<summary>§2 — Configuration system ✅</summary>

Type-based configuration via `ServerFunctionConfiguration` subclass. Settings: `BaseRoute`, `RouteNaming`, `DefaultHttpMethod`, `HttpVersion`, `ApiType`, `GenerateProblemDetails`, `Nullable`, `CustomHttpClientType`, `CacheSeconds`, `RateLimitPolicy`, `Policy`, `CorsPolicy`. Reads constructor assignments at compile time via Roslyn. Cross-compilation manifest (`__BsfConfig_*`) for referenced assemblies.

</details>

<details>
<summary>§3 — HTTP transport (partial) ✅</summary>

| # | Item |
|---|---|
| 3.1 | Route/path parameters — `{id}` syntax, constraint stripping, route/query/body inference |
| 3.2 | `IAsyncEnumerable<T>` streaming — chunked JSON via `Results.Stream`; `GetFromJsonAsAsyncEnumerable<T>()` on client |
| 3.4 | OpenAPI metadata — `.WithTags()`, `.Produces<T>()`, `.ProducesProblem(500)`, auto-detected `.WithOpenApi()` |
| 3.5 | Response/output caching — `[ServerFunction(CacheSeconds = 30)]`, collection default, BSF019/BSF020 guards |
| 3.6 | Rate limiting — `[ServerFunction(RateLimitPolicy = "fixed")]`, collection default |

</details>

<details>
<summary>§4 — Security & auth ✅</summary>

Named policies (`Policy`), role-based auth (`Roles`), CORS per interface (`CorsPolicy`), anti-forgery (`RequireAntiForgery`), endpoint filters (`Filters`). BSF021/BSF022 diagnostics for empty strings.

</details>

<details>
<summary>§5 — Production readiness ✅</summary>

Health checks (`AddServerFunctionHealthChecks()` + `MapServerFunctionHealthChecks()`), code fix providers (6 IDE quick-fixes), BenchmarkDotNet benchmark suite.

</details>

<details>
<summary>§6 — gRPC transport ✅</summary>

Code-first gRPC via protobuf-net.Grpc. Server service class, client proxy, shared contract interface, request wrappers, `IAsyncEnumerable<T>` server-streaming, auth, DI registration. BSF023/BSF024/BSF025 diagnostics for REST-only concepts on gRPC interfaces.

</details>

<details>
<summary>§8 — CI/CD ✅</summary>

GitHub Actions: `ci.yml` (build + test on push/PR), `benchmarks.yml` (matrix per benchmark class, BenchmarkDotNet → gh-pages chart), `release.yml` (tag-triggered NuGet publish). Live benchmark chart at https://ncigula.github.io/BlazorServerFunctions/dev/bench/.

</details>

---

## Active roadmap (sorted by value)

---

### §1 — File upload  `[ServerFunction]` with `Stream` / `IFormFile` ✅
**Size:** 🟡 &nbsp; **Value:** 🔥 High

`Stream`, `IFormFile`, and `IFormFileCollection` parameters are fully supported. Client proxies build `MultipartFormDataContent`; server endpoints bind via `IFormFile` / `[FromForm]` inline parameters. Diagnostics BSF026/027/028 guard invalid combinations.

**Delivered:**
- **Client side:** `MultipartFormDataContent` with `StreamContent` per file; regular params as `StringContent` form fields
- **Server side:** Inline lambda parameters (`IFormFile`/`IFormFileCollection`/`[FromForm]`); no DTO record; `.DisableAntiforgery()` on file upload endpoints
- **New diagnostics:** BSF026 (file param on GET/DELETE), BSF027 (file + streaming return), BSF028 (file on gRPC)
- **Tests:** 8 unit snapshot tests, 4 integration diagnostic tests, 4 E2E round-trip tests

---

### §2 — Result\<T\> converter ~~cancelled~~
**Size:** 🟡 &nbsp; **Value:** 🔸 Medium → ~~Won't implement~~

The problem this was meant to solve (MediatR / discriminated union results returning HTTP errors) is already solved cleanly without any generator changes:

1. The `IXxxService` interface is a thin adapter — implement it on the server, call `mediator.Send(...)`, unwrap the result, and throw a typed domain exception on failure (2–3 lines per method).
2. A single `IExceptionHandler` registered in `Program.cs` maps domain exceptions to `ProblemDetails` responses centrally, once, for all endpoints.

Adding `IServerFunctionResultConverter<T>` + `[ServerFunction(ResultConverter = ...)]` + BSF029/030 diagnostics would impose generator complexity and a new abstraction to learn for a problem that the existing ASP.NET Core exception-handling pipeline already solves. The thin service wrapper is the right boundary; the generator has no business knowing about result types. **Documented in README under "Using with MediatR".**

---

### §3 — OpenAPI customization per method ✅
**Size:** 🟡 &nbsp; **Value:** 🔸 Medium

**Delivered:**
- `Summary` (`string?`) → `.WithSummary("...")` — short Swagger UI label
- `Description` (`string?`) → `.WithDescription("...")` — long description, supports Markdown
- `Tags` (`string[]?`) → overrides auto-generated `.WithTags(interfaceName)`
- `ProducesStatusCodes` (`int[]?`) → additional `.Produces(404)` etc. alongside `.Produces<T>(200)`
- `ExcludeFromOpenApi` (`bool`) → `.ExcludeFromDescription()` instead of `.WithOpenApi()`
- 8 unit snapshot tests

---

### §4 — `TypedResults` on server endpoints ✅
**Size:** 🟡 &nbsp; **Value:** 🔸 Medium

**Delivered:**
- `Results.Ok(value)` / `Results.Ok()` / `Results.Problem(...)` → `TypedResults.Ok(value)` / `TypedResults.Ok()` / `TypedResults.Problem(...)` in `RestServerEndpointGenerator`
- Lambda return types explicitly annotated: `async Task<Results<Ok<T>, ProblemHttpResult>> (...) =>` (or `Ok<T>` when `GenerateProblemDetails = false`); `using System.Threading.Tasks;` and `using Microsoft.AspNetCore.Http.HttpResults;` added to generated files
- `.Produces<T>(200)` and `.ProducesProblem(500)` removed from fluent chains — now inferred by ASP.NET Core OpenAPI from the typed return annotation
- User-specified extra status codes (`ProducesStatusCodes`) still emitted as explicit `.Produces(statusCode)` calls
- 99 unit snapshot files updated across four snapshot folders

---

### §5 — Explicit parameter binding ✅
**Size:** 🟢 &nbsp; **Value:** 🔸 Medium

**Delivered:**
- `ParameterSource` enum (`Auto`, `Route`, `Query`, `Body`, `Header`) + `[ServerFunctionParameter]` attribute in Abstractions
- Server: `[FromHeader(Name="...")]`, `[FromQuery]` emitted for explicit sources; auto params remain in the DTO
- Client: `HttpRequestMessage`+`SendAsync` path for headers/mixed; query params sent in URL
- BSF031 (error): `ParameterSource.Route` specified but `{paramName}` not present in the route template
- BSF032 (error): `ParameterSource.Body` on a GET or DELETE method — browsers (Fetch API) forbid a body on GET/DELETE, making the endpoint unreachable from WASM
- Unit snapshot tests + E2E round-trip tests (Auto, Route, Header, Query-on-POST) + sample app (`IExplicitBindingService`)

---

### §6 — `[Obsolete]` propagation
**Size:** 🟢 &nbsp; **Value:** 🔹 Low–Medium

If a service method is marked `[Obsolete]`, the generated client proxy method silently drops the annotation. Consumers of the generated client get no deprecation warning at the call site.

**What to build:**
- During parsing, check each method symbol for `ObsoleteAttribute` (via `IMethodSymbol.GetAttributes()`)
- If present, store `ObsoleteInfo` (message, isError) on `MethodInfo`
- Client proxy generator emits `[Obsolete("message", isError)]` on the generated method
- Server endpoint generator emits `.Deprecated()` on the OpenAPI metadata (only when `Microsoft.AspNetCore.OpenApi` is referenced)
- No new diagnostics needed
- If possible, do the following – when using [Obsolete] on a method in an interface, the user can create a custom partial class (the same one getting generated) and implement this method manually. This would require the generated client service class to the partial but the server API endpoint could be a problem here.

---

### §7 — gRPC HTTP/2 enforcement
**Size:** 🟢 &nbsp; **Value:** 🔹 Low (safety/correctness)

The generated `GrpcChannel` singleton does not enforce HTTP/2. In misconfigured environments (reverse proxy stripping HTTP/2, or HTTP/1.1 fallback enabled), gRPC calls fail at runtime with an opaque `RpcException`. The generator should make this a compile-time non-issue.

**What to build:**
- In `GrpcClientProxyGenerator`, set `HttpVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher` on the `SocketsHttpHandler` passed to `GrpcChannel.ForAddress`
- Emit a comment in the generated registration: `// gRPC requires HTTP/2. Ensure your reverse proxy forwards HTTP/2.`
- No new attributes or diagnostics — this is always the right default

---

### §8 — Code readability & maintainability
**Size:** 🔴 &nbsp; **Value:** 🔹 Internal (maintainability)

Large generator classes (`RestClientProxyGenerator`, `RestServerEndpointGenerator`, gRPC equivalents) are 500–600 lines each. The REST and gRPC generators share significant logic that is currently duplicated.

**What to build:**
- Split each generator into partial classes by concern: routing, auth, caching, OpenAPI, streaming, DI registration
- Extract shared helpers (route building, auth chain generation, diagnostic guards) into a common `GeneratorHelpers` static class
- Shared base for REST/gRPC registration generators
- No user-visible changes; all existing tests must pass unchanged after the refactor

---

## Progress tracker

- [x] §1 — File upload (`Stream` / `IFormFile`)
- [~] §2 — Result\<T\> converter — cancelled (thin service wrapper + `IExceptionHandler` is the right pattern; documented in README)
- [x] §3 — OpenAPI customization per method (`Summary`, `Description`, `Tags`, `ProducesStatusCodes`, `ExcludeFromOpenApi`)
- [x] §4 — `TypedResults` on server endpoints (`TypedResults.Ok<T>()`, explicit lambda return annotation, removed `.Produces<T>()` / `.ProducesProblem()`)
- [x] §5 — Explicit parameter binding (`[ServerFunctionParameter(From = ...)]`)
- [ ] §6 — `[Obsolete]` propagation to generated client + OpenAPI
- [ ] §7 — gRPC HTTP/2 enforcement
- [ ] §8 — Code readability & maintainability (partial classes, shared helpers)
