namespace BlazorServerFunctions.Generator.IntegrationTests;

public static class TestHelpers
{
    public static IEnumerable<string> GetProjectFiles(string projectName)
    {
        string solutionRoot = FindSolutionRoot();
        string? projectDir = Directory.GetDirectories(
                Path.Combine(solutionRoot, "samples"),
                projectName,
                SearchOption.AllDirectories)
            .FirstOrDefault();

        if (projectDir is null)
            throw new DirectoryNotFoundException($"Could not find project directory for {projectName}.");

        return Directory.EnumerateFiles(projectDir, "*.cs", SearchOption.AllDirectories);
    }

    private static string FindSolutionRoot()
    {
        string dir = Directory.GetCurrentDirectory();

        while (!Directory.EnumerateFiles(dir, "*.slnx").Any())
        {
            DirectoryInfo? parent = Directory.GetParent(dir);
            if (parent is null)
                throw new DirectoryNotFoundException("Could not find solution root (.slnx).");

            dir = parent.FullName;
        }

        return dir;
    }
}