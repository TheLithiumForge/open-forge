namespace OpenForge.Cli;

internal static class Program
{
    public static Task<int> Main(string[] args)
    {
        return CliApplication.RunAsync(args);
    }
}
