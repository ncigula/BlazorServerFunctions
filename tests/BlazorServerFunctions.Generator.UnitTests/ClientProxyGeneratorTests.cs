namespace BlazorServerFunctions.Generator.UnitTests;

/// <summary>
/// Unit tests for ClientProxyGenerator - testing string output generation
/// </summary>
public class ClientProxyGeneratorTests
{
    [Fact]
    public Task Generate_BasicInterface_ProducesCorrectCode()
    {
        var interfaceInfo = TestDataFactory.BasicGetInterface();

        var generated = ClientProxyGenerator.Generate(interfaceInfo);

        return Verify(generated);
    }
}