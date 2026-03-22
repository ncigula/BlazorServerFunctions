using System;
using System.Linq;
using System.Threading.Tasks;
using BlazorServerFunctions.Generator.CodeFixes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace BlazorServerFunctions.Generator.UnitTests.CodeFixTests;

public sealed class RemoveCacheSecondsCodeFixTests
{
    [Fact]
    public async Task RemovesCacheSeconds_LeavingOtherArguments()
    {
        const string source = """
            using BlazorServerFunctions.Abstractions;
            using System.Threading.Tasks;

            [ServerFunctionCollection]
            public interface IFoo
            {
                [ServerFunction(HttpMethod = "POST", CacheSeconds = 30)]
                Task<string> PostAsync();
            }
            """;

        var actual = await CodeFixTestHelper.ApplyFirstFixAsync(
            new RemoveCacheSecondsCodeFix(),
            "BSF020",
            DiagnosticSeverity.Error,
            source,
            root => root.DescendantNodes().OfType<MethodDeclarationSyntax>().First().Identifier.Span);

        Assert.DoesNotContain("CacheSeconds", actual, StringComparison.Ordinal);
        Assert.Contains("HttpMethod = \"POST\"", actual, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RemovesCacheSeconds_WhenLastArgument()
    {
        const string source = """
            using BlazorServerFunctions.Abstractions;
            using System.Threading.Tasks;

            [ServerFunctionCollection]
            public interface IFoo
            {
                [ServerFunction(HttpMethod = "POST", CacheSeconds = 60)]
                Task PostAsync();
            }
            """;

        var actual = await CodeFixTestHelper.ApplyFirstFixAsync(
            new RemoveCacheSecondsCodeFix(),
            "BSF020",
            DiagnosticSeverity.Error,
            source,
            root => root.DescendantNodes().OfType<MethodDeclarationSyntax>().First().Identifier.Span);

        Assert.DoesNotContain("CacheSeconds", actual, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RegistersExactlyOneAction()
    {
        const string source = """
            using BlazorServerFunctions.Abstractions;
            using System.Threading.Tasks;

            [ServerFunctionCollection]
            public interface IFoo
            {
                [ServerFunction(HttpMethod = "POST", CacheSeconds = 30)]
                Task<string> PostAsync();
            }
            """;

        var titles = await CodeFixTestHelper.GetFixTitlesAsync(
            new RemoveCacheSecondsCodeFix(),
            "BSF020",
            DiagnosticSeverity.Error,
            source,
            root => root.DescendantNodes().OfType<MethodDeclarationSyntax>().First().Identifier.Span);

        var title = Assert.Single(titles);
        Assert.Equal("Remove CacheSeconds (not valid on non-GET endpoints)", title);
    }
}
