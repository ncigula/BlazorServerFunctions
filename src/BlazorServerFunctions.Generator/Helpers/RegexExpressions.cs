using System;
using System.Text.RegularExpressions;

namespace BlazorServerFunctions.Generator.Helpers;

public static class RegexExpressions
{
    public static readonly Regex IsAsyncTypeRegex = new Regex(
        @"^\s*(?:System\.Threading\.Tasks\.)?(Task|ValueTask)(\s*<.*>)?\s*$",
        RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Singleline,
        TimeSpan.FromMilliseconds(1000));
}
