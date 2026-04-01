namespace BlazorServerFunctions.Generator.Models;

internal sealed record ParameterInfo
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public bool HasDefaultValue { get; set; }
    public string? DefaultValue { get; set; }
    public bool IsRouteParameter { get; set; }
    public bool IsValueType { get; set; }
    public FileKind FileKind { get; set; } = FileKind.None;

    /// <summary>
    /// Explicit binding source from <c>[ServerFunctionParameter(From = ...)]</c>.
    /// <see cref="ParameterSource.Auto"/> means inferred (the default).
    /// </summary>
    public ParameterSource ExplicitSource { get; set; } = ParameterSource.Auto;

    /// <summary>
    /// Optional custom name from <c>[ServerFunctionParameter(Name = "...")]</c>.
    /// Used as the HTTP header name for <see cref="ParameterSource.Header"/>,
    /// or as the query key for <see cref="ParameterSource.Query"/>.
    /// </summary>
    public string? ExplicitName { get; set; }
}