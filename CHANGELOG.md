# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
