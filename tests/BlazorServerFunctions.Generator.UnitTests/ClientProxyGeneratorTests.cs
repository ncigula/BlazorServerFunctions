using System.Text;

namespace BlazorServerFunctions.Generator.UnitTests;

public class ClientProxyGeneratorTests
{
    [Theory]
    [InlineData("void", AsyncType.None)]
    [InlineData("Task", AsyncType.Task)]
    [InlineData("Task<T>", AsyncType.Task)]
    [InlineData("ValueTask", AsyncType.ValueTask)]
    [InlineData("ValueTask<T>", AsyncType.ValueTask)]
    public Task GenerateMethodSignature_Generates_MethodSignature(string returnType, AsyncType asyncType)
    {
        var method = new MethodInfoBuilder()
            .WithName("GetUser")
            .Returning(returnType)
            .IsAsyncMethod(asyncType)
            .UsingHttp("GET")
            .Build();

        StringBuilder sb = new StringBuilder();
        ClientProxyGenerator.GenerateMethodSignature(sb, method);

        return Verify(sb.ToString()).UseParameters(returnType);
    }

    [Fact]
    public Task GenerateMethodSignature_IncludesCancellationToken_When_MethodHasIt()
    {
        var method = new MethodInfoBuilder()
            .WithName("GetUser")
            .Returning("UserDto")
            .IsAsyncMethod(AsyncType.Task)
            .UsingHttp("GET")
            .WithParameter(
                new ParameterInfoBuilder()
                    .WithName("id")
                    .WithType("int")
                    .Build()
            )
            .WithCancellationToken()
            .Build();

        StringBuilder sb = new StringBuilder();
        ClientProxyGenerator.GenerateMethodSignature(sb, method);

        return Verify(sb.ToString());
    }
}
