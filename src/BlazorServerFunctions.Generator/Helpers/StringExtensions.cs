using System.Globalization;

namespace BlazorServerFunctions.Generator.Helpers;

public static class StringExtensions
{
    public static string Capitalize(this string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        ReadOnlySpan<char> src = value.AsSpan();
        char first = char.ToUpper(src[0], CultureInfo.InvariantCulture);

        // Fast path: if length == 1, avoid allocation logic
        if (src.Length == 1)
            return new string(first, 1);

        // Allocate final string and fill it manually
        char[] buffer = new char[src.Length];
        buffer[0] = first;
        src.Slice(1).CopyTo(buffer.AsSpan(1));

        return new string(buffer);
    }
}