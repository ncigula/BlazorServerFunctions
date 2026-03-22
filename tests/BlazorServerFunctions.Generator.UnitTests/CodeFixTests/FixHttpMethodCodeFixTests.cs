using System;
using System.Linq;
using System.Threading.Tasks;
using BlazorServerFunctions.Generator.CodeFixes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace BlazorServerFunctions.Generator.UnitTests.CodeFixTests;

public sealed class FixHttpMethodCodeFixTests
{
    [Fact]
    public async Task ReplacesInvalidVerb_WithFirstAction()
    {
        const string source = """
            using BlazorServerFunctions.Abstractions;
            using System.Threading.Tasks;

            [ServerFunctionCollection]
            public interface IFoo
            {
                [ServerFunction(HttpMethod = "INVALID")]
                Task<string> GetAsync();
            }
            """;

        var actual = await CodeFixTestHelper.ApplyFirstFixAsync(
            new FixHttpMethodCodeFix(),
            "BSF013",
            DiagnosticSeverity.Error,
            source,
            root => root.DescendantNodes().OfType<MethodDeclarationSyntax>().First().Identifier.Span);

        Assert.Contains("HttpMethod = \"GET\"", actual, StringComparison.Ordinal);
        Assert.DoesNotContain("INVALID", actual, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RegistersFiveActions_OnePerVerb()
    {
        const string source = """
            using BlazorServerFunctions.Abstractions;
            using System.Threading.Tasks;

            [ServerFunctionCollection]
            public interface IFoo
            {
                [ServerFunction(HttpMethod = "INVALID")]
                Task<string> GetAsync();
            }
            """;

        var titles = await CodeFixTestHelper.GetFixTitlesAsync(
            new FixHttpMethodCodeFix(),
            "BSF013",
            DiagnosticSeverity.Error,
            source,
            root => root.DescendantNodes().OfType<MethodDeclarationSyntax>().First().Identifier.Span);

        Assert.Equal(5, titles.Length);
        Assert.Contains("Change to HttpMethod = \"GET\"", titles);
        Assert.Contains("Change to HttpMethod = \"POST\"", titles);
    }

    [Fact]
    public async Task PreservesOtherArguments_WhenReplacingVerb()
    {
        const string source = """
            using BlazorServerFunctions.Abstractions;
            using System.Threading.Tasks;

            [ServerFunctionCollection]
            public interface IFoo
            {
                [ServerFunction(HttpMethod = "WRONG", RequireAuthorization = true)]
                Task<string> GetAsync();
            }
            """;

        var actual = await CodeFixTestHelper.ApplyFirstFixAsync(
            new FixHttpMethodCodeFix(),
            "BSF013",
            DiagnosticSeverity.Error,
            source,
            root => root.DescendantNodes().OfType<MethodDeclarationSyntax>().First().Identifier.Span);

        Assert.Contains("HttpMethod = \"GET\"", actual, StringComparison.Ordinal);
        Assert.Contains("RequireAuthorization = true", actual, StringComparison.Ordinal);
    }
}
