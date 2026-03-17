using BlazorServerFunctions.Generator.Helpers;

namespace BlazorServerFunctions.Generator.UnitTests;

public class RegexExpressionTests
{
    [Theory]
    [InlineData("Task", true)]
    [InlineData("Task<T>", true)]
    [InlineData("ValueTask", true)]
    [InlineData("ValueTask<T>", true)]
    [InlineData("Type", false)]
    [InlineData("void", false)]
    public void AsyncReturnTypeRegex_Correctly_Parses(string methodReturnType, bool isAsync)
    {
        Assert.Equal(RegexExpressions.IsAsyncTypeRegex.IsMatch(methodReturnType), isAsync);
    }
}