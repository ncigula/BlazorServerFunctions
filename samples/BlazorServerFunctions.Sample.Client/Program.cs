using BlazorServerFunctions.Sample.Client;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

// Register all server function clients (IWeatherService → WeatherServiceClient, etc.)
// so WASM and Auto components can inject the same interfaces as Server components.
builder.Services.AddServerFunctionClients(new Uri(builder.HostEnvironment.BaseAddress));

await builder.Build().RunAsync().ConfigureAwait(true);