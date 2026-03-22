using System;
using System.Linq;
using System.Threading.Tasks;
using BlazorServerFunctions.Generator.CodeFixes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace BlazorServerFunctions.Generator.UnitTests.CodeFixTests;

public sealed class RemoveEmptyCorsPolicyCodeFixTests
{
    [Fact]
    public async Task RemovesCorsPolicy_WhenOnlyArgument_RemovesParens()
    {
        const string source = """
            using BlazorServerFunctions.Abstractions;
            using System.Threading.Tasks;

            [ServerFunctionCollection(CorsPolicy = "")]
            public interface IFoo
            {
                [ServerFunction(HttpMethod = "GET")]
                Task<string> GetAsync();
            }
            """;

        var actual = await CodeFixTestHelper.ApplyFirstFixAsync(
            new RemoveEmptyCorsPolicyCodeFix(),
            "BSF022",
            DiagnosticSeverity.Error,
            source,
            root => root.DescendantNodes().OfType<InterfaceDeclarationSyntax>().First().Identifier.Span);

        Assert.DoesNotContain("CorsPolicy", actual, StringComparison.Ordinal);
        Assert.Contains("[ServerFunctionCollection]", actual, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RemovesCorsPolicy_LeavingRequireAuthorization()
    {
        const string source = """
            using BlazorServerFunctions.Abstractions;
            using System.Threading.Tasks;

            [ServerFunctionCollection(RequireAuthorization = true, CorsPolicy = "")]
            public interface IFoo
            {
                [ServerFunction(HttpMethod = "GET")]
                Task<string> GetAsync();
            }
            """;

        var actual = await CodeFixTestHelper.ApplyFirstFixAsync(
            new RemoveEmptyCorsPolicyCodeFix(),
            "BSF022",
            DiagnosticSeverity.Error,
            source,
            root => root.DescendantNodes().OfType<InterfaceDeclarationSyntax>().First().Identifier.Span);

        Assert.DoesNotContain("CorsPolicy", actual, StringComparison.Ordinal);
        Assert.Contains("RequireAuthorization = true", actual, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RegistersExactlyOneAction()
    {
        const string source = """
            using BlazorServerFunctions.Abstractions;
            using System.Threading.Tasks;

            [ServerFunctionCollection(CorsPolicy = "")]
            public interface IFoo
            {
                [ServerFunction(HttpMethod = "GET")]
                Task<string> GetAsync();
            }
            """;

        var titles = await CodeFixTestHelper.GetFixTitlesAsync(
            new RemoveEmptyCorsPolicyCodeFix(),
            "BSF022",
            DiagnosticSeverity.Error,
            source,
            root => root.DescendantNodes().OfType<InterfaceDeclarationSyntax>().First().Identifier.Span);

        var title = Assert.Single(titles);
        Assert.Equal("Remove empty CorsPolicy property", title);
    }
}
