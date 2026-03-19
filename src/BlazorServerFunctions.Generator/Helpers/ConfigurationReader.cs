using BlazorServerFunctions.Generator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BlazorServerFunctions.Generator.Helpers;

/// <summary>
/// Reads compile-time configuration from a <see cref="ServerFunctionConfiguration"/> subclass
/// by walking the syntax tree of the class's parameterless constructor.
/// </summary>
internal static class ConfigurationReader
{
    private const string ConfigBaseClassName = "ServerFunctionConfiguration";

    /// <summary>
    /// Resolves a <see cref="ConfigurationInfo"/> from the given config type symbol.
    /// Walks the full inheritance chain base-first, accumulating constructor assignments.
    /// Falls back to <see cref="ConfigurationInfo.Default"/> for any class without source
    /// (e.g. from a referenced assembly with no PDB).
    /// <para>
    /// When <paramref name="interfaceSymbol"/> and <paramref name="compilation"/> are provided
    /// and source-based reading yields defaults, a cross-compilation manifest class
    /// (<c>__BsfConfig_{InterfaceName}</c>) is checked as a fallback.
    /// </para>
    /// </summary>
    public static ConfigurationInfo ReadConfiguration(
        INamedTypeSymbol configType,
        INamedTypeSymbol? interfaceSymbol = null,
        Compilation? compilation = null)
    {
        // Build inheritance chain: most-derived → base. We want base → derived for application order.
        var chain = new List<INamedTypeSymbol>();
        var current = configType;

        while (current is not null
               && !string.Equals(current.Name, "Object", StringComparison.Ordinal)
               && !string.Equals(current.Name, ConfigBaseClassName, StringComparison.Ordinal))
        {
            chain.Add(current);
            current = current.BaseType;
        }

        // Reverse so we apply base overrides first, derived last
        chain.Reverse();

        var config = ConfigurationInfo.Default;

        foreach (var type in chain)
        {
            config = ApplyConstructorAssignments(config, type);
        }

        // Cross-compilation fallback: when source reading yielded defaults (e.g. config class is in a
        // referenced compiled assembly), look for a __BsfConfig_{Interface} manifest class emitted by
        // the library project's generator run.
        if (config == ConfigurationInfo.Default
            && compilation is not null
            && interfaceSymbol is not null)
        {
            var fromManifest = TryReadFromManifest(interfaceSymbol, compilation);
            if (fromManifest is not null)
                return fromManifest;
        }

        return config;
    }

    /// <summary>
    /// Attempts to read <see cref="ConfigurationInfo"/> from a <c>__BsfConfig_{InterfaceName}</c>
    /// manifest class emitted by a library project's generator. Returns <c>null</c> if not found.
    /// </summary>
    /// <remarks>
    /// Uses namespace-symbol navigation rather than <c>GetTypeByMetadataName</c> so that
    /// the <c>internal</c> manifest class is found even when accessed from a different
    /// assembly (where <c>GetTypeByMetadataName</c> would refuse due to accessibility).
    /// </remarks>
    public static ConfigurationInfo? TryReadFromManifest(
        INamedTypeSymbol interfaceSymbol,
        Compilation compilation)
    {
        // Navigate the interface's own namespace to find the manifest — this works for internal
        // types from referenced assemblies, unlike GetTypeByMetadataName which enforces accessibility.
        var className = $"__BsfConfig_{interfaceSymbol.Name}";
        var manifestType = interfaceSymbol.ContainingNamespace
            .GetTypeMembers(className)
            .FirstOrDefault();

        if (manifestType is null)
            return null;

        var config = ConfigurationInfo.Default;

        foreach (var member in manifestType.GetMembers())
        {
            if (member is not IFieldSymbol { IsConst: true } field || field.ConstantValue is null)
                continue;

            config = field.Name switch
            {
                "__BaseRoute" when field.ConstantValue is string br && !string.IsNullOrEmpty(br)
                    => config with { BaseRoute = br },
                "__RouteNaming" when field.ConstantValue is int rn
                    => config with { RouteNaming = (RouteNaming)rn },
                "__DefaultHttpMethod" when field.ConstantValue is string dm
                    => config with { DefaultHttpMethod = string.IsNullOrEmpty(dm) ? null : dm },
                "__GenerateProblemDetails" when field.ConstantValue is bool gpd
                    => config with { GenerateProblemDetails = gpd },
                "__EnableResilience" when field.ConstantValue is bool er
                    => config with { EnableResilience = er },
                "__Nullable" when field.ConstantValue is bool nb
                    => config with { Nullable = nb },
                "__CustomHttpClientType" when field.ConstantValue is string ct
                    => config with { CustomHttpClientType = string.IsNullOrEmpty(ct) ? null : ct },
                "__ApiType" when field.ConstantValue is int at
                    => config with { ApiType = (ApiType)at },
                "__CacheSeconds" when field.ConstantValue is int cs
                    => config with { CacheSeconds = cs },
                "__RateLimitPolicy" when field.ConstantValue is string rlp
                    => config with { RateLimitPolicy = string.IsNullOrEmpty(rlp) ? null : rlp },
                _ => config
            };
        }

        return config;
    }

    private static ConfigurationInfo ApplyConstructorAssignments(ConfigurationInfo current, INamedTypeSymbol type)
    {
        var ctor = type.Constructors.FirstOrDefault(static c => c.Parameters.IsEmpty);
        if (ctor is null)
            return current;

        var syntaxRef = ctor.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxRef is null)
            return current;

        var ctorSyntax = syntaxRef.GetSyntax() as ConstructorDeclarationSyntax;
        if (ctorSyntax?.Body is null)
            return current;

        return ApplyStatements(current, ctorSyntax.Body.Statements);
    }

    private static ConfigurationInfo ApplyStatements(
        ConfigurationInfo current,
        SyntaxList<StatementSyntax> statements)
    {
        var baseRoute = current.BaseRoute;
        var routeNaming = current.RouteNaming;
        var defaultHttpMethod = current.DefaultHttpMethod;
        var generateProblemDetails = current.GenerateProblemDetails;
        var enableResilience = current.EnableResilience;
        var nullable = current.Nullable;
        var customHttpClientType = current.CustomHttpClientType;
        var apiType = current.ApiType;
        var cacheSeconds = current.CacheSeconds;
        var rateLimitPolicy = current.RateLimitPolicy;

        foreach (var statement in statements)
        {
            if (statement is not ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax assignment })
                continue;

            if (assignment.Left is not IdentifierNameSyntax propId)
                continue;

            ApplyAssignment(
                propId.Identifier.Text,
                assignment.Right,
                ref baseRoute,
                ref routeNaming,
                ref defaultHttpMethod,
                ref generateProblemDetails,
                ref enableResilience,
                ref nullable,
                ref customHttpClientType,
                ref apiType,
                ref cacheSeconds,
                ref rateLimitPolicy);
        }

        return current with
        {
            BaseRoute = baseRoute,
            RouteNaming = routeNaming,
            DefaultHttpMethod = defaultHttpMethod,
            GenerateProblemDetails = generateProblemDetails,
            EnableResilience = enableResilience,
            Nullable = nullable,
            CustomHttpClientType = customHttpClientType,
            ApiType = apiType,
            CacheSeconds = cacheSeconds,
            RateLimitPolicy = rateLimitPolicy,
        };
    }

#pragma warning disable MA0051 // Method too long — necessary switch dispatching
    private static void ApplyAssignment(
        string propName,
        ExpressionSyntax value,
        ref string baseRoute,
        ref RouteNaming routeNaming,
        ref string? defaultHttpMethod,
        ref bool generateProblemDetails,
        ref bool enableResilience,
        ref bool nullable,
        ref string? customHttpClientType,
        ref ApiType apiType,
        ref int cacheSeconds,
        ref string? rateLimitPolicy)
    {
        switch (propName)
        {
            case "BaseRoute":
                baseRoute = ExtractStringLiteral(value) ?? baseRoute;
                break;
            case "RouteNaming":
                routeNaming = ExtractEnumValue(value, routeNaming);
                break;
            case "DefaultHttpMethod":
                defaultHttpMethod = ExtractStringOrNull(value, defaultHttpMethod);
                break;
            case "GenerateProblemDetails":
                generateProblemDetails = ExtractBoolLiteral(value) ?? generateProblemDetails;
                break;
            case "EnableResilience":
                enableResilience = ExtractBoolLiteral(value) ?? enableResilience;
                break;
            case "Nullable":
                nullable = ExtractBoolLiteral(value) ?? nullable;
                break;
            case "CustomHttpClientType":
                customHttpClientType = ExtractTypeofTypeName(value);
                break;
            case "ApiType":
                apiType = ExtractEnumValue(value, apiType);
                break;
            case "CacheSeconds":
                cacheSeconds = ExtractIntLiteral(value) ?? cacheSeconds;
                break;
            case "RateLimitPolicy":
                rateLimitPolicy = ExtractStringOrNull(value, rateLimitPolicy);
                break;
        }
    }
#pragma warning restore MA0051

    private static string? ExtractStringLiteral(ExpressionSyntax expr)
    {
        if (expr is LiteralExpressionSyntax lit && lit.IsKind(SyntaxKind.StringLiteralExpression))
            return lit.Token.ValueText;
        return null;
    }

    /// <summary>Extracts a string literal, or null for a null literal / default expression.</summary>
    private static string? ExtractStringOrNull(ExpressionSyntax expr, string? fallback)
    {
        if (expr is LiteralExpressionSyntax lit)
        {
            if (lit.IsKind(SyntaxKind.NullLiteralExpression))
                return null;
            if (lit.IsKind(SyntaxKind.StringLiteralExpression))
                return lit.Token.ValueText;
        }

        if (expr is DefaultExpressionSyntax or PostfixUnaryExpressionSyntax)
            return null;

        return fallback;
    }

    private static int? ExtractIntLiteral(ExpressionSyntax expr)
    {
        if (expr is LiteralExpressionSyntax lit && lit.IsKind(SyntaxKind.NumericLiteralExpression)
            && lit.Token.Value is int i)
            return i;
        return null;
    }

    private static bool? ExtractBoolLiteral(ExpressionSyntax expr)
    {
        if (expr is not LiteralExpressionSyntax lit)
            return null;

        if (lit.IsKind(SyntaxKind.TrueLiteralExpression))
            return true;
        if (lit.IsKind(SyntaxKind.FalseLiteralExpression))
            return false;

        return null;
    }

    private static TEnum ExtractEnumValue<TEnum>(ExpressionSyntax expr, TEnum fallback)
        where TEnum : struct, Enum
    {
        // RouteNaming.KebabCase  →  member = "KebabCase"
        if (expr is MemberAccessExpressionSyntax memberAccess)
        {
            var member = memberAccess.Name.Identifier.Text;
            if (Enum.TryParse<TEnum>(member, ignoreCase: false, out var result))
                return result;
        }

        return fallback;
    }

    private static string? ExtractTypeofTypeName(ExpressionSyntax expr)
    {
        // typeof(MyHttpClient)  →  "MyHttpClient"
        if (expr is TypeOfExpressionSyntax typeOf)
            return typeOf.Type.ToString();

        // null or default → clear the type
        return null;
    }
}
