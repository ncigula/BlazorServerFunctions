using System.Globalization;
using Microsoft.CodeAnalysis;

namespace BlazorServerFunctions.Generator.Models;

public sealed record Error
{
    public string Code { get; }
    public string Description { get; }
    public ErrorType Type { get; }

    // Optional Roslyn diagnostic metadata
    public DiagnosticDescriptor Descriptor { get; } = new(
        id: "BSG000",
        title: "Unknown error",
        messageFormat: "Unknown error",
        category: "None",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public IEnumerable<object> MessageArgs { get; } = [];

    public Error(
        string code,
        string description,
        ErrorType type,
        DiagnosticDescriptor? descriptor = null,
        params object[] messageArgs)
    {
        Code = code;
        Description = description;
        Type = type;
        
        if (descriptor is not null)
            Descriptor = descriptor;
        
        MessageArgs = messageArgs;
    }

    public static readonly Error None =
        new(string.Empty, string.Empty, ErrorType.Failure);
    
    public static readonly Error NullValue = new(
        "General.Null",
        "Null value was provided",
        ErrorType.Failure);


    public static Error Failure(string code, string description) =>
        new(code, description, ErrorType.Failure);

    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    public static Error Problem(string code, string description) =>
        new(code, description, ErrorType.Problem);

    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);

    public static Error Diagnostic(
        DiagnosticDescriptor descriptor,
        params object[] args)
        => new(
            descriptor.Id,
            descriptor.Title.ToString(CultureInfo.InvariantCulture),
            ErrorType.Problem,
            descriptor,
            args);
}