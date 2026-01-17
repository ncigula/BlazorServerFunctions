using System.Runtime.CompilerServices;

namespace BlazorServerFunctions.Generator.IntegrationTests;

public static class TestHelpers
{
    public static string GetSolutionPath()
    {
        var currentFilePath = GetCurrentFilePath();

        var solutionRoot =
            Path.GetDirectoryName(
                Path.GetDirectoryName(
                    Path.GetDirectoryName(currentFilePath)));
        
        return solutionRoot!;
    }

    public static string GetProjectPath(string projectDirectoryName)
    {
        var solutionRoot = GetSolutionPath();

        var projectPath = Path.Combine(solutionRoot!, "samples", projectDirectoryName);

        return projectPath;
    }
    
    public static IEnumerable<string> GetProjectFiles(string projectDirectoryName)
    {
        var projectPath = GetProjectPath(projectDirectoryName);

        return Directory.EnumerateFiles(projectPath, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                        && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));
    }

    private static string GetCurrentFilePath([CallerFilePath] string path = "") => path;
}