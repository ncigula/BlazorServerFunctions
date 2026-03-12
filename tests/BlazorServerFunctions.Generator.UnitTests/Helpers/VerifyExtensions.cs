namespace BlazorServerFunctions.Generator.UnitTests.Helpers;

internal static class VerifyExtensions
{
    internal static SettingsTask VerifyNoDiagnostics(this GeneratorDriverRunResult result)
    {
        Assert.Empty(result.Diagnostics);
        Assert.True(result.GeneratedTrees.Any(), "No sources were generated.");
        return Verify(result);
    }
}