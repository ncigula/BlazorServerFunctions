namespace BlazorServerFunctions.Generator.IntegrationTests._old;

public static class TestHelpers
{
    public static IEnumerable<string> GetProjectFiles(string projectName)
    {
        string solutionRoot = FindSolutionRoot();

        // Search both src and samples
        var searchRoots = new[]
        {
            Path.Combine(solutionRoot, "src"),
            Path.Combine(solutionRoot, "samples")
        };

        foreach (var root in searchRoots)
        {
            var projectDir = Directory.GetDirectories(
                    root,
                    projectName,
                    SearchOption.AllDirectories)
                .FirstOrDefault();

            if (projectDir != null)
                return Directory.EnumerateFiles(projectDir, "*.cs", SearchOption.AllDirectories);
        }

        throw new DirectoryNotFoundException($"Could not find project directory for {projectName}.");
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