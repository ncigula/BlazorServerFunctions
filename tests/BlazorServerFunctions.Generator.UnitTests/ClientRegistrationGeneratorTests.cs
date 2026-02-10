using HttpMethod = BlazorServerFunctions.Generator.Models.HttpMethod;

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
                    .UsingHttp(HttpMethod.Get)
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
}