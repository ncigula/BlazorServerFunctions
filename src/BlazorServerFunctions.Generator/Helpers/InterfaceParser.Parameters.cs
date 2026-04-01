using System.Text.RegularExpressions;
using BlazorServerFunctions.Generator.Models;
using Microsoft.CodeAnalysis;
using HttpMethod = BlazorServerFunctions.Generator.Models.HttpMethod;

namespace BlazorServerFunctions.Generator.Helpers;

internal sealed partial class InterfaceParser
{
    private static List<ParameterInfo> ParseParameters(IMethodSymbol methodSymbol)
    {
        return methodSymbol.Parameters
            .Where(p => !string.Equals(p.Type.ToDisplayString(), "System.Threading.CancellationToken", StringComparison.Ordinal))
            .Select(static parameter =>
            {
                var info = new ParameterInfo
                {
                    Name = parameter.Name,
                    Type = parameter.Type.ToDisplayString(),
                    HasDefaultValue = parameter.HasExplicitDefaultValue,
                    DefaultValue = parameter.HasExplicitDefaultValue && parameter.ExplicitDefaultValue is not null
                        ? Convert.ToString(parameter.ExplicitDefaultValue, System.Globalization.CultureInfo.InvariantCulture)
                        : null,
                    IsValueType = parameter.Type.IsValueType,
                    FileKind = DetectFileKind(parameter.Type.ToDisplayString()),
                };

                ParseServerFunctionParameterAttribute(parameter, info);
                return info;
            })
            .ToList();
    }

    private static void ParseServerFunctionParameterAttribute(IParameterSymbol parameter, ParameterInfo info)
    {
        var sfpAttr = parameter.GetAttributes()
            .FirstOrDefault(a => string.Equals(
                a.AttributeClass?.Name, "ServerFunctionParameterAttribute",
                StringComparison.Ordinal));

        if (sfpAttr is null)
            return;

        foreach (var arg in sfpAttr.NamedArguments)
        {
            switch (arg.Key)
            {
                case "From":
                    if (arg.Value.Value is int enumVal
                        && Enum.IsDefined(typeof(ParameterSource), enumVal))
                        info.ExplicitSource = (ParameterSource)enumVal;
                    break;
                case "Name":
                    info.ExplicitName = arg.Value.Value?.ToString();
                    break;
            }
        }
    }

    private static FileKind DetectFileKind(string typeName) => typeName switch
    {
        "System.IO.Stream" => FileKind.Stream,
        "Microsoft.AspNetCore.Http.IFormFile" => FileKind.FormFile,
        "Microsoft.AspNetCore.Http.IFormFileCollection" => FileKind.FormFileCollection,
        _ => FileKind.None,
    };

    /// <summary>
    /// Validates route parameters in CustomRoute against method parameters.
    /// Reports BSF017 for unmatched route params and BSF018 for complex-typed route params.
    /// Marks matched parameters with IsRouteParameter = true.
    /// </summary>
    private void ValidateAndMarkRouteParameters(IMethodSymbol methodSymbol, MethodInfo methodInfo)
    {
        if (methodInfo.CustomRoute is null)
            return;

        var matches = RegexExpressions.RouteParameterRegex.Matches(methodInfo.CustomRoute);
        foreach (Match match in matches)
        {
            var paramName = match.Groups["name"].Value;
            var idx = methodInfo.Parameters.FindIndex(
                p => string.Equals(p.Name, paramName, StringComparison.OrdinalIgnoreCase));

            if (idx < 0)
            {
                _context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.RouteParameterNotFound,
                    methodSymbol.Locations.First(),
                    methodInfo.Name,
                    paramName));
                continue;
            }

            var paramInfo = methodInfo.Parameters[idx];

            if (IsComplexRouteType(paramInfo.Type))
            {
                _context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.RouteParameterComplexType,
                    methodSymbol.Locations.First(),
                    methodInfo.Name,
                    paramName,
                    paramInfo.Type));
            }

            methodInfo.Parameters[idx] = paramInfo with { IsRouteParameter = true };
        }
    }

    private static bool IsComplexRouteType(string typeName)
    {
        var bare = typeName.EndsWith("?", StringComparison.Ordinal)
            ? typeName.AsSpan(0, typeName.Length - 1).ToString()
            : typeName;

        // Fully qualified names (contain '.') are assumed bindable
        if (bare.IndexOf('.') >= 0)
            return false;

        // Generic or array types are complex
        if (bare.IndexOf('<') >= 0 || bare.IndexOf('[') >= 0)
            return true;

        return !RouteBindableTypes.Contains(bare);
    }

    private static readonly HashSet<string> RouteBindableTypes = new(StringComparer.Ordinal)
    {
        "int", "long", "short", "byte", "uint", "ulong", "ushort", "sbyte",
        "float", "double", "decimal", "bool", "char", "string",
        "Guid", "DateTime", "DateTimeOffset", "DateOnly", "TimeOnly", "TimeSpan",
    };

    /// <summary>
    /// Validates explicit <see cref="ParameterSource"/> annotations after all other parsing is complete.
    /// BSF031: <see cref="ParameterSource.Route"/> specified but <c>{paramName}</c> not in route template.
    /// BSF032: <see cref="ParameterSource.Body"/> on GET or DELETE.
    /// </summary>
    private void ValidateExplicitParameterBindings(IMethodSymbol methodSymbol, MethodInfo methodInfo)
    {
        foreach (var param in methodInfo.Parameters)
        {
            if (param.ExplicitSource == ParameterSource.Route && !param.IsRouteParameter)
                _context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.ExplicitRouteParameterMissingFromTemplate,
                    methodSymbol.Locations.First(),
                    param.Name, methodInfo.Name));

            if (param.ExplicitSource == ParameterSource.Body
                && methodInfo.HttpMethod is HttpMethod.Get or HttpMethod.Delete)
                _context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.BodyParameterOnGetOrDelete,
                    methodSymbol.Locations.First(),
                    param.Name, methodInfo.Name));
        }
    }
}
