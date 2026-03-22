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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveCacheSecondsCodeFix))]
[Shared]
public sealed class RemoveCacheSecondsCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.CacheOnNonGetMethod.Id);

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

        var cacheArg = CodeFixHelper.FindNamedArgument(attribute, "CacheSeconds");
        if (cacheArg is null)
            return;

        var capturedRoot = root;
        var capturedAttr = attribute;
        var capturedArg = cacheArg;
        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Remove CacheSeconds (not valid on non-GET endpoints)",
                createChangedDocument: _ =>
                {
                    var newAttr = CodeFixHelper.RemoveArgument(capturedAttr, capturedArg);
                    var newRoot = capturedRoot.ReplaceNode(capturedAttr, newAttr);
                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                },
                equivalenceKey: "RemoveCacheSeconds"),
            diagnostic);
    }
}
