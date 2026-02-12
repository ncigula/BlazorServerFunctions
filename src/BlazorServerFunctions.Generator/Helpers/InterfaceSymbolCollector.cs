using Microsoft.CodeAnalysis;

namespace BlazorServerFunctions.Generator.Helpers;

/// <summary>
/// Collects interface symbols from referenced assemblies without parsing them.
/// Parsing happens later in Execute() where SourceProductionContext is available.
/// </summary>
internal sealed class InterfaceSymbolCollector : SymbolVisitor
{
    private readonly List<INamedTypeSymbol> _result;
    private readonly CancellationToken _cancellationToken;

    public InterfaceSymbolCollector(List<INamedTypeSymbol> result, CancellationToken cancellationToken)
    {
        _result = result;
        _cancellationToken = cancellationToken;
    }

    public override void VisitNamespace(INamespaceSymbol symbol)
    {
        _cancellationToken.ThrowIfCancellationRequested();
        foreach (var member in symbol.GetMembers())
            member.Accept(this);
    }

    public override void VisitNamedType(INamedTypeSymbol symbol)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        if (symbol.TypeKind == TypeKind.Interface)
        {
            // Quick check: does it have [ServerFunctionCollection]?
            var hasAttribute = symbol.GetAttributes()
                .Any(a => string.Equals(
                    a.AttributeClass?.Name,
                    "ServerFunctionCollectionAttribute",
                    StringComparison.OrdinalIgnoreCase));

            if (hasAttribute)
                _result.Add(symbol); // Just collect, don't parse yet
        }

        foreach (var member in symbol.GetTypeMembers())
            member.Accept(this);
    }
}