using Microsoft.CodeAnalysis;

namespace BlazorServerFunctions.Generator.Helpers;

/// <summary>
/// Wraps SourceProductionContext to automatically track whether any errors have been reported.
/// Use this instead of manually tracking hasErrors flags.
/// </summary>
public sealed class SourceProductionContextWrapper(SourceProductionContext context)
{
    /// <summary>
    /// True if any Error-level diagnostics have been reported.
    /// </summary>
    public bool HasErrors { get; private set; }

    /// <summary>
    /// Reports a diagnostic and automatically sets HasErrors if it's an error.
    /// </summary>
    public void ReportDiagnostic(Diagnostic diagnostic)
    {
        if (diagnostic.Severity == DiagnosticSeverity.Error)
            HasErrors = true;

        context.ReportDiagnostic(diagnostic);
    }

    /// <summary>
    /// Forwards CancellationToken from the underlying context.
    /// </summary>
    public CancellationToken CancellationToken => context.CancellationToken;
}