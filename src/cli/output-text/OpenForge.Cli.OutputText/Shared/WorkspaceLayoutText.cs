namespace OpenForge.Cli.OutputText.Shared;

public static class WorkspaceLayoutText
{
    public static string Describe(string mainRoot, string loaderPath)
        => $"Context folder: {mainRoot}\nLoader: {loaderPath}";
}
