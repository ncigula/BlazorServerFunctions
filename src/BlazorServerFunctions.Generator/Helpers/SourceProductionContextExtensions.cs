using BlazorServerFunctions.Generator.Models;
using Microsoft.CodeAnalysis;

namespace BlazorServerFunctions.Generator.Helpers;

internal static class SourceProductionContextExtensions
{
    internal static void ReportError(this SourceProductionContext context, Error error, INamedTypeSymbol symbol)
    {
        context.ReportDiagnostic(
            Diagnostic.Create(
                descriptor: error.Descriptor,
                location: symbol.Locations.FirstOrDefault(),
                //additionalLocations: symbol.Locations.Skip(1),
                messageArgs: error.MessageArgs.ToArray()));
    }
}