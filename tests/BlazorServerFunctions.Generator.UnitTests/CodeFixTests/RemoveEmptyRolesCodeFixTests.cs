using System;
using System.Linq;
using System.Threading.Tasks;
using BlazorServerFunctions.Generator.CodeFixes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace BlazorServerFunctions.Generator.UnitTests.CodeFixTests;

public sealed class RemoveEmptyRolesCodeFixTests
{
    [Fact]
    public async Task RemovesRoles_LeavingOtherArguments()
    {
        const string source = """
            using BlazorServerFunctions.Abstractions;
            using System.Threading.Tasks;

            [ServerFunctionCollection]
            public interface IFoo
            {
                [ServerFunction(HttpMethod = "GET", Roles = "")]
                Task<string> GetAsync();
            }
            """;

        var actual = await CodeFixTestHelper.ApplyFirstFixAsync(
            new RemoveEmptyRolesCodeFix(),
            "BSF021",
            DiagnosticSeverity.Error,
            source,
            root => root.DescendantNodes().OfType<MethodDeclarationSyntax>().First().Identifier.Span);

        Assert.DoesNotContain("Roles", actual, StringComparison.Ordinal);
        Assert.Contains("HttpMethod = \"GET\"", actual, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RemovesRoles_WhenOnlyArgument_RemovesParens()
    {
        const string source = """
            using BlazorServerFunctions.Abstractions;
            using System.Threading.Tasks;

            [ServerFunctionCollection]
            public interface IFoo
            {
                [ServerFunction(Roles = "")]
                Task<string> GetAsync();
            }
            """;

        var actual = await CodeFixTestHelper.ApplyFirstFixAsync(
            new RemoveEmptyRolesCodeFix(),
            "BSF021",
            DiagnosticSeverity.Error,
            source,
            root => root.DescendantNodes().OfType<MethodDeclarationSyntax>().First().Identifier.Span);

        Assert.DoesNotContain("Roles", actual, StringComparison.Ordinal);
        Assert.Contains("[ServerFunction]", actual, StringComparison.Ordinal);
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
                [ServerFunction(HttpMethod = "GET", Roles = "")]
                Task<string> GetAsync();
            }
            """;

        var titles = await CodeFixTestHelper.GetFixTitlesAsync(
            new RemoveEmptyRolesCodeFix(),
            "BSF021",
            DiagnosticSeverity.Error,
            source,
            root => root.DescendantNodes().OfType<MethodDeclarationSyntax>().First().Identifier.Span);

        var title = Assert.Single(titles);
        Assert.Equal("Remove empty Roles property", title);
    }
}
