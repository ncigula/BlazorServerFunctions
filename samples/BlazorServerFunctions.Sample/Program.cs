using System.Security.Claims;
using BlazorServerFunctions.Sample;
using BlazorServerFunctions.Sample.Components;
using BlazorServerFunctions.Sample.Components.Admin;
using BlazorServerFunctions.Sample.Components.Crud;
using BlazorServerFunctions.Sample.Components.Echo;
using BlazorServerFunctions.Sample.Components.RouteParams;
using BlazorServerFunctions.Sample.Components.Weather;
using BlazorServerFunctions.Sample.Shared;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Cookie auth for the Admin demo page.
builder.Services.AddAuthentication("Cookies")
    .AddCookie(options =>
    {
        options.LoginPath = "/demos/admin/wasm";
        // API clients (HttpClient from WASM) don't follow redirects gracefully —
        // return 401 directly so the generated client throws HttpRequestException
        // instead of following the redirect to the login page and returning HTML.
        options.Events.OnRedirectToLogin = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddProblemDetails();

builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IEchoService, EchoService>();
builder.Services.AddSingleton<ICrudService, CrudService>();
builder.Services.AddScoped<IRouteParamService, RouteParamService>();

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

// Demo login/logout for the Admin page (issues a simple cookie — not for production use).
app.MapPost("/demos/admin/login", async (HttpContext ctx) =>
{
    var claims = new List<Claim> { new(ClaimTypes.Name, "demo-user") };
    var identity = new ClaimsIdentity(claims, "Cookies");
    await ctx.SignInAsync(new ClaimsPrincipal(identity)).ConfigureAwait(false);
    return Results.Redirect("/demos/admin/wasm");
}).DisableAntiforgery();

app.MapPost("/demos/admin/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync().ConfigureAwait(false);
    return Results.Redirect("/demos/admin/wasm");
}).DisableAntiforgery();

await app.RunAsync().ConfigureAwait(true);

public partial class Program
{
    protected Program() { }
}