using System.Globalization;
using System.Text;
using BlazorServerFunctions.Generator.Models;

namespace BlazorServerFunctions.Generator.Helpers;

public static class StringExtensions
{
    public static string ToPascalCase(this string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value!.Length == 1)
            return char.ToUpper(value[0], CultureInfo.InvariantCulture).ToString();

        return char.ToUpper(value[0], CultureInfo.InvariantCulture)
               + value.Substring(1);
    }

    /// <summary>
    /// Applies the given <see cref="RouteNaming"/> convention to a route segment.
    /// <see cref="RouteNaming.PascalCase"/> is a no-op (the default C# method name is already PascalCase).
    /// </summary>
    internal static string ApplyRouteNaming(this string segment, RouteNaming naming) =>
        naming switch
        {
            RouteNaming.CamelCase => segment.ToCamelCase(),
            RouteNaming.KebabCase => segment.ToKebabCase(),
            RouteNaming.SnakeCase => segment.ToSnakeCase(),
            _ => segment,
        };

    /// <summary>Lowercases the first character: <c>GetUser</c> → <c>getUser</c>.</summary>
    internal static string ToCamelCase(this string value)
    {
        if (string.IsNullOrEmpty(value) || !char.IsUpper(value[0]))
            return value;

        var chars = value.ToCharArray();
        chars[0] = char.ToLowerInvariant(chars[0]);
        return new string(chars);
    }

    /// <summary>
    /// Inserts a hyphen before each uppercase character (after position 0) and lowercases everything:
    /// <c>GetUserById</c> → <c>get-user-by-id</c>.
    /// </summary>
    internal static string ToKebabCase(this string value) =>
        SeparatedCase(value, '-');

    /// <summary>
    /// Inserts an underscore before each uppercase character (after position 0) and lowercases everything:
    /// <c>GetUserById</c> → <c>get_user_by_id</c>.
    /// </summary>
    internal static string ToSnakeCase(this string value) =>
        SeparatedCase(value, '_');

    /// <summary>Escapes backslashes and double-quotes so a string can be embedded in a C# string literal.</summary>
    internal static string EscapeStringLiteral(this string value) =>
        value.Replace("\\", "\\\\").Replace("\"", "\\\"");

    /// <summary>
    /// Extracts the generic type arguments from a Roslyn display-string type name such as
    /// <c>Result&lt;UserDto&gt;</c> or <c>Result&lt;UserDto, ValidationError&gt;</c>.
    /// Returns an empty array for non-generic types.
    /// Handles arbitrarily nested generics, e.g.
    /// <c>Result&lt;List&lt;UserDto&gt;, Error&gt;</c> → <c>["List&lt;UserDto&gt;", "Error"]</c>.
    /// </summary>
    internal static string[] ExtractGenericTypeArgs(this string typeString)
    {
        var ltIdx = typeString.IndexOf('<');
        if (ltIdx < 0)
            return [];

        // Strip the outer < … > to get the inner argument list.
        var inner = typeString.AsSpan(ltIdx + 1, typeString.Length - ltIdx - 2).ToString();

        var args = new List<string>();
        int depth = 0;
        int start = 0;

        for (int i = 0; i < inner.Length; i++)
        {
            var c = inner[i];
            if (c == '<') depth++;
            else if (c == '>') depth--;
            else if (c == ',' && depth == 0)
            {
                args.Add(inner.AsSpan(start, i - start).Trim().ToString());
                start = i + 1;
            }
        }

        args.Add(inner.Substring(start).Trim());
        return [.. args];
    }

    private static string SeparatedCase(string value, char separator)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var sb = new StringBuilder(value.Length + 4);
        foreach (var ch in value)
        {
            if (char.IsUpper(ch) && sb.Length > 0)
                sb.Append(separator);
            sb.Append(char.ToLowerInvariant(ch));
        }

        return sb.ToString();
    }
}