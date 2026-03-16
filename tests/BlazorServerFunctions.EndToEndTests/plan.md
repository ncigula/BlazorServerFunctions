# E2E Test Suite Plan

## Context

The sample projects have `IWeatherService` in `Sample.Shared`, but neither the server (`Program.cs`) nor the WASM client have the generated extension methods wired up. `BlazorServerFunctions.EndToEndTests` exists but is empty. We'll use `WebApplicationFactory<Program>` to host the sample server in-memory and exercise the generated code via real HTTP calls.

The tests prove: generator produces server endpoint registration + client proxy that together round-trip correctly, including CancellationToken propagation, interface-level authorization, and parameter passing via GET query strings and POST bodies.

---

## New interfaces to add to `Sample.Shared`

### 1. Modify `IWeatherService` — add CancellationToken
```csharp
[ServerFunctionCollection]
public interface IWeatherService
{
    [ServerFunction(HttpMethod = "GET")]
    Task<WeatherForecastDto[]> GetWeatherForecastsAsync(CancellationToken cancellationToken = default);
}
```
Update `WeatherService.GetWeatherForecastsAsync` to accept and forward the token to `Task.Delay`.

### 2. New: `IAdminService` — interface-level RequireAuthorization
```csharp
[ServerFunctionCollection(RequireAuthorization = true)]
public interface IAdminService
{
    [ServerFunction(HttpMethod = "GET")]
    Task<string> GetSecretAsync(CancellationToken cancellationToken = default);
}
```
Add `AdminService : IAdminService` in the server project returning a fixed string `"top-secret"`.

### 3. New: `IEchoService` — parameter passing (GET + POST)
```csharp
[ServerFunctionCollection]
public interface IEchoService
{
    [ServerFunction(HttpMethod = "GET")]
    Task<string> GetEchoAsync(string message);

    [ServerFunction(HttpMethod = "POST")]
    Task<string> PostEchoAsync(string message);
}
```
Add `EchoService : IEchoService` returning the message as-is. Tests GET with `[AsParameters]` query binding and POST with `[FromBody]` JSON binding.

### 4. New: `ComplexDto` — shared DTO for CRUD tests
```csharp
public sealed class ComplexDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public List<string> Tags { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
    public decimal Price { get; set; }
}
```

### 5. New: `ICrudService` — all remaining HTTP verbs + complex DTO
```csharp
[ServerFunctionCollection(RoutePrefix = "crud")]
public interface ICrudService
{
    [ServerFunction(HttpMethod = "GET")]
    Task<ComplexDto> GetAsync(int id);

    [ServerFunction(HttpMethod = "POST")]
    Task<ComplexDto> CreateAsync(ComplexDto item);

    [ServerFunction(HttpMethod = "PUT")]
    Task<ComplexDto> UpdateAsync(int id, ComplexDto item);

    [ServerFunction(HttpMethod = "PATCH")]
    Task<ComplexDto> PatchAsync(int id, string field, string value);

    [ServerFunction(HttpMethod = "DELETE")]
    Task DeleteAsync(int id);
}
```
Add `CrudService : ICrudService` that echoes inputs back (no real storage needed — we're testing the transport layer, not CRUD logic):
- `GetAsync` → returns `new ComplexDto { Id = id, Name = "item-{id}" }`
- `CreateAsync` → returns the item with `Id = 1` set
- `UpdateAsync` → returns the item with the given `id` set
- `PatchAsync` → returns `new ComplexDto { Id = id, Name = value }` (reflects the patch)
- `DeleteAsync` → returns (void-equivalent)

---

## Step-by-step implementation

### Step 1 — `samples/BlazorServerFunctions.Sample.Shared/`

- **Modify** `IWeatherService.cs` — add CT parameter (see above)
- **Create** `IAdminService.cs`
- **Create** `IEchoService.cs`
- **Create** `ComplexDto.cs`
- **Create** `ICrudService.cs`

### Step 2 — `samples/BlazorServerFunctions.Sample/`

- **Modify** `Components/Weather/WeatherService.cs` — accept CT, pass to `Task.Delay`
- **Create** `Components/Admin/AdminService.cs` — `return "top-secret";`
- **Create** `Components/Echo/EchoService.cs` — `return message;`
- **Create** `Components/Crud/CrudService.cs` — echo-back implementation
- **Modify** `Program.cs`:
  ```csharp
  builder.Services.AddAuthentication();   // required for RequireAuthorization to work
  builder.Services.AddAuthorization();
  builder.Services.AddScoped<IWeatherService, WeatherService>();
  builder.Services.AddScoped<IAdminService, AdminService>();
  builder.Services.AddScoped<IEchoService, EchoService>();
  builder.Services.AddScoped<ICrudService, CrudService>();

  // ... existing middleware ...
  app.UseAuthentication();               // before UseAuthorization
  app.UseAuthorization();
  app.MapServerFunctionEndpoints();      // generated — registers all 3 interfaces

  await app.RunAsync().ConfigureAwait(true);

  public partial class Program { }      // required for WebApplicationFactory<Program>
  ```

### Step 3 — `samples/BlazorServerFunctions.Sample.Client/Program.cs`

```csharp
WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddServerFunctionClients(builder.HostEnvironment.BaseAddress);
await builder.Build().RunAsync().ConfigureAwait(true);
```

### Step 4 — `tests/BlazorServerFunctions.EndToEndTests/BlazorServerFunctions.EndToEndTests.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <NoWarn>$(NoWarn);NU1608</NoWarn>
  </PropertyGroup>

  <ItemGroup>
    <Content Include="xunit.runner.json" CopyToOutputDirectory="PreserveNewest"/>
  </ItemGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" />
  </ItemGroup>

  <ItemGroup>
    <!-- Generator runs in this project → generates WeatherServiceClient, etc. -->
    <ProjectReference Include="..\..\src\BlazorServerFunctions.Generator\BlazorServerFunctions.Generator.csproj"
                      OutputItemType="Analyzer"
                      ReferenceOutputAssembly="false" />
    <ProjectReference Include="..\..\src\BlazorServerFunctions.Abstractions\BlazorServerFunctions.Abstractions.csproj" />
    <!-- Test server host -->
    <ProjectReference Include="..\..\samples\BlazorServerFunctions.Sample\BlazorServerFunctions.Sample.csproj" />
    <!-- Interface definitions + DTOs -->
    <ProjectReference Include="..\..\samples\BlazorServerFunctions.Sample.Shared\BlazorServerFunctions.Sample.Shared.csproj" />
  </ItemGroup>
</Project>
```

The generator runs in the test project (Library type), finds all 3 interfaces in the referenced Shared assembly, generates `WeatherServiceClient`, `AdminServiceClient`, `EchoServiceClient` + `ServerFunctionClientsRegistration`.

### Step 5 — `tests/BlazorServerFunctions.EndToEndTests/GlobalUsings.cs`

```csharp
global using Xunit;
global using Microsoft.AspNetCore.Mvc.Testing;
global using BlazorServerFunctions.Sample.Shared;
```

### Step 6 — New test files

#### `Helpers/TestAuthHandler.cs`
```csharp
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BlazorServerFunctions.EndToEndTests;

public class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] { new Claim(ClaimTypes.Name, "testuser") };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
```

#### `WeatherServiceE2ETests.cs` (6 tests)
```csharp
namespace BlazorServerFunctions.EndToEndTests;

public class WeatherServiceE2ETests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WeatherServiceClient _client = new(factory.CreateClient());

    [Fact]
    public async Task GetWeatherForecastsAsync_ReturnsExpectedCount()
    {
        var result = await _client.GetWeatherForecastsAsync();
        Assert.Equal(5, result.Length);
    }

    [Fact]
    public async Task GetWeatherForecastsAsync_AllForecastsHaveRequiredFields()
    {
        var result = await _client.GetWeatherForecastsAsync();
        Assert.All(result, f =>
        {
            Assert.NotNull(f.Summary);
            Assert.NotEqual(default, f.Date);
        });
    }

    [Fact]
    public async Task GetWeatherForecastsAsync_TemperatureFMatchesFormula()
    {
        var result = await _client.GetWeatherForecastsAsync();
        Assert.All(result, f =>
            Assert.Equal(32 + (int)(f.TemperatureC / 0.5556), f.TemperatureF));
    }

    [Fact]
    public async Task GetWeatherForecastsAsync_WithDefaultCancellationToken_Succeeds()
    {
        var result = await _client.GetWeatherForecastsAsync(CancellationToken.None);
        Assert.NotEmpty(result);
    }

    // Pre-cancelled: proves CT is passed to HttpClient.GetAsync()
    [Fact]
    public async Task GetWeatherForecastsAsync_WithPreCancelledToken_ThrowsImmediately()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => _client.GetWeatherForecastsAsync(cts.Token));
    }

    // Mid-flight: proves CT propagates through the server to Task.Delay in the service
    // WeatherService has a 500ms delay; we cancel at 100ms so the request is in-flight
    [Fact]
    public async Task GetWeatherForecastsAsync_CancelledMidFlight_ThrowsTaskCanceledException()
    {
        using var cts = new CancellationTokenSource(millisecondsDelay: 100);
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => _client.GetWeatherForecastsAsync(cts.Token));
    }
}
```

#### `AuthorizationE2ETests.cs` (3 tests)
```csharp
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorServerFunctions.EndToEndTests;

public class AuthorizationE2ETests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    // No auth: factory default → RequireAuthorization returns 401
    private readonly AdminServiceClient _unauthenticatedClient = new(factory.CreateClient());

    // With auth: override auth scheme → authenticated user → 200
    private readonly AdminServiceClient _authenticatedClient = new(
        factory.WithWebHostBuilder(b =>
            b.ConfigureTestServices(services =>
                services.AddAuthentication(TestAuthHandler.SchemeName)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.SchemeName, _ => { })))
            .CreateClient());

    [Fact]
    public async Task GetSecretAsync_WithoutAuthentication_Throws401()
    {
        var ex = await Assert.ThrowsAsync<HttpRequestException>(
            () => _unauthenticatedClient.GetSecretAsync());
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, ex.StatusCode);
    }

    [Fact]
    public async Task GetSecretAsync_WithAuthentication_ReturnsSecret()
    {
        var result = await _authenticatedClient.GetSecretAsync();
        Assert.Equal("top-secret", result);
    }

    [Fact]
    public async Task WeatherEndpoint_WithoutAuthentication_StillAccessible()
    {
        // Proves non-auth interfaces are unaffected by auth middleware
        var weatherClient = new WeatherServiceClient(factory.CreateClient());
        var result = await weatherClient.GetWeatherForecastsAsync();
        Assert.NotEmpty(result);
    }
}
```

#### `EchoServiceE2ETests.cs` (3 tests)
```csharp
namespace BlazorServerFunctions.EndToEndTests;

public class EchoServiceE2ETests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly EchoServiceClient _client = new(factory.CreateClient());

    [Fact]
    public async Task GetEchoAsync_QueryStringParameter_RoundTripsCorrectly()
    {
        var result = await _client.GetEchoAsync("hello-world");
        Assert.Equal("hello-world", result);
    }

    [Fact]
    public async Task PostEchoAsync_BodyParameter_RoundTripsCorrectly()
    {
        var result = await _client.PostEchoAsync("hello-post");
        Assert.Equal("hello-post", result);
    }

    [Fact]
    public async Task GetEchoAsync_WithSpecialCharacters_RoundTripsCorrectly()
    {
        var result = await _client.GetEchoAsync("hello world & more");
        Assert.Equal("hello world & more", result);
    }
}
```

#### `CrudServiceE2ETests.cs` (6 tests — one per HTTP verb + complex DTO round-trip)
```csharp
namespace BlazorServerFunctions.EndToEndTests;

public class CrudServiceE2ETests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly CrudServiceClient _client = new(factory.CreateClient());

    [Fact]
    public async Task GetAsync_ReturnsItemWithCorrectId()
    {
        var result = await _client.GetAsync(42);
        Assert.Equal(42, result.Id);
    }

    [Fact]
    public async Task CreateAsync_ComplexDto_RoundTripsAllFields()
    {
        var input = new ComplexDto
        {
            Name = "Test Item",
            Description = "A complex object",
            Tags = ["tag1", "tag2"],
            CreatedAt = DateTimeOffset.UtcNow,
            Price = 99.99m
        };

        var result = await _client.CreateAsync(input);

        Assert.Equal(input.Name, result.Name);
        Assert.Equal(input.Description, result.Description);
        Assert.Equal(input.Tags, result.Tags);
        Assert.Equal(input.Price, result.Price);
    }

    [Fact]
    public async Task UpdateAsync_PutWithComplexDto_ReturnsUpdatedItem()
    {
        var update = new ComplexDto { Id = 5, Name = "Updated", Price = 1.0m };
        var result = await _client.UpdateAsync(5, update);
        Assert.Equal(5, result.Id);
        Assert.Equal("Updated", result.Name);
    }

    [Fact]
    public async Task PatchAsync_PartialUpdate_ReflectsChange()
    {
        var result = await _client.PatchAsync(7, "Name", "patched-value");
        Assert.Equal(7, result.Id);
        Assert.Equal("patched-value", result.Name);
    }

    [Fact]
    public async Task DeleteAsync_CompletesWithoutError()
    {
        // DELETE returns void — just verify no exception thrown
        await _client.DeleteAsync(10);
    }

    [Fact]
    public async Task CreateAsync_NullableFields_RoundTripsNullCorrectly()
    {
        var input = new ComplexDto { Name = "No Description", Description = null };
        var result = await _client.CreateAsync(input);
        Assert.Null(result.Description);
    }
}
```

---

## What each test group proves about the generator

| Test group | Tests | Generator feature exercised |
|---|---|---|
| `WeatherServiceE2ETests` | 6 | GET endpoint, array deserialization, CancellationToken in signature, pre-cancelled throws, mid-flight cancellation propagates through server |
| `AuthorizationE2ETests` | 3 | `group.RequireAuthorization()` emitted for interface-level auth; non-auth endpoints unaffected |
| `EchoServiceE2ETests` | 3 | GET with `[AsParameters]` query binding; POST with `[FromBody]` JSON binding |
| `CrudServiceE2ETests` | 6 | PUT, PATCH, DELETE verbs; complex DTO round-trip including nullable fields and collections |

**Total: 18 E2E tests**

---

## All files

### Create (new)
| File | Description |
|---|---|
| `samples/BlazorServerFunctions.Sample.Shared/IAdminService.cs` | Authorized interface |
| `samples/BlazorServerFunctions.Sample.Shared/IEchoService.cs` | GET + POST parameter interface |
| `samples/BlazorServerFunctions.Sample.Shared/ComplexDto.cs` | DTO with collections, nullable, decimal |
| `samples/BlazorServerFunctions.Sample.Shared/ICrudService.cs` | All 5 HTTP verbs |
| `samples/BlazorServerFunctions.Sample/Components/Admin/AdminService.cs` | Returns `"top-secret"` |
| `samples/BlazorServerFunctions.Sample/Components/Echo/EchoService.cs` | Returns `message` |
| `samples/BlazorServerFunctions.Sample/Components/Crud/CrudService.cs` | Echo-back for all methods |
| `tests/BlazorServerFunctions.EndToEndTests/Helpers/TestAuthHandler.cs` | Test auth scheme |
| `tests/BlazorServerFunctions.EndToEndTests/WeatherServiceE2ETests.cs` | 6 tests |
| `tests/BlazorServerFunctions.EndToEndTests/AuthorizationE2ETests.cs` | 3 tests |
| `tests/BlazorServerFunctions.EndToEndTests/EchoServiceE2ETests.cs` | 3 tests |
| `tests/BlazorServerFunctions.EndToEndTests/CrudServiceE2ETests.cs` | 6 tests |

### Modify (existing)
| File | Change |
|---|---|
| `samples/BlazorServerFunctions.Sample.Shared/IWeatherService.cs` | Add `CancellationToken` default param |
| `samples/BlazorServerFunctions.Sample/Components/Weather/WeatherService.cs` | Accept + forward CT |
| `samples/BlazorServerFunctions.Sample/Program.cs` | Wire endpoints, auth, services, `partial class Program` |
| `samples/BlazorServerFunctions.Sample.Client/Program.cs` | `AddServerFunctionClients` |
| `tests/BlazorServerFunctions.EndToEndTests/BlazorServerFunctions.EndToEndTests.csproj` | Swap packages, add refs |
| `tests/BlazorServerFunctions.EndToEndTests/GlobalUsings.cs` | Add usings |

---

## Future-proofing note (gRPC / RPC)

Tests are intentionally transport-agnostic: they call `WeatherServiceClient.GetWeatherForecastsAsync()` and assert on `WeatherForecastDto[]` — not on HTTP verbs, URLs, or JSON. When a gRPC transport is added, a `WeatherServiceGrpcClient` would be generated, and these assertions would remain valid with minimal changes (swap client type). The `IWeatherService` interface contract is the stable surface being tested.

---

## Verification

```bash
dotnet test tests/BlazorServerFunctions.EndToEndTests
```

Expected: 11 tests pass. Failure modes are diagnostic:
- Build error → generator didn't produce a client class
- 404 → `MapServerFunctionEndpoints()` not wired or wrong route generated
- 401 on weather → auth middleware incorrectly applied to non-auth interface
- 200 on admin without auth → `RequireAuthorization()` not emitted
- Wrong echo value → parameter binding attribute wrong (GET vs POST mismatch)
- CT mid-flight test not throwing → CT not propagated through server to `Task.Delay`
- Complex DTO field missing → JSON serialization contract broken
