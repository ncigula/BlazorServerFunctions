using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<BlazorServerFunctions_Sample>("webApp");

await builder.Build().RunAsync();