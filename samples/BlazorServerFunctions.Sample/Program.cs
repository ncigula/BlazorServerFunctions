using System.Security.Claims;
using BlazorServerFunctions.Sample;
using BlazorServerFunctions.Sample.Components;
using BlazorServerFunctions.Sample.Components.Admin;
using BlazorServerFunctions.Sample.Components.Caching;
using BlazorServerFunctions.Sample.Components.Crud;
using BlazorServerFunctions.Sample.Components.RateLimiting;
using BlazorServerFunctions.Sample.Components.Echo;
using BlazorServerFunctions.Sample.Components.RouteParams;
using BlazorServerFunctions.Sample.Components.Streaming;
using BlazorServerFunctions.Sample.Components.Weather;
using BlazorServerFunctions.Sample.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.RateLimiting;

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
        // return 401/403 with a ProblemDetails JSON body so the generated client
        // throws HttpRequestException with the correct status code and detail.
        options.Events.OnRedirectToLogin = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            ctx.Response.ContentType = "application/problem+json";
            return ctx.Response.WriteAsync(
                """{"status":401,"title":"Unauthorized","detail":"Authentication is required."}""");
        };
        options.Events.OnRedirectToAccessDenied = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            ctx.Response.ContentType = "application/problem+json";
            return ctx.Response.WriteAsync(
                """{"status":403,"title":"Forbidden","detail":"Insufficient permissions."}""");
        };
    });
builder.Services.AddAuthorization(options =>
    // "AdminOnly" policy requires the "Admin" role — used by IAdminService.GetPolicySecretAsync
    // and GetRoleAndPolicySecretAsync to demonstrate named policy enforcement alongside Roles.
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin")));
builder.Services.AddProblemDetails();

builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IEchoService, EchoService>();
builder.Services.AddSingleton<ICrudService, CrudService>();
builder.Services.AddScoped<IRouteParamService, RouteParamService>();
builder.Services.AddScoped<IStreamingService, StreamingService>();
builder.Services.AddSingleton<ICacheableService, CacheableService>();
builder.Services.AddSingleton<IRateLimitedService, RateLimitedService>();

builder.Services.AddOutputCache();

builder.Services.AddRateLimiter(options =>
    options.AddFixedWindowLimiter("fixed", limiter =>
    {
        limiter.PermitLimit = 10;
        limiter.Window = TimeSpan.FromSeconds(10);
    }));

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
app.UseOutputCache();
app.UseRateLimiter();

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

// Signs in as a user with the "Admin" role — used in E2E tests for role/policy scenarios.
app.MapPost("/demos/admin/login/admin", async (HttpContext ctx) =>
{
    var claims = new List<Claim>
    {
        new(ClaimTypes.Name, "admin-user"),
        new(ClaimTypes.Role, "Admin"),
    };
    var identity = new ClaimsIdentity(claims, "Cookies");
    await ctx.SignInAsync(new ClaimsPrincipal(identity)).ConfigureAwait(false);
    return Results.Ok();
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