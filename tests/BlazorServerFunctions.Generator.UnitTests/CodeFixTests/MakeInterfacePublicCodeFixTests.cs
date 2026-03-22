using System;
using System.Linq;
using System.Threading.Tasks;
using BlazorServerFunctions.Generator.CodeFixes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace BlazorServerFunctions.Generator.UnitTests.CodeFixTests;

public sealed class MakeInterfacePublicCodeFixTests
{
    [Fact]
    public async Task AddsPublicModifier_ReplacingInternal()
    {
        const string source = """
            using BlazorServerFunctions.Abstractions;
            using System.Threading.Tasks;

            [ServerFunctionCollection]
            internal interface IFoo
            {
                [ServerFunction(HttpMethod = "GET")]
                Task<string> GetAsync();
            }
            """;

        const string expected = """
            using BlazorServerFunctions.Abstractions;
            using System.Threading.Tasks;

            [ServerFunctionCollection]
            public interface IFoo
            {
                [ServerFunction(HttpMethod = "GET")]
                Task<string> GetAsync();
            }
            """;

        var actual = await CodeFixTestHelper.ApplyFirstFixAsync(
            new MakeInterfacePublicCodeFix(),
            "BSF003",
            DiagnosticSeverity.Error,
            source,
            root => root.DescendantNodes().OfType<InterfaceDeclarationSyntax>().First().Identifier.Span);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task AddsPublicModifier_ToImplicitlyInternalInterface()
    {
        const string source = """
            using BlazorServerFunctions.Abstractions;
            using System.Threading.Tasks;

            [ServerFunctionCollection]
            interface IFoo
            {
                [ServerFunction(HttpMethod = "GET")]
                Task<string> GetAsync();
            }
            """;

        var actual = await CodeFixTestHelper.ApplyFirstFixAsync(
            new MakeInterfacePublicCodeFix(),
            "BSF003",
            DiagnosticSeverity.Error,
            source,
            root => root.DescendantNodes().OfType<InterfaceDeclarationSyntax>().First().Identifier.Span);

        Assert.Contains("public interface IFoo", actual, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RegistersExactlyOneAction()
    {
        const string source = """
            using BlazorServerFunctions.Abstractions;
            using System.Threading.Tasks;

            [ServerFunctionCollection]
            internal interface IFoo
            {
                [ServerFunction(HttpMethod = "GET")]
                Task<string> GetAsync();
            }
            """;

        var titles = await CodeFixTestHelper.GetFixTitlesAsync(
            new MakeInterfacePublicCodeFix(),
            "BSF003",
            DiagnosticSeverity.Error,
            source,
            root => root.DescendantNodes().OfType<InterfaceDeclarationSyntax>().First().Identifier.Span);

        var title = Assert.Single(titles);
        Assert.Equal("Make interface public", title);
    }
}
