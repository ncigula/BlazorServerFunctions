using System.Runtime.CompilerServices;

namespace BlazorServerFunctions.Generator.UnitTests;

public static class ModuleInitializer
{
    private const string SnapshotDirectory = "_snapshots";
    private static string? _cachedRoot;

    [ModuleInitializer]
    public static void Initialize()
    {
        DerivePathInfo((file, _, type, method) =>
        {
            _cachedRoot ??= FindProjectRoot(file);

            return new(
                Path.Combine(_cachedRoot, SnapshotDirectory),
                type.Name,
                method.Name);
        });


        VerifySourceGenerators.Initialize();
    }

    private static string FindProjectRoot(string file)
    {
        string? dir = Path.GetDirectoryName(file);
    
        if (dir is null)
            return Path.Join(Path.GetDirectoryName(file), SnapshotDirectory);
        
        while (dir is not null)
        {
            if (Directory.EnumerateFiles(dir, "*.csproj").Any())
                return dir;
    
            dir = Directory.GetParent(dir)?.FullName;
        }

        return dir!;
    }

}