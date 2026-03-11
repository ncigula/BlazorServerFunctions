namespace BlazorServerFunctions.Generator.UnitTests;

public class ServerEndpointGeneratorTests
{
    [Fact]
    public Task ServerEndpointGenerator_Generates_ClientProxy_With_One_Method()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("IUserService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("api/user-service")
            .RequiresAuthorization(false)
            .WithMethod(
                new MethodInfoBuilder()
                    .WithName("GetUser")
                    .Returning("UserDto")
                    .UsingHttp("GET")
                    .WithParameter(
                        new ParameterInfoBuilder()
                            .WithName("id")
                            .WithType("int")
                            .Build()
                    )
                    .Build()
            )
            .Build();

        var generated = ServerEndpointGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    [Fact]
    public Task ServerEndpointGenerator_UsesFromBody_For_Post_WithParameters()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("IOrderService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("orders")
            .RequiresAuthorization(false)
            .WithMethod(
                new MethodInfoBuilder()
                    .WithName("CreateOrder")
                    .Returning("OrderDto")
                    .UsingHttp("POST")
                    .WithParameter(
                        new ParameterInfoBuilder()
                            .WithName("name")
                            .WithType("string")
                            .Build()
                    )
                    .Build()
            )
            .Build();

        var generated = ServerEndpointGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    [Fact]
    public Task ServerEndpointGenerator_AppliesAuth_OnGroup_When_InterfaceRequiresAuth()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("IOrderService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("orders")
            .RequiresAuthorization(true)
            .WithMethod(
                new MethodInfoBuilder()
                    .WithName("GetOrder")
                    .Returning("OrderDto")
                    .UsingHttp("GET")
                    .RequiresAuthorization(true)
                    .Build()
            )
            .Build();

        var generated = ServerEndpointGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    [Fact]
    public Task ServerEndpointGenerator_AppliesAuth_OnEndpoint_When_OnlyMethodRequiresAuth()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("IOrderService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("orders")
            .RequiresAuthorization(false)
            .WithMethod(
                new MethodInfoBuilder()
                    .WithName("GetOrder")
                    .Returning("OrderDto")
                    .UsingHttp("GET")
                    .RequiresAuthorization(true)
                    .Build()
            )
            .Build();

        var generated = ServerEndpointGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }

    [Fact]
    public Task ServerEndpointGenerator_PassesCancellationToken_When_MethodHasIt()
    {
        var interfaceInfo = new InterfaceInfoBuilder()
            .WithName("IUserService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("users")
            .RequiresAuthorization(false)
            .WithMethod(
                new MethodInfoBuilder()
                    .WithName("GetUser")
                    .Returning("UserDto")
                    .UsingHttp("GET")
                    .WithCancellationToken()
                    .WithParameter(
                        new ParameterInfoBuilder()
                            .WithName("id")
                            .WithType("int")
                            .Build()
                    )
                    .Build()
            )
            .Build();

        var generated = ServerEndpointGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }
}
