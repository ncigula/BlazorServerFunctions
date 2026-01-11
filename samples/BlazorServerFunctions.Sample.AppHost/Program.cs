using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<BlazorServerFunctions_Sample>("sample");

await builder.Build().RunAsync();