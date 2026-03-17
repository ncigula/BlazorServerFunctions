#pragma warning disable S2094  // Empty class is intentional for compiler polyfill
#pragma warning disable MA0048 // Multiple types in file is intentional for polyfills
// ReSharper disable All

// Required for C# 9 init-only setters and records on netstandard2.0
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}

// Required for nullable analysis attributes not present in netstandard2.0
namespace System.Diagnostics.CodeAnalysis
{
    [System.AttributeUsage(
        System.AttributeTargets.Parameter |
        System.AttributeTargets.Property |
        System.AttributeTargets.ReturnValue,
        AllowMultiple = false)]
    internal sealed class NotNullAttribute : System.Attribute { }
}

#pragma warning restore MA0048
#pragma warning restore S2094
