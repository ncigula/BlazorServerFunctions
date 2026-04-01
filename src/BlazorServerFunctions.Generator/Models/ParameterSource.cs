namespace BlazorServerFunctions.Generator.Models;

/// <summary>
/// Internal mirror of <c>BlazorServerFunctions.Abstractions.ParameterSource</c>.
/// Values must stay in sync with the Abstractions enum so that integer casts
/// from Roslyn attribute data (which carries the Abstractions enum ordinal) produce
/// the correct internal value.
/// </summary>
internal enum ParameterSource
{
    Auto = 0,
    Route = 1,
    Query = 2,
    Body = 3,
    Header = 4,
}
