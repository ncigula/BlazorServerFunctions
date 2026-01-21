namespace BlazorServerFunctions.Generator.UnitTests.Helpers;

internal sealed class ParameterInfoBuilder
{
    private readonly ParameterInfo _param = new()
    {
        Name = "",
        Type = "",
        HasDefaultValue = false,
        DefaultValue = null
    };

    internal ParameterInfoBuilder WithName(string name)
    {
        _param.Name = name;
        return this;
    }

    internal ParameterInfoBuilder WithType(string type)
    {
        _param.Type = type;
        return this;
    }

    internal ParameterInfoBuilder WithDefault(string defaultValue)
    {
        _param.HasDefaultValue = true;
        _param.DefaultValue = defaultValue;
        return this;
    }

    internal ParameterInfo Build() =>
        new ParameterInfo
        {
            Name = _param.Name,
            Type = _param.Type,
            HasDefaultValue = _param.HasDefaultValue,
            DefaultValue = _param.DefaultValue
        };
}