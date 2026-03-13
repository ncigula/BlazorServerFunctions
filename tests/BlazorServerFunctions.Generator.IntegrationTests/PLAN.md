# Integration Tests & Diagnostics Plan

## Context

The goal is to build out a complete, trustworthy integration test suite covering:
1. A bug fix in the generator's client file hint name
2. Diagnostic assertion helpers on `CompiledProject`
3. Full implementation of all declared but unimplemented diagnostics (BSF003-BSF016, BSF101, BSF102)
4. A `DiagnosticIntegrationTests.cs` covering every single BSF code
5. Strengthening existing multi-project tests with no-diagnostic assertions

---

## Phase 0: Fix Client File Hint Name in Generator

**File:** `src/BlazorServerFunctions.Generator/Generators/ServerFunctionCollectionGenerator.cs:262`

The `ClientProxyGenerator` names the generated class `{Name.TrimStart('I')}Client` (e.g., `WeatherServiceClient`), but the file hint name uses `{interfaceInfo.Name}Client.g.cs` (e.g., `IWeatherServiceClient.g.cs`). These are inconsistent.

```csharp
// Before:
context.AddSource($"{interfaceInfo.Name}Client.g.cs", clientCode);

// After:
context.AddSource($"{interfaceInfo.Name.TrimStart('I')}Client.g.cs", clientCode);
```

**`AssertHasClientFiles` with `TrimStart('I')` is intentionally correct — it stays as-is.**

Side effect: unit test snapshots named `I{Name}Client.g.verified.cs` must be deleted and re-accepted after running tests.

---

## Phase 1: Add Diagnostic Helpers to `CompiledProject`

**File:** `tests/BlazorServerFunctions.Generator.IntegrationTests/Helpers/CompiledProject.cs`

Add:
```csharp
public IReadOnlyCollection<Diagnostic> Diagnostics =>
    GeneratorResults.Diagnostics;

public void AssertNoDiagnostics()
{
    if (GeneratorResults.Diagnostics.Length > 0)
    {
        var messages = string.Join(Environment.NewLine,
            GeneratorResults.Diagnostics.Select(d => d.ToString()));
        Assert.Fail($"Expected no diagnostics in {Definition.Name}, but got:{Environment.NewLine}{messages}");
    }
}

public void AssertHasDiagnostic(string diagnosticId)
{
    Assert.True(
        GeneratorResults.Diagnostics.Any(d => d.Id == diagnosticId),
        $"Expected diagnostic {diagnosticId} in {Definition.Name}. " +
        $"Got: [{string.Join(", ", GeneratorResults.Diagnostics.Select(d => d.Id))}]");
}
```

---

## Phase 2: Implement All Missing Diagnostics in `InterfaceParser`

**File:** `src/BlazorServerFunctions.Generator/Helpers/InterfaceParser.cs`

Currently only BSF001, BSF002, BSF012, BSF013 are implemented. The rest exist in `DiagnosticDescriptors.cs` but are never emitted. `HasErrors` tracks errors only (not warnings), so warnings (BSF101, BSF102) emit but don't block code generation.

### Add to `ParseInterface` (interface-level checks):

**BSF003** — After `serverFunctionCollectionAttribute` check, before parsing methods:
```csharp
if (interfaceSymbol.DeclaredAccessibility != Accessibility.Public)
{
    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InterfaceMustBePublic,
        interfaceSymbol.Locations.FirstOrDefault(), interfaceSymbol.Name));
    return interfaceInfo; // skip method parsing
}
```

**BSF004** — Check for generic type parameters:
```csharp
if (interfaceSymbol.TypeParameters.Length > 0)
    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InterfaceCannotBeGeneric,
        interfaceSymbol.Locations.FirstOrDefault(), interfaceSymbol.Name));
```

**BSF005** — Check for property members:
```csharp
if (interfaceSymbol.GetMembers().OfType<IPropertySymbol>().Any())
    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InterfaceCannotHaveProperties,
        interfaceSymbol.Locations.FirstOrDefault(), interfaceSymbol.Name));
```

**BSF006** — Check for event members:
```csharp
if (interfaceSymbol.GetMembers().OfType<IEventSymbol>().Any())
    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InterfaceCannotHaveEvents,
        interfaceSymbol.Locations.FirstOrDefault(), interfaceSymbol.Name));
```

**BSF101** — After `interfaceInfo.Methods.AddRange(...)` (warning, doesn't block generation):
```csharp
if (interfaceInfo.Methods.Count == 0)
    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.EmptyInterface,
        interfaceSymbol.Locations.FirstOrDefault(), interfaceSymbol.Name));
```

### Add to `ParseMethods` (route duplicate check):

**BSF014** — Track seen routes in a dictionary during method iteration:
```csharp
var seenRoutes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
// For each method:
var route = methodInfo.CustomRoute ?? methodInfo.Name;
if (seenRoutes.TryGetValue(route, out var existingName))
    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.DuplicateRoute,
        methodSymbol.Locations.First(), methodInfo.Name, route, existingName));
else
    seenRoutes[route] = methodInfo.Name;
```

### Add to `ParseMethod` (method-level checks):

**BSF007** — Change `ParseReturnType` signature to accept `context`:
```csharp
private static (AsyncType, string) ParseReturnType(SourceProductionContextWrapper context, IMethodSymbol methodSymbol)
```
In the method body: if `!isAsync`, report BSF007 and return `(AsyncType.Task, "void")` fallback. Replace the `throw` with the same fallback (the `_` case is unreachable if `IsAsyncType` is strict, but safer to handle).

**BSF008** — Generic method check:
```csharp
if (methodSymbol.TypeParameters.Length > 0)
    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MethodCannotBeGeneric,
        methodSymbol.Locations.First(), methodSymbol.Name));
```

**BSF009/BSF010/BSF011** — Add parameter modifier checks before `ParseParameters`, iterating `methodSymbol.Parameters`:
- `RefKind.Out` → BSF009
- `RefKind.Ref` → BSF010
- `IsParams` → BSF011

**BSF015** — In `ParseServerFunctionAttributes` after setting `CustomRoute`, validate it:
```csharp
if (methodInfo.CustomRoute != null &&
    (string.IsNullOrWhiteSpace(methodInfo.CustomRoute) || methodInfo.CustomRoute.Any(char.IsWhiteSpace)))
    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InvalidRouteFormat,
        methodSymbol.Locations.First(), methodSymbol.Name, methodInfo.CustomRoute));
```

**BSF102** — Warning when method has many parameters (threshold: > 5):
```csharp
if (methodSymbol.Parameters.Length > 5)
    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.TooManyParameters,
        methodSymbol.Locations.First(), methodSymbol.Name, methodSymbol.Parameters.Length));
```

### Add to `ParseReferencedInterfaces` in `ServerFunctionCollectionGenerator`:

**BSF016** — Wrap the `InterfaceParser.ParseInterface` call in try/catch:
```csharp
try { ... }
catch (Exception)
{
    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ReferencedInterfaceParseFailure,
        Location.None, symbol.Name, symbol.ContainingAssembly.Name));
}
```

---

## Phase 3: Create `DiagnosticIntegrationTests.cs`

**File:** `tests/BlazorServerFunctions.Generator.IntegrationTests/DiagnosticIntegrationTests.cs`

One test per BSF code. Pattern: `ProjectBuilder.AddSharedProject("MyApp", source).Build()` then assert on `scenario.GetProject("MyApp")`. Attribute syntax: `[ServerFunction(HttpMethod = "GET")]`.

| Test | BSF | Input | Assert |
|---|---|---|---|
| `ValidInterface_EmitsNoDiagnostics` | — | Correct interface | `AssertNoDiagnostics()` + has files |
| `MissingCollectionAttribute_EmitsBSF001` | BSF001 | Interface without `[ServerFunctionCollection]` | `AssertHasDiagnostic("BSF001")` + no files |
| `MissingServerFunctionAttribute_EmitsBSF002` | BSF002 | Method without `[ServerFunction]` | `AssertHasDiagnostic("BSF002")` |
| `InternalInterface_EmitsBSF003` | BSF003 | `internal interface` | `AssertHasDiagnostic("BSF003")` |
| `GenericInterface_EmitsBSF004` | BSF004 | `interface IService<T>` | `AssertHasDiagnostic("BSF004")` |
| `InterfaceWithProperty_EmitsBSF005` | BSF005 | Interface with `string Name { get; }` | `AssertHasDiagnostic("BSF005")` |
| `InterfaceWithEvent_EmitsBSF006` | BSF006 | Interface with `event EventHandler OnChanged` | `AssertHasDiagnostic("BSF006")` |
| `NonAsyncReturnType_EmitsBSF007` | BSF007 | Method returning `string` directly | `AssertHasDiagnostic("BSF007")` |
| `GenericMethod_EmitsBSF008` | BSF008 | `Task<T> GetAsync<T>()` | `AssertHasDiagnostic("BSF008")` |
| `OutParameter_EmitsBSF009` | BSF009 | `Task Get(out int x)` | `AssertHasDiagnostic("BSF009")` |
| `RefParameter_EmitsBSF010` | BSF010 | `Task Get(ref int x)` | `AssertHasDiagnostic("BSF010")` |
| `ParamsParameter_EmitsBSF011` | BSF011 | `Task Get(params int[] x)` | `AssertHasDiagnostic("BSF011")` |
| `MissingHttpMethod_EmitsBSF012` | BSF012 | `[ServerFunction]` without `HttpMethod` | `AssertHasDiagnostic("BSF012")` |
| `InvalidHttpMethod_EmitsBSF013` | BSF013 | `[ServerFunction(HttpMethod = "INVALID")]` | `AssertHasDiagnostic("BSF013")` |
| `DuplicateRoutes_EmitsBSF014` | BSF014 | Two methods with same name/route | `AssertHasDiagnostic("BSF014")` |
| `InvalidRouteFormat_EmitsBSF015` | BSF015 | `Route = "has spaces"` | `AssertHasDiagnostic("BSF015")` |
| `ReferencedInterfaceParseFailure_EmitsBSF016` | BSF016 | Multi-project: referenced interface causes parse exception | `AssertHasDiagnostic("BSF016")` (server project) |
| `EmptyInterface_EmitsBSF101Warning` | BSF101 | Interface with no methods | `AssertHasDiagnostic("BSF101")` + `AssertHasNoGeneratedFiles` |
| `TooManyParameters_EmitsBSF102Warning` | BSF102 | Method with 6+ parameters | `AssertHasDiagnostic("BSF102")` + has files (warning, not error) |

Note on BSF016: hard to trigger deterministically in a normal test. It can be tested as a "does not crash" scenario where a referenced interface has malformed/unexpected content — the test verifies the server project doesn't throw and either emits BSF016 or silently skips.

---

## Phase 4: Update `MultiProjectIntegrationTests.cs`

Add `AssertNoDiagnostics()` calls to existing tests that call `AssertCompilesSuccessfully()`:

- `Scenario1`: add `scenario.Client.AssertNoDiagnostics()`
- `Scenario2`: add for shared, client, server
- `All_Generated_Code_Compiles_Successfully`: add `AssertNoDiagnostics()` for all three projects

---

## Files to Modify

| File | Change |
|---|---|
| `src/.../Generators/ServerFunctionCollectionGenerator.cs` | Fix client hint name (Phase 0); add BSF016 try/catch (Phase 2) |
| `src/.../Helpers/InterfaceParser.cs` | Add all missing diagnostic checks (Phase 2) |
| `tests/.../Helpers/CompiledProject.cs` | Add diagnostic helpers (Phase 1) |
| `tests/.../DiagnosticIntegrationTests.cs` | **Create new** — 19 tests (Phase 3) |
| `tests/.../MultiProjectIntegrationTests.cs` | Add `AssertNoDiagnostics()` calls (Phase 4) |
| `tests/UnitTests/_snapshots/ClientGeneratorTests/*Client.g.verified.cs` | Delete `I{Name}Client.g.verified.cs`, re-accept new names after run |

---

## Verification

```bash
dotnet test tests/BlazorServerFunctions.Generator.IntegrationTests
dotnet test tests/BlazorServerFunctions.Generator.UnitTests  # re-accept renamed snapshots
dotnet test BlazorServerFunctions.slnx  # full suite
```
