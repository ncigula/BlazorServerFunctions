# BlazorServerFunctions

[![NuGet](https://img.shields.io/nuget/v/BlazorServerFunctions.svg)](https://www.nuget.org/packages/BlazorServerFunctions)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

**Zero-boilerplate HTTP client proxies and ASP.NET Core minimal API endpoints — generated at compile time from a single C# interface.**

Annotate an interface once. BlazorServerFunctions generates the HttpClient proxy, the server-side minimal API endpoints, and the DI wiring for both. Works with Blazor Server, WASM, and Auto render modes.

---

## Installation

```bash
dotnet add package BlazorServerFunctions
```

The package is a Roslyn source generator — no runtime dependency, no reflection, no middleware.

---

## Quick Start

### 1. Define a shared interface (e.g. in a `.Shared` project)

```csharp
using BlazorServerFunctions.Abstractions;

[ServerFunctionCollection(RoutePrefix = "api/weather")]
public interface IWeatherService
{
    [ServerFunction(HttpMethod = "GET")]
    Task<WeatherForecast[]> GetForecastAsync(string city, CancellationToken ct = default);

    [ServerFunction(HttpMethod = "POST")]
    Task<WeatherForecast> CreateForecastAsync(WeatherForecast forecast, CancellationToken ct = default);
}
```

### 2. Implement it on the server

```csharp
public class WeatherService : IWeatherService
{
    public Task<WeatherForecast[]> GetForecastAsync(string city, CancellationToken ct) { ... }
    public Task<WeatherForecast> CreateForecastAsync(WeatherForecast forecast, CancellationToken ct) { ... }
}
```

### 3. Register on the server (`Program.cs`)

```csharp
builder.Services.AddScoped<IWeatherService, WeatherService>();

var app = builder.Build();

app.MapServerFunctionEndpoints(); // generated — maps all endpoints
```

### 4. Register on the client (`Program.cs` — WASM or Blazor Server)

```csharp
builder.Services.AddServerFunctionClients(
    baseAddress: new Uri("https://localhost:5001"));
```

### 5. Inject and use in a Blazor component

```razor
@inject IWeatherService WeatherService

@code {
    WeatherForecast[]? forecasts;

    protected override async Task OnInitializedAsync()
    {
        forecasts = await WeatherService.GetForecastAsync("London");
    }
}
```

The generator produces a `WeatherServiceClient` that implements `IWeatherService` — your component code never changes between render modes.

---

## Configuration

For advanced scenarios, subclass `ServerFunctionConfiguration` to share settings across multiple interfaces.

```csharp
// Define once — can be shared across interfaces via inheritance
public class MyApiConfig : ServerFunctionConfiguration
{
    public MyApiConfig()
    {
        BaseRoute = "api/v1";
        RouteNaming = RouteNaming.KebabCase;
    }
}

// Apply to an interface
[ServerFunctionCollection(Configuration = typeof(MyApiConfig))]
public interface IUserService
{
    [ServerFunction(HttpMethod = "GET")]
    Task<User[]> GetUsersAsync();  // → GET /api/v1/userservice/get-users-async
}
```

### Available settings

| Setting | Type | Default | Description |
|---|---|---|---|
| `BaseRoute` | `string` | `"api/functions"` | Route prefix for all endpoints in the collection |
| `RouteNaming` | `RouteNaming` | `PascalCase` | Route segment casing: `PascalCase`, `CamelCase`, `KebabCase`, `SnakeCase` |
| `DefaultHttpMethod` | `string?` | `null` | Default HTTP method when `[ServerFunction]` doesn't specify one (suppresses BSF013) |
| `GenerateProblemDetails` | `bool` | `true` | Emit Problem Details error responses from server endpoints |
| `EnableResilience` | `bool` | `false` | Apply standard resilience pipeline to generated HTTP clients |
| `Nullable` | `bool` | `true` | Emit `#nullable enable` at the top of generated files |
| `CustomHttpClientType` | `Type?` | `null` | Use a custom `HttpClient` subclass in generated proxy constructors |
| `ApiType` | `ApiType` | `REST` | Transport protocol (`REST` or `GRPC`) |

**Configuration priority (highest wins):**
```
[ServerFunction(...)] attribute property   ← highest
[ServerFunctionCollection(Configuration = typeof(...))]
Generator defaults                         ← lowest
```

---

## Attribute Reference

### `[ServerFunctionCollection]`

Applied to an interface. Controls route prefix, authorization, and optional configuration for all methods in the collection.

| Property | Type | Default | Description |
|---|---|---|---|
| `RoutePrefix` | `string?` | `null` | Route prefix prepended to all method routes (e.g. `"api/users"`) |
| `RequireAuthorization` | `bool` | `false` | Calls `.RequireAuthorization()` on the generated route group |
| `Configuration` | `Type?` | `null` | A `ServerFunctionConfiguration` subclass that controls code generation settings |

### `[ServerFunction]`

Applied to a method. Controls the HTTP method, route, and per-method authorization.

| Property | Type | Default | Description |
|---|---|---|---|
| `HttpMethod` | `string` | *(required)* | `"GET"`, `"POST"`, `"PUT"`, `"PATCH"`, or `"DELETE"` |
| `Route` | `string?` | Method name | Route segment appended to the collection's prefix |
| `RequireAuthorization` | `bool` | `false` | Calls `.RequireAuthorization()` on this specific endpoint |

---

## DI Setup

### Server-side (`Program.cs`)

```csharp
// Register your service implementations as usual
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// One call maps all generated endpoints
app.MapServerFunctionEndpoints();
```

### Client-side (`Program.cs`)

```csharp
// Simple — base address only
builder.Services.AddServerFunctionClients(
    baseAddress: new Uri(builder.HostEnvironment.BaseAddress));

// Advanced — customise each IHttpClientBuilder (auth handlers, resilience, etc.)
builder.Services.AddServerFunctionClients(
    baseAddress: new Uri(builder.HostEnvironment.BaseAddress),
    configureClient: builder => builder.AddHttpMessageHandler<AuthHandler>());
```

`configureClient` is called for every registered service client, so it's the right place for cross-cutting concerns like JWT bearer tokens, cookie forwarding, resilience pipelines, or logging handlers.

---

## Authentication & JWT Example

**Server** — protect the whole collection:

```csharp
[ServerFunctionCollection(RoutePrefix = "api/admin", RequireAuthorization = true)]
public interface IAdminService
{
    [ServerFunction(HttpMethod = "GET")]
    Task<AdminStats> GetStatsAsync();
}
```

Or protect individual methods:

```csharp
[ServerFunctionCollection(RoutePrefix = "api/users")]
public interface IUserService
{
    [ServerFunction(HttpMethod = "GET")]
    Task<UserProfile[]> GetAllAsync();                         // public

    [ServerFunction(HttpMethod = "DELETE", RequireAuthorization = true)]
    Task DeleteUserAsync(Guid id);                             // protected
}
```

**Client** — attach a JWT bearer delegating handler:

```csharp
public class JwtBearerHandler : DelegatingHandler
{
    private readonly ITokenService _tokens;
    public JwtBearerHandler(ITokenService tokens) => _tokens = tokens;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        var token = await _tokens.GetAccessTokenAsync(ct);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, ct);
    }
}

// Registration
builder.Services.AddTransient<JwtBearerHandler>();
builder.Services.AddServerFunctionClients(
    baseAddress: new Uri(builder.HostEnvironment.BaseAddress),
    configureClient: b => b.AddHttpMessageHandler<JwtBearerHandler>());
```

**Error handling** — when the server returns a non-success status code, the generated client throws `HttpRequestException`. If the server returned a Problem Details body (RFC 9457), its `detail` field is forwarded as the exception message.

---

## How It Works

The generator inspects every `[ServerFunctionCollection]` interface at compile time and produces four files:

| Generated file | Content |
|---|---|
| `{Interface}Client.g.cs` | `HttpClient`-based implementation of the interface |
| `{Interface}ServerExtensions.g.cs` | Minimal API endpoint mappings |
| `ServerFunctionClientsRegistration.g.cs` | `AddServerFunctionClients(...)` extension method |
| `ServerFunctionEndpointsRegistration.g.cs` | `MapServerFunctionEndpoints()` extension method |

The generator detects project type automatically:
- **Server project** (references `IEndpointRouteBuilder`) → generates all four files
- **WASM / Library project** → generates client proxy + client registration only

---

## Sample App

See [`samples/`](samples/) for a complete Blazor app demonstrating all four render modes (Server, WASM, Auto, and a plain HTTP client) with Aspire orchestration.

---

## License

MIT — see [LICENSE](LICENSE).
