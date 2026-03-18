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
    /// </summary>
    public static ConfigurationInfo ReadConfiguration(INamedTypeSymbol configType)
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
                ref apiType);
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
        ref ApiType apiType)
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
