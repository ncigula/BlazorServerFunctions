using System.Runtime.CompilerServices;

namespace BlazorServerFunctions.Generator.IntegrationTests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        DerivePathInfo((file, _, type, method) =>
            new(
                Path.Join(Path.GetDirectoryName(file), "_snapshots"),
                type.Name,
                method.Name));

        VerifySourceGenerators.Initialize();
    }
}
