using System.Text.RegularExpressions;

namespace BlazorServerFunctions.Generator.Helpers;

public static partial class RegexExpressions
{
    [GeneratedRegex(
        @"^\s*(?:System\.Threading\.Tasks\.)?(Task|ValueTask)(\s*<[^>]+>)?\s*$",
        RegexOptions.Compiled | RegexOptions.ExplicitCapture,
        matchTimeoutMilliseconds: 1000)]
    public static partial Regex IsAsyncType();
}