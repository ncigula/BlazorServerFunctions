using BlazorServerFunctions.Sample.Client;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

// Register all server function clients (IWeatherService → WeatherServiceClient, etc.)
// so WASM and Auto components can inject the same interfaces as Server components.
builder.Services.AddServerFunctionClients(new Uri(builder.HostEnvironment.BaseAddress));

// Plain HttpClient for login/logout posts from demo pages (not a generated service client).
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync().ConfigureAwait(true);