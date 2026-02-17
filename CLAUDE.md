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

### Unit Tests (String-Based Pipeline Testing)

Unit tests verify the **full generator pipeline** (parsing → generation) using string-based interface code as input:

- **Input**: C# interface code as strings (not pre-built `InterfaceInfo` objects)
- **Pipeline**: String → `InterfaceParser` → Generators → Generated code + diagnostics
- **Verification**: Snapshot testing via Verify + diagnostic assertions
- **Benefits**: Tests both parser and generator logic, fast execution, comprehensive edge case coverage

Test structure:
```csharp
[Fact]
public Task Generate_BasicInterface_ProducesCorrectCode()
{
    string source = """
        [ServerFunctionCollection]
        public interface IUserService
        {
            [ServerFunction]
            Task<User> GetUserAsync(int id);
        }
        """;

    var result = GeneratorTestHelper.RunGeneratorAsClient(source, new ServerFunctionCollectionGenerator());
    return result.VerifyNoDiagnostics(); // Verifies diagnostics + snapshots all generated files
}
```

**What unit tests cover**:
- All parsing edge cases (attributes, parameter types, return types, route prefixes, HTTP methods)
- All generation variations (method signatures, request/response handling, async types)
- Diagnostic reporting for invalid code
- Both client and server code generation from a single interface

**Test classes**:
- `ClientGeneratorTests` — Tests client proxy + registration generation (Library/Client project types)
- `ServerGeneratorTests` — Tests server endpoint + registration generation (Server project type)

Snapshots live in `tests/BlazorServerFunctions.Generator.UnitTests/_snapshots/`

### Integration Tests (Multi-Project Compilation)

Integration tests focus on **cross-cutting concerns** that unit tests cannot verify:

- Multi-project scenarios (client references server library)
- Actual compilation success (generated code compiles without errors)
- File placement and hint names
- Project type detection logic
- Incremental generator behavior
- External assembly references (interfaces from NuGet packages)

**What integration tests DON'T do**: Re-test all edge cases already covered in unit tests. They verify representative scenarios only.

Integration tests use `CSharpGeneratorDriver` with `ProjectBuilder`/`MultiProjectScenario` to simulate multi-project builds.

### General Rules

- Test projects suppress `CA1707` (underscores in test names are allowed)
- Use fluent builders (`InterfaceInfoBuilder`, `MethodInfoBuilder`, `ParameterInfoBuilder`, `TestDataFactory`) only when constructing test data for non-generator tests or helpers

## Diagnostics

Error codes are centralized in `DiagnosticDescriptors.cs`. Codes use the `BSF` prefix: errors are BSF001-BSF099, warnings are BSF101+. The `InterfaceParser` reports diagnostics via `SourceProductionContext` and returns null on parse failure.

## Generator Internals

- The generator project exposes internals to the unit test project via `InternalsVisibleTo`
- The generator is packaged as a NuGet analyzer (output goes to `analyzers/dotnet/cs`)
- `BlazorServerFunctions.Generator.props` is auto-imported by consuming projects for local development
