using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BlazorServerFunctions.Generator.CodeFixes;

internal static class CodeFixHelper
{
    internal static AttributeArgumentSyntax? FindNamedArgument(AttributeSyntax attribute, string name) =>
        attribute.ArgumentList?.Arguments.FirstOrDefault(
            a => string.Equals(a.NameEquals?.Name.Identifier.Text, name, System.StringComparison.Ordinal));

    internal static AttributeSyntax RemoveArgument(AttributeSyntax attribute, AttributeArgumentSyntax argument)
    {
        var remaining = attribute.ArgumentList!.Arguments.Remove(argument);
        var newArgList = remaining.Count == 0 ? null : attribute.ArgumentList.WithArguments(remaining);
        return attribute.WithArgumentList(newArgList);
    }

    internal static bool IsServerFunctionAttribute(AttributeSyntax attr)
    {
        var name = attr.Name.ToString();
        return string.Equals(name, "ServerFunction", System.StringComparison.Ordinal)
            || string.Equals(name, "ServerFunctionAttribute", System.StringComparison.Ordinal);
    }

    internal static bool IsServerFunctionCollectionAttribute(AttributeSyntax attr)
    {
        var name = attr.Name.ToString();
        return string.Equals(name, "ServerFunctionCollection", System.StringComparison.Ordinal)
            || string.Equals(name, "ServerFunctionCollectionAttribute", System.StringComparison.Ordinal);
    }
}
