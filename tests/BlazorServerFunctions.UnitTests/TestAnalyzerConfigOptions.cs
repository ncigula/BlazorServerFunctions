using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace BlazorServerFunctions.UnitTests;

public class TestAnalyzerConfigOptionsProvider(Dictionary<string, string> options) : AnalyzerConfigOptionsProvider
{
    public override AnalyzerConfigOptions GlobalOptions { get; } = new TestAnalyzerConfigOptions(options);
    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => new TestAnalyzerConfigOptions(options);
    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => new TestAnalyzerConfigOptions(options);
}

public class TestAnalyzerConfigOptions(Dictionary<string, string> options) : AnalyzerConfigOptions
{
    public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value) => options.TryGetValue(key, out value);
}
