using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace BlazorServerFunctions.UnitTests;

public class TestAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
{
    private readonly Dictionary<string, string> _options;

    public TestAnalyzerConfigOptionsProvider(Dictionary<string, string> options)
    {
        _options = options;
        GlobalOptions = new TestAnalyzerConfigOptions(options);
    }

    public override AnalyzerConfigOptions GlobalOptions { get; }

    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => new TestAnalyzerConfigOptions(_options);
    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => new TestAnalyzerConfigOptions(_options);
}

public class TestAnalyzerConfigOptions : AnalyzerConfigOptions
{
    private readonly Dictionary<string, string> _options;

    public TestAnalyzerConfigOptions(Dictionary<string, string> options)
    {
        _options = options;
    }

    public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
    {
        return _options.TryGetValue(key, out value);
    }
}
