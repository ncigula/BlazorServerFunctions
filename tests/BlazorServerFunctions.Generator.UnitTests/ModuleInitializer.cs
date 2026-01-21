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
                typeName: "_",
                methodName: method.Name);
        });

        VerifySourceGenerators.Initialize();
    }

    // private static string FindProjectRoot(string file)
    // {
    //     string? dir = Path.GetDirectoryName(file);
    //
    //     if (dir is null)
    //         return Path.Join(Path.GetDirectoryName(file), SnapshotDirectory);
    //
    //     while (dir is not null)
    //     {
    //         if (Directory.EnumerateFiles(dir, "*.csproj").Any())
    //             return dir;
    //
    //         dir = Directory.GetParent(dir)?.FullName;
    //     }
    //
    //     return dir!;
    // }

}