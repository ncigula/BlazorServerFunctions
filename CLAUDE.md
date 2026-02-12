# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

BlazorServerFunctions is a **Roslyn incremental source generator** that generates HTTP client proxies and ASP.NET Core server endpoints from annotated C# interfaces. Interfaces marked with `[ServerFunctionCollection]` and methods marked with `[ServerFunction]` produce four types of generated code: client proxy classes, server endpoint mappings, client DI registration, and server DI registration.

## Build & Test Commands

```bash
# Build the entire solution
dotnet build BlazorServerFunctions.slnx

# Run all tests
dotnet test BlazorServerFunctions.slnx

# Run a specific test project
dotnet test tests/BlazorServerFunctions.Generator.UnitTests
dotnet test tests/BlazorServerFunctions.Generator.IntegrationTests
dotnet test tests/BlazorServerFunctions.EndToEndTests

# Run a single test by name
dotnet test tests/BlazorServerFunctions.Generator.UnitTests --filter "FullyQualifiedName~MethodName"

```

## Architecture

### Source Generator Pipeline

`ServerFunctionCollectionGenerator` (the main `IIncrementalGenerator`) orchestrates the full pipeline:

1. **Interface Detection** — Finds interfaces with `[ServerFunctionCollection]` attribute from both local source and referenced assemblies (`InterfaceSymbolCollector`)
2. **Parsing** — `InterfaceParser` extracts metadata (route prefixes, HTTP methods, auth requirements, return types, parameters) into `InterfaceInfo`/`MethodInfo`/`ParameterInfo` models
3. **Code Generation** — Four generators produce output:
   - `ClientProxyGenerator` → `{Interface}Client.g.cs` (HttpClient-based implementation)
   - `ServerEndpointGenerator` → `{Interface}ServerExtensions.g.cs` (minimal API endpoints)
   - `ClientRegistrationGenerator` → `ServerFunctionClientsRegistration.g.cs`
   - `ServerRegistrationGenerator` → `ServerFunctionEndpointsRegistration.g.cs`

### Project Type Detection

The generator detects project type by inspecting compilation references:
- **Server** (has `IEndpointRouteBuilder`): generates endpoints + clients + both registrations
- **Client** (has `WebAssemblyHostBuilder`): generates clients + client registration only
- **Library** (neither): generates clients + client registration only

### Source Layout

- `src/BlazorServerFunctions.Generator/` — The source generator (models in `Models/`, generators in `Generators/`, parsing/helpers in `Helpers/`)
- `src/BlazorServerFunctions.Abstractions/` — Public attributes (`ServerFunctionCollectionAttribute`, `ServerFunctionAttribute`)
- `tests/` — Unit tests (snapshot-based with Verify), integration tests (multi-project compilation), E2E tests (Aspire-based)
- `samples/` — Example Blazor app with Aspire orchestration

## Code Quality Rules

- **All warnings are errors** (`TreatWarningsAsErrors` + `CodeAnalysisTreatWarningsAsErrors`)
- **Banned APIs** (`BannedSymbols.txt`): Use `DateTime.UtcNow` not `DateTime.Now`, use `Span<char>` not `String.Substring`, use `Guid.CreateVersion7()` not `Guid.NewGuid()`
- Target framework is **.NET 10.0** with nullable reference types enabled
- Multiple analyzers enforced: SonarAnalyzer, Meziantou, AsyncFixer, BannedApiAnalyzers, Threading.Analyzers

## Testing Patterns

- **Unit tests** use fluent builders (`InterfaceInfoBuilder`, `MethodInfoBuilder`, `ParameterInfoBuilder`, `TestDataFactory`) to construct test data, and **Verify snapshot testing** to assert generated code
- Snapshots live in `tests/BlazorServerFunctions.Generator.UnitTests/_snapshots/`
- **Integration tests** use `CSharpGeneratorDriver` with `ProjectBuilder`/`MultiProjectScenario` to verify generated code compiles in multi-project setups
- Test projects suppress `CA1707` (underscores in test names are allowed)

## Diagnostics

Error codes are centralized in `DiagnosticDescriptors.cs`. Codes use the `BSF` prefix: errors are BSF001-BSF099, warnings are BSF101+. The `InterfaceParser` reports diagnostics via `SourceProductionContext` and returns null on parse failure.

## Generator Internals

- The generator project exposes internals to the unit test project via `InternalsVisibleTo`
- The generator is packaged as a NuGet analyzer (output goes to `analyzers/dotnet/cs`)
- `BlazorServerFunctions.Generator.props` is auto-imported by consuming projects for local development
