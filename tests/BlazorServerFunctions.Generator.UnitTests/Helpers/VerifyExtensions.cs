namespace BlazorServerFunctions.Generator.UnitTests.Helpers;

internal static class VerifyExtensions
{
    internal static SettingsTask VerifyNoDiagnostics(this GeneratorDriverRunResult result)
    {
        Assert.Empty(result.Diagnostics);
        Assert.True(result.GeneratedTrees.Any(), "No sources were generated.");
        return Verify(result);
    }

    internal static void AssertDiagnostic(this GeneratorDriverRunResult result, string diagnosticId)
    {
        Assert.Contains(result.Diagnostics, d => string.Equals(d.Id, diagnosticId, StringComparison.Ordinal));
    }
}