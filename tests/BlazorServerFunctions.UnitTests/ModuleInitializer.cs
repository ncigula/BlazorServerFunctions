using System.Runtime.CompilerServices;

namespace BlazorServerFunctions.UnitTests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifySourceGenerators.Initialize();
    }
}
