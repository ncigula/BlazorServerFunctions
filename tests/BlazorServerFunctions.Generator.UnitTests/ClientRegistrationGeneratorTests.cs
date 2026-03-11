namespace BlazorServerFunctions.Generator.UnitTests;

public class ClientRegistrationGeneratorTests
{
    [Fact]
    public Task ClientRegistrationGenerator_Generates_ClientProxy_With_One_Method()
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

        var generated = ClientRegistrationGenerator.Generate([interfaceInfo]);

        return Verify(generated);
    }

    [Fact]
    public Task ClientRegistrationGenerator_UsesCommonNamespace_When_InterfacesAreInDifferentNamespaces()
    {
        var userService = new InterfaceInfoBuilder()
            .WithName("IUserService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("users")
            .Build();

        var orderService = new InterfaceInfoBuilder()
            .WithName("IOrderService")
            .WithNamespace("MyApp.Orders")
            .WithRoutePrefix("orders")
            .Build();

        var generated = ClientRegistrationGenerator.Generate([userService, orderService]);

        return Verify(generated);
    }
}
