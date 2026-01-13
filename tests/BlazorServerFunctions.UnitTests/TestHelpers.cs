using System.Runtime.CompilerServices;

namespace BlazorServerFunctions.UnitTests;

public static class TestHelpers
{
    public static IEnumerable<string> GetProjectFiles(string projectDirectoryName)
    {
        var currentFilePath = GetCurrentFilePath();

        var solutionRoot =
            Path.GetDirectoryName(
                Path.GetDirectoryName(
                    Path.GetDirectoryName(currentFilePath)));

        var projectPath = Path.Combine(solutionRoot!, "samples", projectDirectoryName);

        return Directory.EnumerateFiles(projectPath, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                        && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));
    }

    private static string GetCurrentFilePath([CallerFilePath] string path = "") => path;
}