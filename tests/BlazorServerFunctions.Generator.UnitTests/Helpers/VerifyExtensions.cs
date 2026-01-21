namespace BlazorServerFunctions.Generator.UnitTests.Helpers;

internal static class VerifyExtensions
{
    internal static SettingsTask VerifyNoDiagnostics(GeneratorDriverRunResult result)
    {
        Assert.Empty(result.Diagnostics);
        return Verify(result);
    }
}