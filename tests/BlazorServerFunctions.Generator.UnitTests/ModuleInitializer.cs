using System.Runtime.CompilerServices;

namespace BlazorServerFunctions.Generator.UnitTests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        DerivePathInfo((file, projectDirectory, type, method) =>
        {
            var path = Path.Combine(projectDirectory, "_snapshots", type.Name);
            return new PathInfo(
                directory: path,
                typeName: type.Name,
                methodName: method.Name);
        });

        VerifySourceGenerators.Initialize();
    }
}