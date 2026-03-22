using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BlazorServerFunctions.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace BlazorServerFunctions.Generator.UnitTests.CodeFixTests;

internal static class CodeFixTestHelper
{
    internal static async Task<string> ApplyFirstFixAsync(
        CodeFixProvider provider,
        string diagnosticId,
        DiagnosticSeverity severity,
        string source,
        Func<SyntaxNode, TextSpan> spanSelector)
    {
        using var workspace = new Microsoft.CodeAnalysis.AdhocWorkspace();
        var document = CreateDocument(workspace, source);

        var syntaxRoot = await document.GetSyntaxRootAsync().ConfigureAwait(false);
        var syntaxTree = syntaxRoot!.SyntaxTree;

        var span = spanSelector(syntaxRoot);
        var diagnostic = CreateDiagnostic(diagnosticId, severity, syntaxTree, span);

        var actions = new List<CodeAction>();
        var ctx = new CodeFixContext(document, diagnostic, (a, _) => actions.Add(a), CancellationToken.None);
        await provider.RegisterCodeFixesAsync(ctx).ConfigureAwait(false);

        if (actions.Count == 0)
            return source;

        var ops = await actions[0].GetOperationsAsync(CancellationToken.None).ConfigureAwait(false);
        var applyOp = ops.OfType<ApplyChangesOperation>().First();
        var newDoc = applyOp.ChangedSolution.GetDocument(document.Id)!;
        return (await newDoc.GetTextAsync().ConfigureAwait(false)).ToString();
    }

    internal static async Task<string[]> GetFixTitlesAsync(
        CodeFixProvider provider,
        string diagnosticId,
        DiagnosticSeverity severity,
        string source,
        Func<SyntaxNode, TextSpan> spanSelector)
    {
        using var workspace = new Microsoft.CodeAnalysis.AdhocWorkspace();
        var document = CreateDocument(workspace, source);

        var syntaxRoot = await document.GetSyntaxRootAsync().ConfigureAwait(false);
        var syntaxTree = syntaxRoot!.SyntaxTree;
        var span = spanSelector(syntaxRoot);

        var diagnostic = CreateDiagnostic(diagnosticId, severity, syntaxTree, span);

        var actions = new List<CodeAction>();
        var ctx = new CodeFixContext(document, diagnostic, (a, _) => actions.Add(a), CancellationToken.None);
        await provider.RegisterCodeFixesAsync(ctx).ConfigureAwait(false);

        return actions.Select(a => a.Title).ToArray();
    }

    private static Document CreateDocument(Microsoft.CodeAnalysis.AdhocWorkspace workspace, string source)
    {
        var refs = new MetadataReference[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Threading.Tasks").Location),
            MetadataReference.CreateFromFile(typeof(ServerFunctionCollectionAttribute).Assembly.Location),
        };

        var projectInfo = Microsoft.CodeAnalysis.ProjectInfo.Create(
            ProjectId.CreateNewId(),
            VersionStamp.Create(),
            name: "Test",
            assemblyName: "Test",
            language: LanguageNames.CSharp)
            .WithMetadataReferences(refs)
            .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var project = workspace.AddProject(projectInfo);
        return project.AddDocument("Test.cs", source);
    }

    private static Diagnostic CreateDiagnostic(
        string id,
        DiagnosticSeverity severity,
        SyntaxTree tree,
        TextSpan span)
    {
        var descriptor = new DiagnosticDescriptor(
            id: id,
            title: id,
            messageFormat: id,
            category: "Usage",
            defaultSeverity: severity,
            isEnabledByDefault: true);
        return Diagnostic.Create(descriptor, Location.Create(tree, span));
    }
}
