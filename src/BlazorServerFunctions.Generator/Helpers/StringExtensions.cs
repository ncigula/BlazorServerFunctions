using System.Globalization;

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
}