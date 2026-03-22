using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using BlazorServerFunctions.Generator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BlazorServerFunctions.Generator.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FixHttpMethodCodeFix))]
[Shared]
public sealed class FixHttpMethodCodeFix : CodeFixProvider
{
    private static readonly string[] ValidVerbs = { "GET", "POST", "PUT", "DELETE", "PATCH" };

    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.InvalidHttpMethod.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var diagnostic = context.Diagnostics[0];
        var node = root.FindNode(diagnostic.Location.SourceSpan);
        var methodDecl = node.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().FirstOrDefault();
        if (methodDecl is null)
            return;

        var attribute = methodDecl.AttributeLists
            .SelectMany(al => al.Attributes)
            .FirstOrDefault(CodeFixHelper.IsServerFunctionAttribute);
        if (attribute is null)
            return;

        var httpMethodArg = CodeFixHelper.FindNamedArgument(attribute, "HttpMethod");
        if (httpMethodArg is null)
            return;

        var capturedRoot = root;
        var capturedArg = httpMethodArg;
        foreach (var verb in ValidVerbs)
        {
            var capturedVerb = verb;
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: $"Change to HttpMethod = \"{capturedVerb}\"",
                    createChangedDocument: _ =>
                    {
                        var newArg = capturedArg.WithExpression(
                            SyntaxFactory.LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                SyntaxFactory.Literal(capturedVerb)));
                        var newRoot = capturedRoot.ReplaceNode(capturedArg, newArg);
                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    },
                    equivalenceKey: $"FixHttpMethod_{capturedVerb}"),
                diagnostic);
        }
    }
}
