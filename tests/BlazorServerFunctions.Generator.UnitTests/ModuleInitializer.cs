using System.Runtime.CompilerServices;

namespace BlazorServerFunctions.Generator.UnitTests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        DerivePathInfo((file, _, type, method) =>
        {
            var directory = Path.GetDirectoryName(file)!;
        
            return new PathInfo(
                directory: Path.Combine(directory, "_snapshots"),
                typeName: type.Name,
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