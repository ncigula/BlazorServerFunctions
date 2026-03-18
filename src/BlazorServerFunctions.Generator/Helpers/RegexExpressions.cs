using System;
using System.Text.RegularExpressions;

namespace BlazorServerFunctions.Generator.Helpers;

public static class RegexExpressions
{
    public static readonly Regex IsAsyncTypeRegex = new Regex(
        @"^\s*(?:System\.Threading\.Tasks\.)?(Task|ValueTask)(\s*<.*>)?\s*$",
        RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Singleline,
        TimeSpan.FromMilliseconds(1000));

    /// <summary>
    /// Matches route parameter tokens such as {id}, {id:int}, {id?}, {slug:minlength(3)}.
    /// Named group "name" captures the bare parameter name.
    /// </summary>
    public static readonly Regex RouteParameterRegex = new Regex(
        @"\{(?<name>[a-zA-Z_][a-zA-Z0-9_]*)(?:[?:][^}]*)?\}",
        RegexOptions.Compiled | RegexOptions.ExplicitCapture,
        TimeSpan.FromMilliseconds(1000));
}
