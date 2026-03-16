using BlazorServerFunctions.Sample;
using BlazorServerFunctions.Sample.Components;
using BlazorServerFunctions.Sample.Components.Admin;
using BlazorServerFunctions.Sample.Components.Crud;
using BlazorServerFunctions.Sample.Components.Echo;
using BlazorServerFunctions.Sample.Components.Weather;
using BlazorServerFunctions.Sample.Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IEchoService, EchoService>();
builder.Services.AddScoped<ICrudService, CrudService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorServerFunctions.Sample.Client._Imports).Assembly);

app.MapServerFunctionEndpoints();

await app.RunAsync().ConfigureAwait(true);