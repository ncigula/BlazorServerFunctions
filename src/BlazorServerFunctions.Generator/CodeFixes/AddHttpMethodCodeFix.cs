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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddHttpMethodCodeFix))]
[Shared]
public sealed class AddHttpMethodCodeFix : CodeFixProvider
{
    private static readonly string[] ValidVerbs = { "GET", "POST", "PUT", "DELETE", "PATCH" };

    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.HttpMethodRequired.Id);

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

        var capturedRoot = root;
        var capturedAttr = attribute;
        foreach (var verb in ValidVerbs)
        {
            var capturedVerb = verb;
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: $"Set HttpMethod = \"{capturedVerb}\"",
                    createChangedDocument: _ =>
                    {
                        var httpMethodArg = CodeFixHelper.FindNamedArgument(capturedAttr, "HttpMethod");

                        AttributeSyntax newAttr;
                        if (httpMethodArg is null)
                        {
                            var newArg = SyntaxFactory.AttributeArgument(
                                SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName("HttpMethod")),
                                nameColon: null,
                                expression: SyntaxFactory.LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    SyntaxFactory.Literal(capturedVerb)));

                            var newArgList = capturedAttr.ArgumentList is null
                                ? SyntaxFactory.AttributeArgumentList(SyntaxFactory.SingletonSeparatedList(newArg))
                                : capturedAttr.ArgumentList.AddArguments(newArg);

                            newAttr = capturedAttr.WithArgumentList(newArgList);
                        }
                        else
                        {
                            var newArg = httpMethodArg.WithExpression(
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    SyntaxFactory.Literal(capturedVerb)));
                            newAttr = capturedAttr.ReplaceNode(httpMethodArg, newArg);
                        }

                        var newRoot = capturedRoot.ReplaceNode(capturedAttr, newAttr);
                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    },
                    equivalenceKey: $"AddHttpMethod_{capturedVerb}"),
                diagnostic);
        }
    }
}
