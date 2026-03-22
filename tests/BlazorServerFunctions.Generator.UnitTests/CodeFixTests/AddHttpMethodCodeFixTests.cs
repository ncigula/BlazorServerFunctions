using System;
using System.Linq;
using System.Threading.Tasks;
using BlazorServerFunctions.Generator.CodeFixes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace BlazorServerFunctions.Generator.UnitTests.CodeFixTests;

public sealed class AddHttpMethodCodeFixTests
{
    [Fact]
    public async Task AddsHttpMethod_WhenAttributeHasNoArguments()
    {
        const string source = """
            using BlazorServerFunctions.Abstractions;
            using System.Threading.Tasks;

            [ServerFunctionCollection]
            public interface IFoo
            {
                [ServerFunction]
                Task<string> GetAsync();
            }
            """;

        var actual = await CodeFixTestHelper.ApplyFirstFixAsync(
            new AddHttpMethodCodeFix(),
            "BSF012",
            DiagnosticSeverity.Error,
            source,
            root => root.DescendantNodes().OfType<MethodDeclarationSyntax>().First().Identifier.Span);

        Assert.Contains("HttpMethod = \"GET\"", actual, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AddsHttpMethod_WhenAttributeHasOtherArguments()
    {
        const string source = """
            using BlazorServerFunctions.Abstractions;
            using System.Threading.Tasks;

            [ServerFunctionCollection]
            public interface IFoo
            {
                [ServerFunction(RequireAuthorization = true)]
                Task<string> GetAsync();
            }
            """;

        var actual = await CodeFixTestHelper.ApplyFirstFixAsync(
            new AddHttpMethodCodeFix(),
            "BSF012",
            DiagnosticSeverity.Error,
            source,
            root => root.DescendantNodes().OfType<MethodDeclarationSyntax>().First().Identifier.Span);

        Assert.Contains("HttpMethod = \"GET\"", actual, StringComparison.Ordinal);
        Assert.Contains("RequireAuthorization = true", actual, StringComparison.Ordinal);
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
                [ServerFunction]
                Task<string> GetAsync();
            }
            """;

        var titles = await CodeFixTestHelper.GetFixTitlesAsync(
            new AddHttpMethodCodeFix(),
            "BSF012",
            DiagnosticSeverity.Error,
            source,
            root => root.DescendantNodes().OfType<MethodDeclarationSyntax>().First().Identifier.Span);

        Assert.Equal(5, titles.Length);
        Assert.Contains("Set HttpMethod = \"GET\"", titles);
        Assert.Contains("Set HttpMethod = \"POST\"", titles);
        Assert.Contains("Set HttpMethod = \"PUT\"", titles);
        Assert.Contains("Set HttpMethod = \"DELETE\"", titles);
        Assert.Contains("Set HttpMethod = \"PATCH\"", titles);
    }
}
