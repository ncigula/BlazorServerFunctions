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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakeInterfacePublicCodeFix))]
[Shared]
public sealed class MakeInterfacePublicCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.InterfaceMustBePublic.Id);

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

        var capturedRoot = root;
        var capturedDecl = interfaceDecl;
        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Make interface public",
                createChangedDocument: _ =>
                {
                    var accessKinds = new[] { SyntaxKind.InternalKeyword, SyntaxKind.PrivateKeyword, SyntaxKind.ProtectedKeyword };
                    var modifiers = capturedDecl.Modifiers;

                    foreach (var kind in accessKinds)
                    {
                        var existing = modifiers.FirstOrDefault(m => m.IsKind(kind));
                        if (existing != default)
                            modifiers = modifiers.Remove(existing);
                    }

                    var publicToken = SyntaxFactory.Token(SyntaxKind.PublicKeyword)
                        .WithTrailingTrivia(SyntaxFactory.Space);
                    modifiers = modifiers.Insert(0, publicToken);

                    var newDecl = capturedDecl.WithModifiers(modifiers);
                    var newRoot = capturedRoot.ReplaceNode(capturedDecl, newDecl);
                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                },
                equivalenceKey: "MakeInterfacePublic"),
            diagnostic);
    }
}
