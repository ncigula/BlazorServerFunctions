using BlazorServerFunctions.Generator.Helpers;

namespace BlazorServerFunctions.Generator.UnitTests;

public class StringExtensionTests
{
    [Theory]
    [InlineData("", "")]
    [InlineData("a", "A")]
    [InlineData("aaaa", "Aaaa")]
    [InlineData("aaaa aaaa", "Aaaa aaaa")]
    [InlineData("lowerCase", "LowerCase")]
    public void ToPascalCase_Makes_First_Character_Uppercase(string input, string expected)
    {
        Assert.Equal(input.ToPascalCase(), expected);
    }
}