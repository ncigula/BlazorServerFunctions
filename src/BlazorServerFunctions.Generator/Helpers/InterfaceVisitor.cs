using BlazorServerFunctions.Generator.Models;
using Microsoft.CodeAnalysis;

namespace BlazorServerFunctions.Generator.Helpers;

internal sealed class InterfaceVisitor : SymbolVisitor
{
    private readonly List<InterfaceInfo> _result;
    private readonly CancellationToken _cancellationToken;

    public InterfaceVisitor(List<InterfaceInfo> result, CancellationToken cancellationToken)
    {
        _result = result;
        _cancellationToken = cancellationToken;
    }

    public override void VisitNamespace(INamespaceSymbol symbol)
    {
        _cancellationToken.ThrowIfCancellationRequested();
        foreach (var member in symbol.GetMembers())
        {
            member.Accept(this);
        }
    }

    public override void VisitNamedType(INamedTypeSymbol symbol)
    {
        _cancellationToken.ThrowIfCancellationRequested();
        if (symbol.TypeKind == TypeKind.Interface)
        {
            var interfaceInfo = InterfaceParser.ParseInterface(symbol, _cancellationToken);
            if (interfaceInfo != null)
            {
                _result.Add(interfaceInfo);
            }
        }

        foreach (var member in symbol.GetTypeMembers())
        {
            member.Accept(this);
        }
    }
}