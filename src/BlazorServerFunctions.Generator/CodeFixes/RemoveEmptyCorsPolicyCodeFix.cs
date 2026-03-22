using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using BlazorServerFunctions.Generator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BlazorServerFunctions.Generator.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveEmptyCorsPolicyCodeFix))]
[Shared]
public sealed class RemoveEmptyCorsPolicyCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.EmptyCorsPolicy.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var diagnostic = context.Diagnostics[0];
        var node = root.FindNode(diagnostic.Location.SourceSpan);
        var interfaceDecl = node.AncestorsAndSelf().OfType<InterfaceDeclarationSyntax>().FirstOrDefault();
        if (interfaceDecl is null)
            return;

        var attribute = interfaceDecl.AttributeLists
            .SelectMany(al => al.Attributes)
            .FirstOrDefault(CodeFixHelper.IsServerFunctionCollectionAttribute);
        if (attribute is null)
            return;

        var corsPolicyArg = CodeFixHelper.FindNamedArgument(attribute, "CorsPolicy");
        if (corsPolicyArg is null)
            return;

        var capturedRoot = root;
        var capturedAttr = attribute;
        var capturedArg = corsPolicyArg;
        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Remove empty CorsPolicy property",
                createChangedDocument: _ =>
                {
                    var newAttr = CodeFixHelper.RemoveArgument(capturedAttr, capturedArg);
                    var newRoot = capturedRoot.ReplaceNode(capturedAttr, newAttr);
                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                },
                equivalenceKey: "RemoveEmptyCorsPolicy"),
            diagnostic);
    }
}
